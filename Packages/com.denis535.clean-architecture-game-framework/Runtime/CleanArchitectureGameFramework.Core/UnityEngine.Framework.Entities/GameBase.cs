#nullable enable
namespace UnityEngine.Framework.Entities {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [DefaultExecutionOrder( ScriptExecutionOrders.Game )]
    public abstract class GameBase : MonoBehaviour {

        // Awake
        public abstract void Awake();
        public abstract void OnDestroy();

    }
}
