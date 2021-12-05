using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MelonLoader;

namespace ScreenshotManager.Config
{
    public class DiscordWebhookConfiguration
    {
        private static readonly List<string> ValidURLs = new List<string>() {
            "https://discordapp.com/api/webhooks/",
            "https://discord.com/api/webhooks/",
            "https://canary.discordapp.com/api/webhooks/",
            "https://canary.discord.com/api/webhooks/",
            "https://media.guilded.gg/webhooks/"
        };

        private string file;

        public Entry<string> WebhookURL;
        public Entry<bool> SetUsername;
        public Entry<string> Username;
        public Entry<bool> SetMessage;
        public Entry<string> Message;
        public Entry<string> CreationTimeFormat;
        public Entry<int> CompressionThreshold;
        public Entry<bool> AutoUpload;

        public DiscordWebhookConfiguration(string file) => this.file = file;

        public bool Load()
        {
            try
            {
                string[] lines = File.ReadAllLines(file);
                WebhookURL = new Entry<string>("WebhookURL", "https://discord.com/...").Read(lines);
                SetUsername = new Entry<bool>("SetUsername", true).Read(lines);
                Username = new Entry<string>("Username", "{vrcname}").Read(lines);
                SetMessage = new Entry<bool>("SetMessage", true).Read(lines);
                Message = new Entry<string>("Message", "New screenshot by {vrcname} taken at {world} {creationtime} {timestamp:R}").Read(lines);
                CreationTimeFormat = new Entry<string>("CreationTimeFormat", "dd.MM.yyyy HH:mm:ss").Read(lines);
                CompressionThreshold = new Entry<int>("CompressionThreshold", -1).Read(lines);
                AutoUpload = new Entry<bool>("AutoUpload", false).Read(lines);
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                return false;
            }
            return true;
        }

        public bool IsValid() => ValidURLs.Any(validUrl => WebhookURL.Value.StartsWith(validUrl));

        public class Entry<T>
        {
            public string Name;
            public T Value;
            public T DefaultValue;

            public Entry(string name, T defaultValue)
            {
                Name = name;
                DefaultValue = defaultValue;
            }

            public Entry<T> Read(string[] lines)
            {
                foreach (string line in lines)
                {
                    if (line.Contains("="))
                    {
                        string[] content = line.Split('=');
                        if (content.Length == 2 && content[0].Trim().Equals(Name))
                        {
                            Value = (T)Convert.ChangeType(content[1].Trim(), typeof(T));
                            return this;
                        }
                        else
                        {
                            Value = DefaultValue;
                        }
                    }
                    else
                    {
                        Value = DefaultValue;
                    }
                }

                return this;
            }
        }
    }
}
