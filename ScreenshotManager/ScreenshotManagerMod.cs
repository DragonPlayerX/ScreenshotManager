using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using MelonLoader;
using UnhollowerRuntimeLib;
using VRChatUtilityKit.Components;

using ScreenshotManager;
using ScreenshotManager.Config;
using ScreenshotManager.Core;

[assembly: MelonInfo(typeof(ScreenshotManagerMod), "ScreenshotManager", "1.0.1", "DragonPlayer", "https://github.com/DragonPlayerX/ScreenshotManager")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ScreenshotManager
{
    public class ScreenshotManagerMod : MelonMod
    {
        public static readonly string Version = "1.0.1";

        public static ScreenshotManagerMod Instance;

        public static Queue<Action> pendingActions = new Queue<Action>();

        public override void OnApplicationStart()
        {
            Instance = this;
            MelonLogger.Msg("Initializing ScreenshotManager " + Version + "...");

            Configuration.Init();

            if (!File.Exists("Executables/DiscordWebhook.exe"))
            {
                if (!Directory.Exists("Executables"))
                {
                    Directory.CreateDirectory("Executables");
                }
                ExtractResource();
            }
            else
            {
                MelonLogger.Msg("Validating checksums of external resources...");
                if (!CompareChecksums())
                {
                    ExtractResource();
                }
            }

            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonCoroutines.Start(Init());
        }

        private IEnumerator Init()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;

            MenuManager.PrepareAssets();
            MenuManager.CreateMenus();

            ImageHandler.Init();
            ImageHandler.Reload();

            MelonLogger.Msg("Running version " + Version + " of ScreenshotManager.");
        }

        public static void Enqueue(Action action)
        {
            pendingActions.Enqueue(action);
        }

        public override void OnUpdate()
        {
            if (pendingActions.Count > 0)
            {
                List<Action> actions = pendingActions.ToList();
                pendingActions.Clear();

                foreach (Action action in actions)
                {
                    if (action != null)
                    {
                        action.Invoke();
                    }
                }
            }
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
