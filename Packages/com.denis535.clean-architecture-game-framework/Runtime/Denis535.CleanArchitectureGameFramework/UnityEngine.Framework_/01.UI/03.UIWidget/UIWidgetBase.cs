#nullable enable
namespace UnityEngine.Framework {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.TreeMachine;
    using UnityEngine;

    public abstract class UIWidgetBase : NodeBase3<UIWidgetBase>, IDisposable {

        private CancellationTokenSource? disposeCancellationTokenSource;

        // System
        public bool IsDisposed { get; private set; }
        public CancellationToken DisposeCancellationToken {
            get {
                if (disposeCancellationTokenSource == null) {
                    disposeCancellationTokenSource = new CancellationTokenSource();
                    if (IsDisposed) disposeCancellationTokenSource.Cancel();
                }
                return disposeCancellationTokenSource.Token;
            }
        }
        // Screen
        protected UIScreenBase? Screen {
            get {
                Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
                Assert.Operation.Message( $"Widget {this} must be active or activating or deactivating" ).Valid( Activity is Activity_.Active or Activity_.Activating or Activity_.Deactivating );
                return (UIScreenBase?) Tree;
            }
        }
        // View
        [MemberNotNullWhen( true, "ViewBase", "View" )]
        public virtual bool IsViewable {
            get {
                Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
                return false;
            }
        }
        private protected virtual UIViewBase? ViewBase {
            get {
                Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
                return null;
            }
        }
        protected internal virtual UIViewBase? View {
            get {
                Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
                return ViewBase;
            }
        }

        // Constructor
        public UIWidgetBase() {
        }
        public virtual void Dispose() {
            foreach (var child in Children) {
                Assert.Operation.Message( $"Child {child} must be disposed" ).Valid( child.IsDisposed );
            }
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            Assert.Operation.Message( $"Widget {this} must be inactive" ).Valid( Activity is Activity_.Inactive );
            disposeCancellationTokenSource?.Cancel();
            IsDisposed = true;
        }

        // OnAttach
        protected override void OnBeforeAttach(object? argument) {
            base.OnBeforeAttach( argument );
        }
        protected override void OnAttach(object? argument) {
        }
        protected override void OnAfterAttach(object? argument) {
            base.OnAfterAttach( argument );
        }
        protected override void OnBeforeDetach(object? argument) {
            base.OnBeforeDetach( argument );
        }
        protected override void OnDetach(object? argument) {
        }
        protected override void OnAfterDetach(object? argument) {
            base.OnAfterDetach( argument );
        }

        // OnDescendantAttach
        protected override void OnBeforeDescendantAttach(UIWidgetBase descendant, object? argument) {
        }
        protected override void OnAfterDescendantAttach(UIWidgetBase descendant, object? argument) {
        }
        protected override void OnBeforeDescendantDetach(UIWidgetBase descendant, object? argument) {
        }
        protected override void OnAfterDescendantDetach(UIWidgetBase descendant, object? argument) {
        }

        // AddChild
        protected override void AddChild(UIWidgetBase child, object? argument = null) {
            Assert.Argument.Message( $"Argument 'child' ({child}) must be non-disposed" ).Valid( !child.IsDisposed );
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            base.AddChild( child, argument );
        }
        protected override void RemoveChild(UIWidgetBase child, object? argument = null) {
            Assert.Argument.Message( $"Argument 'child' ({child}) must be non-disposed" ).Valid( !child.IsDisposed );
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            base.RemoveChild( child, argument );
            child.Dispose();
        }

        // ShowView
        protected internal virtual void ShowView(UIViewBase view) {
            Assert.Argument.Message( $"Argument 'view' ({view}) must be non-disposed" ).Valid( !view.IsDisposed );
            Assert.Argument.Message( $"Argument 'view' ({view}) must be non-attached to parent" ).Valid( !view.IsAttachedToParent );
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            var wasShown = TryShowView( view );
            Assert.Operation.Message( $"View {view} was not shown" ).Valid( wasShown );
        }
        protected internal virtual void HideView(UIViewBase view) {
            Assert.Argument.Message( $"Argument 'view' ({view}) must be non-disposed" ).Valid( !view.IsDisposed );
            Assert.Argument.Message( $"Argument 'view' ({view}) must be attached to parent" ).Valid( view.IsAttachedToParent );
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            var wasHidden = TryHideView( view );
            Assert.Operation.Message( $"View {view} was not hidden" ).Valid( wasHidden );
        }

        // TryShowView
        private bool TryShowView(UIViewBase view) {
            Assert.Argument.Message( $"Argument 'view' ({view}) must be non-disposed" ).Valid( !view.IsDisposed );
            Assert.Argument.Message( $"Argument 'view' ({view}) must be non-attached to parent" ).Valid( !view.IsAttachedToParent );
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            if (IsViewable && View.TryAddView( View )) {
                return true;
            }
            return Parent?.TryShowView( view ) ?? false;
        }
        private bool TryHideView(UIViewBase view) {
            Assert.Argument.Message( $"Argument 'view' ({view}) must be non-disposed" ).Valid( !view.IsDisposed );
            Assert.Argument.Message( $"Argument 'view' ({view}) must be attached to parent" ).Valid( view.IsAttachedToParent );
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            if (IsViewable && View.TryRemoveView( View )) {
                return true;
            }
            return Parent?.TryHideView( view ) ?? false;
        }

    }
    public abstract class UIWidgetBase<TView> : UIWidgetBase where TView : notnull, UIViewBase {

        private TView view = default!;

        // View
        public sealed override bool IsViewable {
            get {
                Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
                return true;
            }
        }
        private protected sealed override UIViewBase? ViewBase {
            get {
                Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
                return View;
            }
        }
        protected internal new TView View {
            get {
                Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
                return view;
            }
            init {
                Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
                view = value;
            }
        }

        // Constructor
        public UIWidgetBase() {
        }
        public override void Dispose() {
            foreach (var child in Children) {
                Assert.Operation.Message( $"Child {child} must be disposed" ).Valid( child.IsDisposed );
            }
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            Assert.Operation.Message( $"Widget {this} must be inactive" ).Valid( Activity is Activity_.Inactive );
            Assert.Operation.Message( $"Widget {this} must be released" ).Valid( View.IsDisposed );
            base.Dispose();
        }

        // ShowSelf
        protected virtual void ShowSelf() {
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            Assert.Operation.Message( $"Widget {this} must be non-root" ).NotDisposed( !IsRoot );
            Parent.ShowView( View );
        }
        protected virtual void HideSelf() {
            Assert.Operation.Message( $"Widget {this} must be non-disposed" ).NotDisposed( !IsDisposed );
            Assert.Operation.Message( $"Widget {this} must be non-root" ).NotDisposed( !IsRoot );
            Parent.HideView( View );
        }

    }
}
