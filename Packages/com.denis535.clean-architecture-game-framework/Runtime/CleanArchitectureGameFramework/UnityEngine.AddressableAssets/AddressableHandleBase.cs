﻿#nullable enable
namespace UnityEngine.AddressableAssets {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public abstract class AddressableHandleBase {

        // Handle
        public abstract bool IsValid { get; }
        public abstract bool IsDone { get; }
        public abstract bool IsSucceeded { get; }
        public abstract bool IsFailed { get; }
        public abstract Exception? Exception { get; }

        // Constructor
        public AddressableHandleBase() {
            //ResourceManager.ExceptionHandler = CustomExceptionHandler;
        }

        // Heleprs
        protected void Assert_IsValid() {
            Assert.Operation.Message( $"AddressableHandle {this} must be valid" ).Valid( IsValid );
        }
        protected void Assert_IsSucceeded() {
            Assert.Operation.Message( $"AddressableHandle {this} must be succeeded" ).Valid( IsSucceeded );
        }
        protected void Assert_IsNotValid() {
            Assert.Operation.Message( $"AddressableHandle {this} is already valid" ).Valid( !IsValid );
        }

    }
    public abstract class AddressableHandle : AddressableHandleBase {

        // Key
        public string Key { get; }

        // Constructor
        public AddressableHandle(string key) {
            Key = key;
        }

        // Utils
        public override string ToString() {
            return "AddressableHandle: " + Key;
        }

    }
    public abstract class DynamicAddressableHandle : AddressableHandleBase {

        // Key
        public string? Key { get; protected set; }

        // Constructor
        public DynamicAddressableHandle(string? key) {
            Key = key;
        }

        // Utils
        public override string ToString() {
            if (Key != null) {
                return "DynamicAddressableHandle: " + Key;
            } else {
                return "DynamicAddressableHandle";
            }
        }

    }
}