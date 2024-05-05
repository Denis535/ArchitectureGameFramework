﻿#nullable enable
namespace UnityEngine.Framework.App {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [DefaultExecutionOrder( ScriptExecutionOrders.Application )]
    public abstract class ApplicationBase : MonoBehaviour {

        // Awake
        public abstract void Awake();
        public abstract void OnDestroy();

    }
}
