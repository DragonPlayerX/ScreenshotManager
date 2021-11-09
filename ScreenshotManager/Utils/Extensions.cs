using MelonLoader;
using System;
using UnityEngine;

namespace ScreenshotManager.Utils
{

    public static class Extensions
    {

        public static void SetLayerRecursive(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (var child in gameObject.transform)
                SetLayerRecursive(child.Cast<Transform>().gameObject, layer);
        }

        public static void DelegateSafeInvoke(this Delegate @delegate, params object[] args)
        {
            if (@delegate == null)
                return;

            foreach (Delegate @delegates in @delegate.GetInvocationList())
            {
                try
                {
                    @delegates.DynamicInvoke(args);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error("Error while invoking delegate:\n" + ex.ToString());
                }
            }
        }
    }
}
