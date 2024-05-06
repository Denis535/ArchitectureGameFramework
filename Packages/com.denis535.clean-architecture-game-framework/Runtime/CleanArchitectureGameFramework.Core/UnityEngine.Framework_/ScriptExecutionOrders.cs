#nullable enable
namespace UnityEngine.Framework {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class ScriptExecutionOrders {

        // Program
        public const int Program      = 10_000;
        // UI
        public const int UIAudioTheme = 11_000;
        public const int UIScreen     = 11_100;
        public const int UIRouter     = 11_200;
        // Application
        public const int Application  = 12_000;
        // Game
        public const int Game         = 13_000;
        public const int Player       = 13_100;
        public const int Level        = 13_200;
        public const int World        = 13_300;
        public const int Entity       = 13_400;

    }
}
