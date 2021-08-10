using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Security.Cryptography;
using MelonLoader;
using UnhollowerRuntimeLib;
using VRChatUtilityKit.Components;

using ScreenshotManager;
using ScreenshotManager.Config;
using ScreenshotManager.Core;
using ScreenshotManager.Tasks;

[assembly: MelonInfo(typeof(ScreenshotManagerMod), "ScreenshotManager", "1.2.2", "DragonPlayer", "https://github.com/DragonPlayerX/ScreenshotManager")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("UI Expansion Kit")]

namespace ScreenshotManager
{
    public class ScreenshotManagerMod : MelonMod
    {
        public static readonly string Version = "1.2.2";

        public static ScreenshotManagerMod Instance { get; private set; }

        public override void OnApplicationStart()
        {
            Instance = this;
            MelonLogger.Msg("Initializing ScreenshotManager " + Version + "...");

            if (MelonHandler.Mods.Any(mod => mod.Info.Name == "PhotoOrganization"))
            {
                MenuManager.ShowPhotoOrganizationWarning = true;
                MelonLogger.Warning("\n\n\n" +
                "PhotoOrganization was found.\n" +
                "This mod is redundant when using ScreenshotManager because it has its own file organization features.\n" +
                "I not recommend using them at the same time!\n\n");
            }

            Configuration.Init();

            if (!File.Exists("Executables/DiscordWebhook.exe"))
            {
                if (!Directory.Exists("Executables"))
                    Directory.CreateDirectory("Executables");
                ExtractResource();
            }
            else
            {
                MelonLogger.Msg("Validating checksums of external resources...");
                if (!CompareChecksums())
                    ExtractResource();
            }

            if (!Directory.Exists(Configuration.ScreenshotDirectoryEntry.Value))
                Directory.CreateDirectory(Configuration.ScreenshotDirectoryEntry.Value);

            FileOrganization.PatchMethod();
            if (Configuration.FileOrganizationEntry.Value)
                FileOrganization.OrganizeAll();

            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonCoroutines.Start(Init());
        }

        private IEnumerator Init()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null) yield return null;

            MenuManager.PrepareAssets();
            MenuManager.CreateMenus();

            ImageHandler.Init();
            ImageHandler.ReloadFiles().NoAwait();

            MelonLogger.Msg("Running version " + Version + " of ScreenshotManager.");
        }

        public override void OnUpdate()
        {
            TaskProvider.mainThreadQueue.Dequeue();
        }

        private static bool CompareChecksums()
        {
            Stream internalResource = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenshotManager.DiscordWebhook.exe");
            Stream externalResource = new FileStream("Executables/DiscordWebhook.exe", FileMode.Open, FileAccess.Read);

            SHA256 sha256 = SHA256.Create();
            string internalHash = BytesToString(sha256.ComputeHash(internalResource));
            string externalHash = BytesToString(sha256.ComputeHash(externalResource));

            internalResource.Close();
            externalResource.Close();

            MelonLogger.Msg("Internal Hash: " + internalHash);
            MelonLogger.Msg("Existing Hash: " + externalHash);

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

        private static void ExtractResource()
        {
            MelonLogger.Msg("Extracting DiscordWebhook.exe...");
            try
            {
                Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenshotManager.DiscordWebhook.exe");
                FileStream file = new FileStream("Executables/DiscordWebhook.exe", FileMode.Create, FileAccess.Write);
                resource.CopyTo(file);
                resource.Close();
                file.Close();
                MelonLogger.Msg("Successfully extracted DiscordWebhook.exe");
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
            }
        }
    }
}
