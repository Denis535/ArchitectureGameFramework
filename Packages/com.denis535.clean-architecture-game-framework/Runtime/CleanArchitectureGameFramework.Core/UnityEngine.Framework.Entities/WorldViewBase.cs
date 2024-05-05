#nullable enable
namespace UnityEngine.Framework.Entities {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class WorldViewBase : IDisposable {

        // GameObject
        protected abstract GameObject GameObject { get; init; }
        // Transform
        protected Transform Transform => GameObject.transform;

        // Constructor
        public WorldViewBase(GameObject gameObject) {
            GameObject = gameObject;
        }
        public abstract void Dispose();

    }
}
