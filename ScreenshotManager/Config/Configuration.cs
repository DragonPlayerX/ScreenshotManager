using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MelonLoader;

using ScreenshotManager.Resources;

namespace ScreenshotManager.Config
{
    public static class Configuration
    {

        private static readonly MelonPreferences_Category Category = MelonPreferences.CreateCategory("ScreenshotManager", "Screenshot Manager");

        public static MelonPreferences_Entry<string> ScreenshotDirectoryEntry;
        public static MelonPreferences_Entry<bool> FileOrganizationEntry;
        public static MelonPreferences_Entry<string> FileOrganizationFolderEntry;
        public static MelonPreferences_Entry<string> FileOrganizationFileEntry;
        public static MelonPreferences_Entry<bool> DiscordWebhookEntry;
        public static MelonPreferences_Entry<bool> TabButtonEntry;
        public static MelonPreferences_Entry<int> TodayHourOffsetEntry;
        public static MelonPreferences_Entry<bool> MultiViewEntry;
        public static MelonPreferences_Entry<int> LastCategoryEntry;
        public static MelonPreferences_Entry<bool> MoveGalleryButtonEntry;
        public static MelonPreferences_Entry<bool> UseFileCreationTimeEntry;
        public static MelonPreferences_Entry<bool> ShowRotationButtonsEntry;

        public static bool HasChanged;

        public static Dictionary<string, DiscordWebhookConfiguration> DiscordWebHooks = new Dictionary<string, DiscordWebhookConfiguration>();

        public static void Init()
        {
            ScreenshotDirectoryEntry = CreateEntry("ScreenshotDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat", "Screenshot Directory");
            FileOrganizationEntry = CreateEntry("FileOrganization", false, "Enable File Organization");
            FileOrganizationFolderEntry = CreateEntry("FileOrganizationFolderName", "yyyy.MM.dd", "Organization Folder Name");
            FileOrganizationFileEntry = CreateEntry("FileOrganizationFileName", "yyyy.MM.dd_HH-mm-ss.fff", "Organization File Name");
            DiscordWebhookEntry = CreateEntry("DiscordWebHook", false, "Enable Discord Webhook");
            TabButtonEntry = CreateEntry("TabButton", true, "TabButton Enabled");
            TodayHourOffsetEntry = CreateEntry("TodayHourOffset", 0, "Today Hour Offset", "Offset the reset of today's pictures");
            MultiViewEntry = CreateEntry("MultiView", false, "MultiView Enabled");
            LastCategoryEntry = CreateEntry("LastCategory", 1, "Last Category");
            MoveGalleryButtonEntry = CreateEntry("MoveGalleryButton", false, "Move Gallery Button");
            UseFileCreationTimeEntry = CreateEntry("UseFileCreationTime", false, "Use File Creation Time");
            ShowRotationButtonsEntry = CreateEntry("ShowRotationButtons", true, "Show Rotation Buttons");

            if (!Directory.EnumerateFileSystemEntries("UserData/ScreenshotManager/DiscordWebhooks").Any())
                ResourceHandler.ExtractResource("DiscordWebhookTemplate.cfg", "UserData/ScreenshotManager/DiscordWebhooks");
        }

        public static void LoadDiscordWebhooks()
        {
            DiscordWebHooks.Clear();
            foreach (FileInfo fileInfo in new DirectoryInfo("UserData/ScreenshotManager/DiscordWebhooks").EnumerateFiles())
            {
                DiscordWebhookConfiguration discordWebhookConfiguration = new DiscordWebhookConfiguration(fileInfo.FullName);
                if (discordWebhookConfiguration.Load())
                    DiscordWebHooks.Add(fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(".")), discordWebhookConfiguration);
                else
                    MelonLogger.Error("Failed to load Webhook file: " + fileInfo.FullName);
            }
            MelonLogger.Msg("Loaded " + DiscordWebHooks.Count + " Discord Webhooks.");
        }

        public static void Save()
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 == null) return;
            if (HasChanged)
            {
                MelonPreferences.Save();
                HasChanged = false;
            }
        }

        private static MelonPreferences_Entry<T> CreateEntry<T>(string name, T defaultValue, string displayname, string description = null)
        {
            MelonPreferences_Entry<T> entry = Category.CreateEntry<T>(name, defaultValue, displayname, description);
            entry.OnValueChangedUntyped += new Action(() => HasChanged = true);
            return entry;
        }
    }
}
