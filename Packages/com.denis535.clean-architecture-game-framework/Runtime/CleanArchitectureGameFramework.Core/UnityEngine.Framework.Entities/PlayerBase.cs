#nullable enable
namespace UnityEngine.Framework.Entities {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [DefaultExecutionOrder( ScriptExecutionOrders.Player )]
    public abstract class PlayerBase : MonoBehaviour {

        // Awake
        public abstract void Awake();
        public abstract void OnDestroy();

    }
}
