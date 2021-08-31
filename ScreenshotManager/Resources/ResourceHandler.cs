using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using MelonLoader;

namespace ScreenshotManager.Resources
{
    public static class ResourceHandler
    {
        public static void ExtractResource(string resourceName, string destination)
        {
            MelonLogger.Msg("Extracting " + resourceName + "...");
            try
            {
                Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenshotManager.Resources." + resourceName);
                FileStream file = new FileStream(destination + "/" + resourceName, FileMode.Create, FileAccess.Write);
                resource.CopyTo(file);
                resource.Close();
                file.Close();
                MelonLogger.Msg("Successfully extracted " + resourceName);
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
            }
        }

        public static bool CompareChecksums(string resourceName, string externalPath)
        {
            MelonLogger.Msg("Validating checksums of external resource [" + resourceName + "]...");

            Stream internalResource = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenshotManager.Resources." + resourceName);
            Stream externalResource = new FileStream(externalPath + "/" + resourceName, FileMode.Open, FileAccess.Read);

            SHA256 sha256 = SHA256.Create();
            string internalHash = BytesToString(sha256.ComputeHash(internalResource));
            string externalHash = BytesToString(sha256.ComputeHash(externalResource));

            internalResource.Close();
            externalResource.Close();

            MelonLogger.Msg("Internal Hash: " + internalHash);
            MelonLogger.Msg("External Hash: " + externalHash);

            return internalHash.Equals(externalHash);
        }

        private static string BytesToString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
