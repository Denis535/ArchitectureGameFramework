#nullable enable
namespace UnityEngine.Framework {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class ProgramBase2 : ProgramBase, IDependencyContainer {

        // GetValue
        Option<object?> IDependencyContainer.GetValue(Type type, object? argument) => GetValue( type, argument );
        // GetValue
        protected abstract Option<object?> GetValue(Type type, object? argument);

    }
    public abstract partial class ProgramBase2<TTheme, TScreen, TRouter, TApplication, TGame> : ProgramBase2
        where TTheme : notnull, UIThemeBase
        where TScreen : notnull, UIScreenBase
        where TRouter : notnull, UIRouterBase
        where TApplication : notnull, ApplicationBase
        where TGame : notnull, GameBase {

        // Framework
        protected abstract TTheme Theme { get; set; }
        protected abstract TScreen Screen { get; set; }
        protected abstract TRouter Router { get; set; }
        protected abstract TApplication Application { get; set; }
        protected abstract TGame? Game { get; }

        // Awake
        protected override void Awake() {
            base.Awake();
        }
        protected override void OnDestroy() {
            base.OnDestroy();
        }

#if UNITY_EDITOR
        // OnInspectorGUI
        protected internal override void OnInspectorGUI() {
            OnInspectorGUI( Theme, Screen, Router, Application, Game );
        }
        protected override void OnInspectorGUI(UIThemeBase theme, UIScreenBase screen, UIRouterBase router, ApplicationBase application, GameBase? game) {
            base.OnInspectorGUI( theme, screen, router, application, game );
        }
#endif

    }
}