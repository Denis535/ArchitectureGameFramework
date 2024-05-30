#nullable enable
namespace CleanArchitectureGameFramework {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;

    public static class Toolbar {

        // TakeScreenshot
        [MenuItem( "Tools/Clean Architecture Game Framework/Take Screenshot (Game) _F12", priority = 0 )]
        internal static void TakeScreenshot_Game() {
            var path = $"Screenshots/{Application.productName}-{DateTime.UtcNow.Ticks}.png";
            ScreenCapture.CaptureScreenshot( path, 1 );
            EditorApplication.Beep();
            EditorUtility.RevealInFinder( path );
        }
        [MenuItem( "Tools/Clean Architecture Game Framework/Take Screenshot (Editor) &F12", priority = 1 )]
        internal static void TakeScreenshot_Editor() {
            var position = EditorGUIUtility.GetMainWindowPosition();
            var texture = new Texture2D( (int) position.width, (int) position.height );
            texture.SetPixels( InternalEditorUtility.ReadScreenPixel( position.position, (int) position.width, (int) position.height ) );
            var bytes = texture.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate( texture );

            var path = $"Screenshots/{Application.productName}-{DateTime.UtcNow.Ticks}.png";
            Directory.CreateDirectory( Path.GetDirectoryName( path ) );
            File.WriteAllBytes( path, bytes );
            EditorApplication.Beep();
            EditorUtility.RevealInFinder( path );
        }

        // OpenAll
        [MenuItem( "Tools/Clean Architecture Game Framework/Open All (CSharp)", priority = 100 )]
        public static void OpenAll_CSharp() {
            OpenAssetsReverse( "Assets/(*.cs)" );
        }
        [MenuItem( "Tools/Clean Architecture Game Framework/Open All (UITheme)", priority = 101 )]
        public static void OpenAll_UITheme() {
            OpenAssetsReverse( "Assets/(*ThemeBase.cs|*Theme.cs)" );
        }
        [MenuItem( "Tools/Clean Architecture Game Framework/Open All (UIScreen)", priority = 102 )]
        public static void OpenAll_UIScreen() {
            OpenAssetsReverse( "Assets/(*ScreenBase.cs|*Screen.cs)" );
        }
        [MenuItem( "Tools/Clean Architecture Game Framework/Open All (UIWidget)", priority = 103 )]
        public static void OpenAll_UIWidget() {
            OpenAssetsReverse( "Assets/(*WidgetBase.cs|*Widget.cs)" );
        }
        [MenuItem( "Tools/Clean Architecture Game Framework/Open All (UIView)", priority = 104 )]
        public static void OpenAll_UIView() {
            OpenAssetsReverse( "Assets/(*ViewBase.cs|*View.cs)" );
        }

        // Helpers
        private static void OpenAssets(params string[] patterns) {
            foreach (var path in GetPaths().GetMatches( patterns )) {
                if (!Path.GetFileName( path ).StartsWith( "_" )) {
                    AssetDatabase.OpenAsset( AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path ) );
                    Thread.Sleep( 100 );
                }
            }
        }
        private static void OpenAssetsReverse(params string[] patterns) {
            foreach (var path in GetPaths().GetMatches( patterns ).Reverse()) {
                if (!Path.GetFileName( path ).StartsWith( "_" )) {
                    AssetDatabase.OpenAsset( AssetDatabase.LoadAssetAtPath<UnityEngine.Object>( path ) );
                    Thread.Sleep( 100 );
                }
            }
        }
        // Helpers
        private static string[] GetPaths() {
            var path = Directory.GetCurrentDirectory();
            return GetPaths( path )
                .Select( i => Path.GetRelativePath( path, i ) )
                .Select( i => i.Replace( '\\', '/' ) )
                .ToArray();
        }
        private static IEnumerable<string> GetPaths(string path) {
            var files = Directory.EnumerateFiles( path )
                .OrderBy( i => i );
            var directories = Directory.EnumerateDirectories( path )
                .OrderBy( i => !i.EndsWith( ".UI" ) )
                .ThenBy( i => !i.EndsWith( ".UI.MainScreen" ) )
                .ThenBy( i => !i.EndsWith( ".UI.GameScreen" ) )
                .ThenBy( i => !i.EndsWith( ".UI.Common" ) )
                .ThenBy( i => !i.EndsWith( ".App" ) )
                .ThenBy( i => !i.EndsWith( ".Entities" ) )
                .ThenBy( i => !i.EndsWith( ".Entities.Characters" ) )
                .ThenBy( i => !i.EndsWith( ".Entities.Worlds" ) )
                .ThenBy( i => !i.EndsWith( ".Entities.Common" ) )
                .ThenBy( i => !i.EndsWith( ".Common" ) )
                .ThenBy( i => i );
            foreach (var file in files) {
                yield return file;
            }
            foreach (var dir in directories) {
                yield return dir;
                foreach (var i in GetPaths( dir )) yield return i;
            }
        }
        // Helpers
        private static IEnumerable<string> GetMatches(this string[] paths, string[] patterns) {
            foreach (var pattern in patterns) {
                var regex = new Regex( "^" + pattern.Replace( " ", "" ).Replace( "*", "(.*?)" ) + "$", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace );
                foreach (var path in paths.Where( path => regex.IsMatch( path ) )) {
                    yield return path;
                }
            }
        }

    }
}
