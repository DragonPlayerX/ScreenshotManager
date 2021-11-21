using System;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace ScreenshotManager.Utils
{
    public static class XrefUtils
    {
        public static bool CheckUsedBy(MethodBase method, string methodName, Type type)
        {
            foreach (XrefInstance instance in XrefScanner.UsedBy(method))
                if (instance.Type == XrefType.Method)
                {
                    MethodBase methodBase = instance.TryResolve();
                    if (methodBase != null && methodBase.DeclaringType == type && methodBase.Name.Contains(methodName))
                        return true;
                }
            return false;
        }
    }
}