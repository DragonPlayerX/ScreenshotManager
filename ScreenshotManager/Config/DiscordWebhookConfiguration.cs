using System;
using System.IO;
using MelonLoader;

namespace ScreenshotManager.Config
{
    public class DiscordWebhookConfiguration
    {

        private string file;

        public Entry<string> WebhookURL;
        public Entry<bool> SetUsername;
        public Entry<string> Username;
        public Entry<bool> SetMessage;
        public Entry<string> Message;
        public Entry<string> CreationTimeFormat;

        public DiscordWebhookConfiguration(string file)
        {
            this.file = file;
        }

        public bool Load()
        {
            try
            {
                string[] lines = File.ReadAllLines(file);
                WebhookURL = new Entry<string>("WebhookURL", lines);
                SetUsername = new Entry<bool>("SetUsername", lines);
                Username = new Entry<string>("Username", lines);
                SetMessage = new Entry<bool>("SetMessage", lines);
                Message = new Entry<string>("Message", lines);
                CreationTimeFormat = new Entry<string>("CreationTimeFormat", lines);
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                return false;
            }
            return true;
        }

        public class Entry<T>
        {

            public string Name;
            public T Value;

            public Entry(string name, string[] lines)
            {
                Name = name;
                foreach (string line in lines)
                {
                    if (line.Contains("="))
                    {
                        string[] content = line.Split('=');
                        if (content.Length == 2 && content[0].Trim().Equals(Name))
                        {
                            Value = (T)Convert.ChangeType(content[1].Trim(), typeof(T));
                            return;
                        }
                        else
                        {
                            Value = default;
                        }
                    }
                    else
                    {
                        Value = default;
                    }
                }
            }
        }
    }
}
