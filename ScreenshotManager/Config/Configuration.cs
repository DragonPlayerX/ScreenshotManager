﻿using System;
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

        public static MelonPreferences_Entry<string> ScreenshotDirectory;
        public static MelonPreferences_Entry<bool> FileOrganization;
        public static MelonPreferences_Entry<string> FileOrganizationFolder;
        public static MelonPreferences_Entry<string> FileOrganizationFile;
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
            ScreenshotDirectory = CreateEntry("ScreenshotDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat", "Screenshot Directory");
            FileOrganization = CreateEntry("FileOrganization", false, "File Organization");
            FileOrganizationFolder = CreateEntry("FileOrganizationFolderName", "yyyy.MM.dd", "Organization Folder Name");
            FileOrganizationFile = CreateEntry("FileOrganizationFileName", "yyyy.MM.dd_HH-mm-ss.fff", "Organization File Name");
            TabButton = CreateEntry("TabButton", true, "TabButton");
            TodayHourOffset = CreateEntry("TodayHourOffset", 0, "Today Hour Offset", "Offset the reset of today's pictures");
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

        public static void LoadDiscordWebhooks()
        {
            DiscordWebhooks.Clear();
            foreach (FileInfo fileInfo in new DirectoryInfo("UserData/ScreenshotManager/DiscordWebhooks").EnumerateFiles())
            {
                if (!fileInfo.Extension.Equals(".cfg")) continue;

                DiscordWebhookConfiguration discordWebhookConfiguration = new DiscordWebhookConfiguration(fileInfo.FullName);
                if (discordWebhookConfiguration.Load())
                    DiscordWebhooks.Add(fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf(".")), discordWebhookConfiguration);
                else
                    MelonLogger.Error("Failed to load Webhook file: " + fileInfo.FullName);
            }
            MelonLogger.Msg("Loaded " + DiscordWebhooks.Count + " Discord Webhooks.");
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
