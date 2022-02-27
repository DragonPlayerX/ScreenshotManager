using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ScreenshotManager.Resources
{
    public static class ResourceHandler
    {
        public static void ExtractResource(string resourceName, string destination)
        {
            ScreenshotManagerMod.Logger.Msg("Extracting " + resourceName + "...");
            try
            {
                Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenshotManager.Resources." + resourceName);
                FileStream file = new FileStream(destination + "/" + resourceName, FileMode.Create, FileAccess.Write);
                resource.CopyTo(file);
                resource.Close();
                file.Close();
                ScreenshotManagerMod.Logger.Msg("Successfully extracted " + resourceName);
            }
            catch (Exception e)
            {
                ScreenshotManagerMod.Logger.Error(e);
            }
        }

        public static bool CompareChecksums(string resourceName, string externalPath)
        {
            ScreenshotManagerMod.Logger.Msg("Validating checksums of external resource [" + resourceName + "]...");

            Stream internalResource = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenshotManager.Resources." + resourceName);
            Stream externalResource = new FileStream(externalPath + "/" + resourceName, FileMode.Open, FileAccess.Read);

            SHA256 sha256 = SHA256.Create();
            string internalHash = BytesToString(sha256.ComputeHash(internalResource));
            string externalHash = BytesToString(sha256.ComputeHash(externalResource));

            internalResource.Close();
            externalResource.Close();

            ScreenshotManagerMod.Logger.Msg("Internal Hash: " + internalHash);
            ScreenshotManagerMod.Logger.Msg("External Hash: " + externalHash);

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
