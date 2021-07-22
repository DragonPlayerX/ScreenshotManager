using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using VRC.Core;

using Object = UnityEngine.Object;

using ScreenshotManager.Config;

namespace ScreenshotManager.Core
{
    public static class ImageHandler
    {

        public enum ImageMenuCategory
        {
            ALL,
            TODAY,
            FAVORITES
        }

        private static readonly Dictionary<ImageMenuCategory, string> Titles = new Dictionary<ImageMenuCategory, string>
        {
            { ImageMenuCategory.ALL, "All Pictures" },
            { ImageMenuCategory.TODAY, "Today's Pictures" },
            { ImageMenuCategory.FAVORITES, "Favorite Pictures" }
        };

        private static readonly Dictionary<ImageMenuCategory, int> IndexCache = new Dictionary<ImageMenuCategory, int>
        {
            { ImageMenuCategory.ALL, 0 },
            { ImageMenuCategory.TODAY, 0 },
            { ImageMenuCategory.FAVORITES, 0 }
        };

        private static readonly string[] Extensions = new string[] { ".png", ".jpeg" };

        private static ImageMenuCategory currentCategory = ImageMenuCategory.TODAY;
        public static Text titleText;
        public static Text infoText;

        private static RawImage singleViewImage;
        private static RawImage[] multiViewImages = new RawImage[9];

        private static FileInfo[] currentActiveFiles;
        private static int currentIndex = 0;

        public static bool multiView = false;

        public static bool isReloading = false;

        public static void Init()
        {
            titleText = MenuManager.menuRect.Find("Title").GetComponent<Text>();
            infoText = MenuManager.menuRect.Find("Info").GetComponent<Text>();
            titleText.text = "Today's Pictures";
            singleViewImage = MenuManager.singleViewObject.GetComponent<RawImage>();
            Button.ButtonClickedEvent toggleViewAction = MenuManager.menus[0].RectTransform.Find("ViewButton").GetComponent<Button>().onClick;
            for (int i = 1; i <= 9; i++)
            {
                multiViewImages[i - 1] = MenuManager.multiViewObject.transform.Find("MultiViewImage" + i).GetComponent<RawImage>();
                int finalI = i;
                multiViewImages[i - 1].GetComponent<Button>().onClick.AddListener(new Action(() =>
                {
                    if (currentIndex + (finalI - 1) < currentActiveFiles.Length)
                    {
                        currentIndex += (finalI - 1);
                        toggleViewAction.Invoke();
                        Update(false);
                    }
                }));
            }
        }

        public static void Next()
        {
            if (isReloading)
                return;
            if (currentIndex + (multiView ? 9 : 1) < currentActiveFiles.Length)
                currentIndex += (multiView ? 9 : 1);
            else
                currentIndex = 0;
            Update(true);
        }

        public static void Previous()
        {
            if (isReloading)
                return;
            if (currentIndex - (multiView ? 9 : 1) >= 0)
                currentIndex -= (multiView ? 9 : 1);
            else
            {
                currentIndex = multiView ? ((currentActiveFiles.Length - 1) / 9) * 9 : currentActiveFiles.Length - 1;
                if (currentIndex < 0)
                    currentIndex = 0;
            }
            Update(true);
        }

        public static void SetMultiView()
        {
            currentIndex = (currentIndex / 9) * 9;
            Update(true);
        }

        public static void Update(bool fetchImages)
        {
            if (multiView)
                infoText.text = (currentIndex / 9 + 1) + "/" + ((currentActiveFiles.Length - 1) / 9 + 1);
            else
                infoText.text = (currentIndex + 1) + "/" + currentActiveFiles.Length;

            titleText.text = Titles[currentCategory];

            if (!fetchImages)
                return;
            if (multiView)
                FetchCurrentImages();
            else
                FetchCurrentImage();
        }

