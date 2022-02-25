using System.IO;
using System.Collections;
using UnityEngine;
using MelonLoader;
using UnhollowerRuntimeLib;

using ScreenshotManager;
using ScreenshotManager.Config;
using ScreenshotManager.Core;
using ScreenshotManager.Tasks;
using ScreenshotManager.Resources;
using ScreenshotManager.UI;
using ScreenshotManager.UI.Components;

[assembly: MelonInfo(typeof(ScreenshotManagerMod), "ScreenshotManager", "2.3.3", "DragonPlayer, Xavi", "https://github.com/DragonPlayerX/ScreenshotManager")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("LagFreeScreenshots")]

namespace ScreenshotManager
{
    public class ScreenshotManagerMod : MelonMod
    {

        public static readonly string Version = "2.3.3";

        public static ScreenshotManagerMod Instance { get; private set; }

        public override void OnApplicationStart()
        {
            Instance = this;
            MelonLogger.Msg("Initializing ScreenshotManager " + Version + "...");

            if (!Directory.Exists("UserData/ScreenshotManager/DiscordWebhooks"))
                Directory.CreateDirectory("UserData/ScreenshotManager/DiscordWebhooks");

            Configuration.Init();

            if (!File.Exists("Executables/DiscordWebhook.exe"))
            {
                if (!Directory.Exists("Executables"))
                    Directory.CreateDirectory("Executables");
                ResourceHandler.ExtractResource("DiscordWebhook.exe", "Executables");
            }
            else
            {
                if (!ResourceHandler.CompareChecksums("DiscordWebhook.exe", "Executables"))
                    ResourceHandler.ExtractResource("DiscordWebhook.exe", "Executables");
            }

            if (!Directory.Exists(Configuration.ScreenshotDirectory.Value))
                Directory.CreateDirectory(Configuration.ScreenshotDirectory.Value);

            FileOrganization.PatchMethod();
            if (Configuration.FileOrganization.Value)
                FileOrganization.OrganizeAll();

            FileDataHandler.Init();

            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();

            MelonCoroutines.Start(Init());
        }

        private IEnumerator Init()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null) yield return null;
            while (GameObject.Find("UserInterface").transform.Find("Canvas_QuickMenu(Clone)/Container/Window/QMParent") == null) yield return null;

            UiManager.UiInit();

            MenuManager.Init();
            MenuManager.ReloadDiscordWebhookButtons();

            ImageHandler.Init();
            ImageHandler.ReloadFiles().NoAwait();

            SteamIntegration.Init();

            MelonLogger.Msg("Running version " + Version + " of ScreenshotManager.");
        }

        public override void OnUpdate() => TaskProvider.mainThreadQueue.Dequeue();
    }
}
