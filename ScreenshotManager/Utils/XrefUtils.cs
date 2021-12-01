using System;
using System.Reflection;
using UnhollowerRuntimeLib.XrefScans;

namespace ScreenshotManager.Utils
{
    public static class XrefUtils
    {
        public static bool CheckForString(MethodBase method, string content)
        {
            foreach (XrefInstance instance in XrefScanner.XrefScan(method))
            {
                if (instance.Type == XrefType.Global && instance.ReadAsObject()?.ToString() == content)
                    return true;
            }
            return false;
        }

        public static bool CheckForUsingMethods(MethodBase method, string methodName)
        {
            foreach (XrefInstance instance in XrefScanner.XrefScan(method))
            {
                if (instance.Type == XrefType.Method && instance.TryResolve() != null && instance.TryResolve().Name.Contains(methodName))
                    return true;
            }
            return false;
        }

        public static bool CheckUsedByType(MethodBase method, Type type)
        {
            foreach (XrefInstance instance in XrefScanner.UsedBy(method))
            {
                if (instance.Type == XrefType.Method && instance.TryResolve().DeclaringType == type)
                    return true;
            }
            return false;
        }
    }
}