        public static void FetchCurrentImage()
        {
            if (currentIndex < currentActiveFiles.Length)
            {
                ScreenshotManagerMod.Enqueue(new Action(() => MelonCoroutines.Start(LoadImage(currentActiveFiles[currentIndex].FullName, singleViewImage))));
            }
            else
            {
                currentIndex = 0;
                DestroyTexture(singleViewImage);
                singleViewImage.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        public static void FetchCurrentImages()
        {
            for (int i = currentIndex; i < currentIndex + 9; i++)
            {
                if (i < currentActiveFiles.Length)
                {
                    string file = currentActiveFiles[i].FullName;
                    RawImage image = multiViewImages[i - currentIndex];
                    ScreenshotManagerMod.Enqueue(new Action(() => MelonCoroutines.Start(LoadImage(file, image))));
                }
                else
                {
                    DestroyTexture(multiViewImages[i - currentIndex]);
                    multiViewImages[i - currentIndex].transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }

        private static IEnumerator LoadImage(string file, RawImage rawImage)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture("file:///" + file);
            webRequest.SendWebRequest();
            while (!webRequest.isDone)
                yield return null;
            if (!webRequest.isNetworkError && !webRequest.isHttpError)
            {
                DestroyTexture(rawImage);
                rawImage.color = new Color(1, 1, 1, 1);
                rawImage.texture = DownloadHandlerTexture.GetContent(webRequest);
                if (rawImage.texture.height > rawImage.texture.width)
                    rawImage.transform.localScale = new Vector3(0.316f, 1, 1);
                else
                    rawImage.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        private static void DestroyTexture(RawImage rawImage)
        {
            rawImage.color = new Color(1, 1, 1, 0.1f);
            if (rawImage.texture != null)
            {
                Object.Destroy(rawImage.texture);
                rawImage.texture = null;
            }
        }

        public async static void ChangeCategory(ImageMenuCategory category)
        {
            if (category == currentCategory)
                return;

            if (isReloading)
                return;

            IndexCache[currentCategory] = currentIndex;

            currentCategory = category;

            titleText.text = Titles[currentCategory];
            currentIndex = IndexCache[currentCategory];
            await ReloadFiles();
        }

        public async static void MovePicture()
        {
            if (currentActiveFiles.Length > 0 && !isReloading)
            {
                FileInfo fileInfo = currentActiveFiles[currentIndex];
                if (fileInfo.Directory.Name.Equals("Favorites"))
                {
                    File.Move(fileInfo.FullName, Configuration.ScreenshotDirectoryEntry.Value + "/" + fileInfo.Name);
                    await ReloadFiles();
                    Update(false);
                }
                else
                {
                    if (!Directory.Exists(Configuration.ScreenshotDirectoryEntry.Value + "/Favorites"))
                    {
                        Directory.CreateDirectory(Configuration.ScreenshotDirectoryEntry.Value + "/Favorites");
                    }
                    File.Move(fileInfo.FullName, Configuration.ScreenshotDirectoryEntry.Value + "/Favorites/" + fileInfo.Name);
                    await ReloadFiles();
                    Update(false);
                }
            }
        }

        public async static void DeletePicture()
        {
            if (currentActiveFiles.Length > 0 && !isReloading)
            {
                FileInfo fileInfo = currentActiveFiles[currentIndex];
                File.Delete(fileInfo.FullName);
                await ReloadFiles();
                Update(false);
            }
        }

        public static void ShowFileInExplorer()
        {
            if (currentIndex < currentActiveFiles.Length && !isReloading)
            {
                Process.Start("explorer.exe", "/select, \"" + currentActiveFiles[currentIndex].FullName + "\""); ;
            }
        }

        public static void SendToDiscordWebhook()
        {
            if (currentIndex < currentActiveFiles.Length && !isReloading)
            {
                FileInfo fileInfo = currentActiveFiles[currentIndex];
                MelonLogger.Msg("Uploading " + fileInfo.Name + " to Discord Webhook...");
                string username = Configuration.DiscordWebhookSetUsernameEntry.Value ? (Configuration.DiscordWebhookUsernameEntry.Value.Replace("{vrcname}", APIUser.CurrentUser.displayName)) : "null";
                string message = Configuration.DiscordWebhookSetMessageEntry.Value ? (Configuration.DiscordWebhookMessageEntry.Value.Replace("{vrcname}", APIUser.CurrentUser.displayName).Replace("{creationtime}", fileInfo.CreationTime.ToString("MM.dd.yyyy HH:mm:ss"))) : "null";
                Process.Start("Executables/DiscordWebhook.exe", "\""
                    + Configuration.DiscordWebhookURLEntry.Value + "\" \""
                    + Configuration.DiscordWebhookSetUsernameEntry.Value.ToString() + "\" \""
                    + username + "\" \""
                    + Configuration.DiscordWebhookSetMessageEntry.Value.ToString() + "\" \""
                    + message + "\" \""
                    + fileInfo.FullName + "\" \""
                    + fileInfo.Name + "\"");
            }
        }

        public static async void Reload()
        {
            await ReloadFiles();
        }

        public static async Task ReloadFiles()
        {
            if (isReloading)
                return;
            isReloading = true;
            await Task.Run(() =>
            {
                infoText.text = "Indexing...";

                if (currentCategory == ImageMenuCategory.TODAY)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectoryEntry.Value);
                    DateTime dateTime = DateTime.Now.Date.AddHours(Configuration.TodayHourOffset.Value);
                    currentActiveFiles = directoryInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(f => Extensions.Any(f.Extension.EndsWith) && f.CreationTime >= dateTime).OrderBy(f => f.CreationTime).ToArray();
                }
                else if (currentCategory == ImageMenuCategory.FAVORITES)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectoryEntry.Value + "\\Favorites");
                    if (!Directory.Exists(Configuration.ScreenshotDirectoryEntry.Value + "\\Favorites"))
                        Directory.CreateDirectory(Configuration.ScreenshotDirectoryEntry.Value + "\\Favorites");
                    currentActiveFiles = directoryInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(f => Extensions.Any(f.Extension.EndsWith)).OrderBy(f => f.CreationTime).ToArray();
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectoryEntry.Value);
                    currentActiveFiles = directoryInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(f => Extensions.Any(f.Extension.EndsWith)).OrderBy(f => f.CreationTime).ToArray();
                }

                if (currentIndex >= currentActiveFiles.Length)
                    currentIndex = 0;

                Update(true);
                isReloading = false;
            });
        }
    }
}
