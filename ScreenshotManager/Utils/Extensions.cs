using System;
using UnityEngine;

namespace ScreenshotManager.Utils
{

    public static class Extensions
    {

        public static float Clamp(this float value, int range)
        {
            if (value > range)
                return range;
            else if (value < -range)
                return -range;
            else
                return value;
        }

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
                    ScreenshotManagerMod.Logger.Error("Error while invoking delegate:\n" + ex.ToString());
                }
            }
        }
    }
}
