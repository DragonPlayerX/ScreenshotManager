using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MelonLoader;
using MelonLoader.Preferences;

using ScreenshotManager.Resources;

namespace ScreenshotManager.Config
{
    public static class Configuration
    {
        private static readonly MelonPreferences_Category Category = MelonPreferences.CreateCategory("ScreenshotManager", "Screenshot Manager");

        public static MelonPreferences_Entry<string> ScreenshotDirectory;
        public static MelonPreferences_Entry<bool> FileOrganization;
        public static MelonPreferences_Entry<string> FileOrganizationFolderTimeFormat;
        public static MelonPreferences_Entry<string> FileOrganizationFileTimeFormat;
        public static MelonPreferences_Entry<string> FileOrganizationNameFormat;
        public static MelonPreferences_Entry<bool> TabButton;
        public static MelonPreferences_Entry<int> TodayHourOffset;
        public static MelonPreferences_Entry<bool> MultiView;
        public static MelonPreferences_Entry<int> LastCategory;
        public static MelonPreferences_Entry<bool> UseFileCreationTime;
        public static MelonPreferences_Entry<bool> ShowRotationButtons;
        public static MelonPreferences_Entry<bool> WriteImageMetadata;
        public static MelonPreferences_Entry<bool> AutoSelectLatest;
        public static MelonPreferences_Entry<int> ZoomFactor;

        public static bool HasChanged;

        public static Dictionary<string, DiscordWebhookConfiguration> DiscordWebhooks = new Dictionary<string, DiscordWebhookConfiguration>();

        public static void Init()
        {
            Category.DeleteEntry("DiscordWebhookURL");
            Category.DeleteEntry("DiscordWebHook");
            Category.DeleteEntry("DiscordWebhookSetUsernameEntry");
            Category.DeleteEntry("DiscordWebhookUsernameEntry");
            Category.DeleteEntry("DiscordWebhookSetMessageEntry");
            Category.DeleteEntry("DiscordWebhookMessageEntry");
            Category.DeleteEntry("UseUIX");
            Category.DeleteEntry("MoveGalleryButton");

            Category.RenameEntry("FileOrganizationFolderName", "FileOrganizationFolderTimeFormat");
            Category.RenameEntry("FileOrganizationFileName", "FileOrganizationFileTimeFormat");

            ScreenshotDirectory = CreateEntry("ScreenshotDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat", "Screenshot Directory");
            FileOrganization = CreateEntry("FileOrganization", false, "File Organization");
            FileOrganizationFolderTimeFormat = CreateEntry("FileOrganizationFolderTimeFormat", "yyyy.MM.dd", "Folder Time Format");
            FileOrganizationFileTimeFormat = CreateEntry("FileOrganizationFileTimeFormat", "yyyy.MM.dd_HH-mm-ss.fff", "File Time Format");
            FileOrganizationNameFormat = CreateEntry("FileOrganizationNameFormat", "VRChat_{timestamp}", "File Name Format", new StringValidator("VRChat_{timestamp}", "{timestamp}"));
            TabButton = CreateEntry("TabButton", true, "TabButton");
            TodayHourOffset = CreateEntry("TodayHourOffset", 0, "Today Hour Offset");
            MultiView = CreateEntry("MultiView", false, "MultiView");
            LastCategory = CreateEntry("LastCategory", 1, "Last Category");
            UseFileCreationTime = CreateEntry("UseFileCreationTime", false, "Use File Creation Time");
            ShowRotationButtons = CreateEntry("ShowRotationButtons", true, "Show Rotation Buttons");
            WriteImageMetadata = CreateEntry("WriteImageMetadata", true, "Image Metadata");
            AutoSelectLatest = CreateEntry("AutoSelectLatest", false, "Auto Select Latest Image");
            ZoomFactor = CreateEntry("ZoomFactor", 4, "Zoom Factor");

            if (!Directory.EnumerateFileSystemEntries("UserData/ScreenshotManager/DiscordWebhooks").Any())
                ResourceHandler.ExtractResource("DiscordWebhookTemplate.cfg", "UserData/ScreenshotManager/DiscordWebhooks");
        }

        public static int LoadDiscordWebhooks()
        {
            DiscordWebhooks.Clear();
            foreach (FileInfo fileInfo in new DirectoryInfo("UserData/ScreenshotManager/DiscordWebhooks").EnumerateFiles())
            {
                if (!fileInfo.Extension.Equals(".cfg")) continue;

                DiscordWebhookConfiguration discordWebhookConfiguration = new DiscordWebhookConfiguration(fileInfo.FullName);
                if (discordWebhookConfiguration.Load())
                    DiscordWebhooks.Add(fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(".")), discordWebhookConfiguration);
                else
                    ScreenshotManagerMod.Logger.Error("Failed to load Webhook file: " + fileInfo.FullName);
            }
            ScreenshotManagerMod.Logger.Msg("Loaded " + DiscordWebhooks.Count + " Discord Webhooks.");
            return DiscordWebhooks.Count;
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

        private static MelonPreferences_Entry<T> CreateEntry<T>(string name, T defaultValue, string displayname, ValueValidator valueValidator = null)
        {
            MelonPreferences_Entry<T> entry = Category.CreateEntry<T>(name, defaultValue, displayname, validator: valueValidator);
            entry.OnValueChangedUntyped += new Action(() => HasChanged = true);
            return entry;
        }

        private class StringValidator : ValueValidator
        {
            public string DefaultValue;
            public string Content;

            public StringValidator(string defaultValue, string content)
            {
                DefaultValue = defaultValue;
                Content = content;
            }

            public override object EnsureValid(object value)
            {
                if (IsValid(value))
                    return value;
                else
                    return DefaultValue;
            }

            public override bool IsValid(object value)
            {
                return (value as string).Contains(Content);
            }
        }
    }
}
