using System;
using MelonLoader;

namespace ScreenshotManager.Config
{
    public static class Configuration
    {

        private static readonly MelonPreferences_Category Category = MelonPreferences.CreateCategory("ScreenshotManager", "Screenshot Manager");

        public static MelonPreferences_Entry<string> ScreenshotDirectoryEntry;
        public static MelonPreferences_Entry<string> DiscordWebhookURLEntry;
        public static MelonPreferences_Entry<bool> DiscordWebhookEntry;
        public static MelonPreferences_Entry<bool> DiscordWebhookSetUsernameEntry;
        public static MelonPreferences_Entry<string> DiscordWebhookUsernameEntry;
        public static MelonPreferences_Entry<bool> DiscordWebhookSetMessageEntry;
        public static MelonPreferences_Entry<string> DiscordWebhookMessageEntry;
        public static MelonPreferences_Entry<bool> TabButtonEntry;
        public static MelonPreferences_Entry<int> TodayHourOffset;

        public static bool hasChanged;

        public static void Init()
        {
            ScreenshotDirectoryEntry = CreateEntry("ScreenshotDirectory", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\VRChat", "Screenshot Directory");
            DiscordWebhookURLEntry = CreateEntry("DiscordWebhookURL", "Replace with Webhook link", "URL to Discord Webhook");
            DiscordWebhookEntry = CreateEntry("DiscordWebHook", false, "Enable Discord Webhook");
            DiscordWebhookSetUsernameEntry = CreateEntry("DiscordWebhookSetUsernameEntry", true, "Enable Webhook Name");
            DiscordWebhookUsernameEntry = CreateEntry("DiscordWebhookUsernameEntry", "{vrcname}", "Webhook Name");
            DiscordWebhookSetMessageEntry = CreateEntry("DiscordWebhookSetMessageEntry", true, "Enable Webhook Message");
            DiscordWebhookMessageEntry = CreateEntry("DiscordWebhookMessageEntry", "New Screenshot by {vrcname} - Picture taken at: {creationtime}", "Webhook Message");
            TabButtonEntry = CreateEntry("TabButton", true, "TabButton Enabled", "false = button appears in camera menu");
            TodayHourOffset = CreateEntry("TodayHourOffset", 0, "Today Hour Offset", "Offset the reset of today's pictures");
        }

        public static void Save()
        {
            if (RoomManager.field_Internal_Static_ApiWorldInstance_0 == null) return;
            if (hasChanged)
            {
                MelonPreferences.Save();
                hasChanged = false;
            }
        }

        private static MelonPreferences_Entry<T> CreateEntry<T>(string name, T defaultValue, string displayname, string description = null)
        {
            MelonPreferences_Entry<T> entry = Category.CreateEntry<T>(name, defaultValue, displayname, description);
            entry.OnValueChangedUntyped += new Action(() => hasChanged = true);
            return entry;
        }
    }
}
