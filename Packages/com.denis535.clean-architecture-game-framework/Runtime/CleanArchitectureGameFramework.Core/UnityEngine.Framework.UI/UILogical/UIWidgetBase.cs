#nullable enable
namespace UnityEngine.Framework.UI {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using UnityEngine;

    public abstract class UIWidgetBase : IUILogicalElement, IDisposable {

        private readonly Lock @lock = new Lock();
        protected CancellationTokenSource? disposeCancellationTokenSource;

        // System
        public bool IsDisposed { get; protected set; }
        public CancellationToken DisposeCancellationToken {
            get {
                if (disposeCancellationTokenSource == null) {
                    disposeCancellationTokenSource = new CancellationTokenSource();
                    if (IsDisposed) disposeCancellationTokenSource.Cancel();
                }
                return disposeCancellationTokenSource.Token;
            }
        }
        public bool DisposeWhenDetach { get; protected init; } = true;
        // View
        [MemberNotNullWhen( true, "View" )] public bool IsViewable => this is IUIViewable;
        public UIViewBase? View => (this as IUIViewable)?.View;
        // Screen
        public UIScreenBase? Screen { get; private set; }
        public UIWidgetState State { get; private set; } = UIWidgetState.Unattached;
        [MemberNotNullWhen( true, "Screen" )] public bool IsAttached => State is UIWidgetState.Attached;
        [MemberNotNullWhen( true, "Screen" )] public bool IsAttaching => State is UIWidgetState.Attaching;
        [MemberNotNullWhen( true, "Screen" )] public bool IsDetaching => State is UIWidgetState.Detaching;
        [MemberNotNullWhen( false, "Screen" )] public bool IsNonAttached => State is UIWidgetState.Unattached or UIWidgetState.Detached;
        // Parent
        [MemberNotNullWhen( false, "Parent" )] public bool IsRoot => Parent == null;
        public UIWidgetBase? Parent { get; internal set; }
        public IReadOnlyList<UIWidgetBase> Ancestors => this.GetAncestors();
        public IReadOnlyList<UIWidgetBase> AncestorsAndSelf => this.GetAncestorsAndSelf();
        // Children
        public bool HasChildren => Children_.Any();
        private List<UIWidgetBase> Children_ { get; } = new List<UIWidgetBase>();
        public IReadOnlyList<UIWidgetBase> Children => Children_;
        public IReadOnlyList<UIWidgetBase> Descendants => this.GetDescendants();
        public IReadOnlyList<UIWidgetBase> DescendantsAndSelf => this.GetDescendantsAndSelf();
        // OnAttach
        public event Action<object?>? OnBeforeAttachEvent;
        public event Action<object?>? OnAfterAttachEvent;
        public event Action<object?>? OnBeforeDetachEvent;
        public event Action<object?>? OnAfterDetachEvent;
        // OnDescendantAttach
        public event Action<UIWidgetBase, object?>? OnBeforeDescendantAttachEvent;
        public event Action<UIWidgetBase, object?>? OnAfterDescendantAttachEvent;
        public event Action<UIWidgetBase, object?>? OnBeforeDescendantDetachEvent;
        public event Action<UIWidgetBase, object?>? OnAfterDescendantDetachEvent;

        // Constructor
        public UIWidgetBase() {
        }
        public virtual void Dispose() {
            Assert.Object.Message( $"Widget {this} must be alive" ).Alive( !IsDisposed );
            Assert.Operation.Message( $"Widget {this} must be non-attached" ).Valid( IsNonAttached );
            foreach (var child in Children) {
                child.Dispose();
            }
            Assert.Operation.Message( $"Widget {this} children must be disposed" ).Valid( Children.All( i => i.IsDisposed ) );
            IsDisposed = true;
            disposeCancellationTokenSource?.Cancel();
        }

        // OnAttach
        public virtual void OnBeforeAttach(object? argument) {
            OnBeforeAttachEvent?.Invoke( argument );
            Parent?.OnBeforeDescendantAttach( this, argument );
        }
        public abstract void OnAttach(object? argument);
        public virtual void OnAfterAttach(object? argument) {
            Parent?.OnAfterDescendantAttach( this, argument );
            OnAfterAttachEvent?.Invoke( argument );
        }
        public virtual void OnBeforeDetach(object? argument) {
            OnBeforeDetachEvent?.Invoke( argument );
            Parent?.OnBeforeDescendantDetach( this, argument );
        }
        public abstract void OnDetach(object? argument);
        public virtual void OnAfterDetach(object? argument) {
            Parent?.OnAfterDescendantDetach( this, argument );
            OnAfterDetachEvent?.Invoke( argument );
        }

        // OnDescendantAttach
        public virtual void OnBeforeDescendantAttach(UIWidgetBase descendant, object? argument) {
            OnBeforeDescendantAttachEvent?.Invoke( descendant, argument );
            Parent?.OnBeforeDescendantAttach( descendant, argument );
        }
        public virtual void OnAfterDescendantAttach(UIWidgetBase descendant, object? argument) {
            Parent?.OnAfterDescendantAttach( descendant, argument );
            OnAfterDescendantAttachEvent?.Invoke( descendant, argument );
        }
        public virtual void OnBeforeDescendantDetach(UIWidgetBase descendant, object? argument) {
            OnBeforeDescendantDetachEvent?.Invoke( descendant, argument );
            Parent?.OnBeforeDescendantDetach( descendant, argument );
        }
        public virtual void OnAfterDescendantDetach(UIWidgetBase descendant, object? argument) {
            Parent?.OnAfterDescendantDetach( descendant, argument );
            OnAfterDescendantDetachEvent?.Invoke( descendant, argument );
        }

