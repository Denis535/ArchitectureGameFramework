﻿#nullable enable
namespace UnityEngine {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public static class ComponentExtensions {

        // Require/Transform
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Transform Require(this Transform transform, string path) {
            var result = transform.Find( path );
            Assert.Operation.Message( $"Transform {path} was not found" ).Valid( result != null );
            return result;
        }

        // Check
        public static void Check(this MonoBehaviour component) {
            Assert.Object.Message( $"Component {component} must be awakened" ).Initialized( component.didAwake );
            Assert.Object.Message( $"Component {component} must be alive" ).Alive( component );
        }

    }
}
