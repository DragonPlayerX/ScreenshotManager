using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using UnhollowerRuntimeLib;
using VRChatUtilityKit.Components;

using ScreenshotManager;
using ScreenshotManager.Config;
using ScreenshotManager.Core;
using ScreenshotManager.Tasks;
using ScreenshotManager.Resources;

[assembly: MelonInfo(typeof(ScreenshotManagerMod), "ScreenshotManager", "1.3.0", "DragonPlayer", "https://github.com/DragonPlayerX/ScreenshotManager")]
[assembly: MelonGame("VRChat", "VRChat")]
[assembly: MelonOptionalDependencies("UI Expansion Kit", "ActiveBackground")]

namespace ScreenshotManager
{
    public class ScreenshotManagerMod : MelonMod
    {
        public static readonly string Version = "1.3.0";

        public static ScreenshotManagerMod Instance { get; private set; }

        public MelonMod ActiveBackgroundMod;

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

            if (MelonHandler.Mods.Any(mod => mod.Info.Name == "ActiveBackground"))
                PatchActiveBackground();

            if (!Directory.Exists("UserData/ScreenshotManager/DiscordWebhooks"))
            {
                Directory.CreateDirectory("UserData/ScreenshotManager/DiscordWebhooks");
            }

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

            if (!Directory.Exists(Configuration.ScreenshotDirectoryEntry.Value))
                Directory.CreateDirectory(Configuration.ScreenshotDirectoryEntry.Value);

            FileOrganization.PatchMethod();
            if (Configuration.FileOrganizationEntry.Value)
                FileOrganization.OrganizeAll();

            ClassInjector.RegisterTypeInIl2Cpp<EnableDisableListener>();
            MelonCoroutines.Start(Init());
        }

        // Fix for ActiveBackground

        private void PatchActiveBackground()
        {
            ActiveBackgroundMod = MelonHandler.Mods.Find(mod => mod.Info.Name == "ActiveBackground");
            MethodInfo methodInfo = typeof(ActiveBackground.Main).GetMethod("Setup", BindingFlags.NonPublic | BindingFlags.Instance);
            HarmonyInstance.Patch(methodInfo, postfix: new HarmonyLib.HarmonyMethod(typeof(ScreenshotManagerMod).GetMethod(nameof(ActiveBackgroundMethod), BindingFlags.Static | BindingFlags.NonPublic)));
            MelonLogger.Msg("ActiveBackground was found and patched.");
        }

        private static async void ActiveBackgroundMethod()
        {
            await Task.Delay(550);
            await TaskProvider.YieldToMainThread();
            ActiveBackground.Main activeBackground = (ActiveBackground.Main)Instance.ActiveBackgroundMod;
            if (activeBackground.enabled.Value)
                MenuManager.MenuRect.GetComponent<Image>().material = GameObject.Find("UserInterface/MenuContent/Backdrop/Backdrop/Background").GetComponent<Image>().material;
            else
                MenuManager.MenuRect.GetComponent<Image>().material = null;
        }

        private IEnumerator Init()
        {
            while (VRCUiManager.field_Private_Static_VRCUiManager_0 == null) yield return null;

            MenuManager.PrepareAssets();
            MenuManager.CreateMenus();
            MenuManager.ReloadDiscordWebhookButtons();

            ImageHandler.Init();
            ImageHandler.ReloadFiles().NoAwait();

            MelonLogger.Msg("Running version " + Version + " of ScreenshotManager.");
        }

        public override void OnUpdate()
        {
            TaskProvider.mainThreadQueue.Dequeue();
        }
    }
}