        // AttachChild
        protected internal virtual void __AttachChild__(UIWidgetBase child, object? argument) {
            // You can override it but you should not directly call this method
            Assert.Argument.Message( $"Argument 'child' must be non-null" ).NotNull( child != null );
            Assert.Operation.Message( $"Widget {this} must have no child {child} widget" ).Valid( !Children.Contains( child ) );
            using (@lock.Enter()) {
                Children_.Add( child );
                child.Parent = this;
                if (IsAttached) {
                    AttachToScreen( child, Screen, argument );
                } else {
                    Assert.Operation.Message( $"You are trying to attach child {child} with argument {argument}, but widget {this} must be attached" ).Valid( argument == null );
                }
            }
        }
        protected internal virtual void __DetachChild__(UIWidgetBase child, object? argument) {
            // You can override it but you should not directly call this method
            Assert.Argument.Message( $"Argument 'child' must be non-null" ).NotNull( child != null );
            Assert.Operation.Message( $"Widget {this} must have child {child} widget" ).Valid( Children.Contains( child ) );
            using (@lock.Enter()) {
                if (IsAttached) {
                    DetachFromScreen( child, Screen, argument );
                } else {
                    Assert.Operation.Message( $"You are trying to detach child {child} with argument {argument}, but widget {this} must be attached" ).Valid( argument == null );
                }
                child.Parent = null;
                Children_.Remove( child );
            }
            if (child.DisposeWhenDetach) {
                child.Dispose();
            }
        }

        // ShowDescendantWidget
        protected virtual void ShowDescendantWidget(UIWidgetBase widget) {
            Assert.Operation.Message( $"Can not show descendant widget: {widget}" ).Valid( Parent != null );
            Parent.ShowDescendantWidget( widget );
        }
        protected virtual void HideDescendantWidget(UIWidgetBase widget) {
            Assert.Operation.Message( $"Can not hide descendant widget: {widget}" ).Valid( Parent != null );
            Parent.HideDescendantWidget( widget );
        }

        // Helpers
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void AttachToScreen(UIWidgetBase widget, UIScreenBase screen, object? argument) {
            Assert.Argument.Message( $"Argument 'widget' must be non-null" ).NotNull( widget != null );
            Assert.Argument.Message( $"Argument 'widget' {widget} must be non-attached" ).Valid( widget.IsNonAttached );
            Assert.Argument.Message( $"Argument 'widget' {widget} must be valid" ).Valid( widget.Screen == null );
            Assert.Argument.Message( $"Argument 'screen' must be non-null" ).NotNull( screen is not null );
            widget.OnBeforeAttach( argument );
            {
                widget.Screen = screen;
                widget.State = UIWidgetState.Attaching;
                {
                    widget.OnAttach( argument );
                    widget.Parent?.ShowDescendantWidget( widget );
                    foreach (var child in widget.Children) {
                        AttachToScreen( child, screen, argument );
                    }
                }
                widget.State = UIWidgetState.Attached;
            }
            widget.OnAfterAttach( argument );
        }
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal static void DetachFromScreen(UIWidgetBase widget, UIScreenBase screen, object? argument) {
            Assert.Argument.Message( $"Argument 'widget' must be non-null" ).NotNull( widget != null );
            Assert.Argument.Message( $"Argument 'widget' {widget} must be attached" ).Valid( widget.IsAttached );
            Assert.Argument.Message( $"Argument 'widget' {widget} must be valid" ).Valid( widget.Screen != null );
            Assert.Argument.Message( $"Argument 'widget' {widget} must be valid" ).Valid( widget.Screen == screen );
            Assert.Argument.Message( $"Argument 'screen' must be non-null" ).NotNull( screen is not null );
            widget.OnBeforeDetach( argument );
            {
                widget.State = UIWidgetState.Detaching;
                {
                    foreach (var child in widget.Children.Reverse()) {
                        DetachFromScreen( child, screen, argument );
                    }
                    widget.Parent?.HideDescendantWidget( widget );
                    widget.OnDetach( argument );
                }
                widget.State = UIWidgetState.Detached;
                widget.Screen = null;
            }
            widget.OnAfterDetach( argument );
        }

    }
    public abstract class UIWidgetBase<TView> : UIWidgetBase, IUIViewable where TView : notnull, UIViewBase {

        // View
        public new TView View { get; protected init; } = default!;
        UIViewBase IUIViewable.View => View;

        // Constructor
        public UIWidgetBase() {
        }
        public UIWidgetBase(TView view) {
            View = view;
        }
        public override void Dispose() {
            Assert.Object.Message( $"Widget {this} must be alive" ).Alive( !IsDisposed );
            Assert.Operation.Message( $"Widget {this} must be non-attached" ).Valid( IsNonAttached );
            foreach (var child in Children) {
                child.Dispose();
            }
            Assert.Operation.Message( $"Widget {this} children must be disposed" ).Valid( Children.All( i => i.IsDisposed ) );
            View.Dispose();
            IsDisposed = true;
            disposeCancellationTokenSource?.Cancel();
        }

    }
}
