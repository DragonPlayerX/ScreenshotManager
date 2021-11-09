using System;
using UnhollowerBaseLib.Attributes;
using UnityEngine;

using ScreenshotManager.Utils;

namespace ScreenshotManager.UI.Components
{

    public class EnableDisableListener : MonoBehaviour
    {

        [method: HideFromIl2Cpp]
        public event Action OnEnableEvent;

        [method: HideFromIl2Cpp]
        public event Action OnDisableEvent;

        public EnableDisableListener(IntPtr value) : base(value) { }

        internal void OnEnable() => OnEnableEvent?.DelegateSafeInvoke();
        internal void OnDisable() => OnDisableEvent?.DelegateSafeInvoke();
    }
}
