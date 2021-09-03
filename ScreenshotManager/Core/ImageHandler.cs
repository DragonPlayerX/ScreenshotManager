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
using ScreenshotManager.Tasks;

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

        public static Text TitleText;
        public static Text InfoText;

        private static RawImage singleViewImage;
        private static RawImage[] multiViewImages = new RawImage[9];

        private static ImageMenuCategory currentCategory;
        private static FileInfo[] currentActiveFiles;
        private static int currentIndex = 0;

        public static bool IsReloading { get; private set; } = false;

        public static void Init()
        {
            currentCategory = (ImageMenuCategory)Configuration.LastCategoryEntry.Value;
            TitleText = MenuManager.MenuRect.Find("Title").GetComponent<Text>();
            InfoText = MenuManager.MenuRect.Find("Info").GetComponent<Text>();
            singleViewImage = MenuManager.SingleViewObject.GetComponent<RawImage>();

            Button.ButtonClickedEvent toggleViewAction = MenuManager.Menus[0].RectTransform.Find("ViewButton").GetComponent<Button>().onClick;
            for (int i = 1; i <= 9; i++)
            {
                multiViewImages[i - 1] = MenuManager.MultiViewObject.transform.Find("MultiViewImage" + i).GetComponent<RawImage>();
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
            if (IsReloading)
                return;
            if (currentIndex + (Configuration.MultiViewEntry.Value ? 9 : 1) < currentActiveFiles.Length)
                currentIndex += (Configuration.MultiViewEntry.Value ? 9 : 1);
            else
                currentIndex = 0;
            Update(true);
        }

        public static void Previous()
        {
            if (IsReloading)
                return;
            if (currentIndex - (Configuration.MultiViewEntry.Value ? 9 : 1) >= 0)
                currentIndex -= (Configuration.MultiViewEntry.Value ? 9 : 1);
            else
            {
                currentIndex = Configuration.MultiViewEntry.Value ? ((currentActiveFiles.Length - 1) / 9) * 9 : currentActiveFiles.Length - 1;
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
            if (Configuration.MultiViewEntry.Value)
                InfoText.text = (currentIndex / 9 + 1) + "/" + ((currentActiveFiles.Length - 1) / 9 + 1);
            else
                InfoText.text = (currentIndex + 1) + "/" + currentActiveFiles.Length;

            TitleText.text = Titles[currentCategory];

            if (!fetchImages)
                return;

            FetchCurrentImages();
        }

        public static void FetchCurrentImages()
        {
            if (Configuration.MultiViewEntry.Value)
            {
                for (int i = currentIndex; i < currentIndex + 9; i++)
                {
                    if (i < currentActiveFiles.Length)
                    {
                        string file = currentActiveFiles[i].FullName;
                        RawImage image = multiViewImages[i - currentIndex];
                        MelonCoroutines.Start(LoadImage(file, image));
                    }
                    else
                    {
                        DestroyTexture(multiViewImages[i - currentIndex]);
                        multiViewImages[i - currentIndex].transform.localScale = new Vector3(1, 1, 1);
                    }
                }
            }
            else
            {
                if (currentIndex < currentActiveFiles.Length)
                {
                    string file = currentActiveFiles[currentIndex].FullName;
                    MelonCoroutines.Start(LoadImage(file, singleViewImage));
                }
                else
                {
                    currentIndex = 0;
                    DestroyTexture(singleViewImage);
                    singleViewImage.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }

        private static IEnumerator LoadImage(string file, RawImage rawImage)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture("file:///" + file, true);
            UnityWebRequestAsyncOperation asyncOperation = webRequest.BeginWebRequest();
            while (!asyncOperation.isDone)
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
            else
            {
                MelonLogger.Warning("Failed to load image: " + file);
                DestroyTexture(rawImage);
                rawImage.color = new Color(1, 1, 1, 1);
                rawImage.texture = MenuManager.ErrorTexture;
                rawImage.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        private static void DestroyTexture(RawImage rawImage)
        {
            rawImage.color = new Color(1, 1, 1, 0.1f);
            if (rawImage.texture != null)
            {
                if (!rawImage.texture.Equals(MenuManager.ErrorTexture))
                    Object.Destroy(rawImage.texture);
                rawImage.texture = null;
            }
        }

        public static void ChangeCategory(ImageMenuCategory category)
        {
            if (category == currentCategory)
                return;
            if (IsReloading)
                return;

            IndexCache[currentCategory] = currentIndex;

            currentCategory = category;

            Configuration.LastCategoryEntry.Value = Array.IndexOf(Enum.GetValues(typeof(ImageMenuCategory)), currentCategory);

            TitleText.text = Titles[currentCategory];
            currentIndex = IndexCache[currentCategory];
            ReloadFiles().NoAwait();
        }

        public static void MovePicture()
        {
            if (currentActiveFiles.Length > 0 && !IsReloading)
            {
                FileInfo fileInfo = currentActiveFiles[currentIndex];
                if (!File.Exists(fileInfo.FullName))
                {
                    MelonLogger.Error("File not found: " + fileInfo.FullName);
                    return;
                }
                if (fileInfo.Directory.Name.Equals("Favorites"))
                {
                    if (Configuration.FileOrganizationEntry.Value)
                    {
                        DateTime creationTime = fileInfo.LastWriteTime;
                        string directory = Configuration.ScreenshotDirectoryEntry.Value + "/" + creationTime.ToString(Configuration.FileOrganizationFolderEntry.Value);
                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);
                        string newFile = directory + "/VRChat_" + creationTime.ToString(Configuration.FileOrganizationFileEntry.Value) + fileInfo.Extension;
                        File.Move(fileInfo.FullName, newFile);
                    }
                    else
                    {
                        File.Move(fileInfo.FullName, Configuration.ScreenshotDirectoryEntry.Value + "/" + fileInfo.Name);
                    }
                    ReloadFiles().NoAwait();
                }
                else
                {
                    if (!Directory.Exists(Configuration.ScreenshotDirectoryEntry.Value + "/Favorites"))
                    {
                        Directory.CreateDirectory(Configuration.ScreenshotDirectoryEntry.Value + "/Favorites");
                    }
                    File.Move(fileInfo.FullName, Configuration.ScreenshotDirectoryEntry.Value + "/Favorites/" + fileInfo.Name);
                    ReloadFiles().NoAwait();
                }
            }
        }

        public static void DeletePicture()
        {
            if (currentActiveFiles.Length > 0 && !IsReloading)
            {
                FileInfo fileInfo = currentActiveFiles[currentIndex];
                if (!File.Exists(fileInfo.FullName))
                {
                    MelonLogger.Error("File not found: " + fileInfo.FullName);
                    Update(false);
                    return;
                }
                File.Delete(fileInfo.FullName);
                ReloadFiles().NoAwait();
            }
            else
            {
                Update(false);
            }
        }

        public static void ShowFileInExplorer()
        {
            if (currentIndex < currentActiveFiles.Length && !IsReloading)
            {
                FileInfo fileInfo = currentActiveFiles[currentIndex];
                if (!File.Exists(fileInfo.FullName))
                {
                    MelonLogger.Error("File not found: " + fileInfo.FullName);
                    return;
                }
                Process.Start("explorer.exe", "/select, \"" + fileInfo.FullName + "\""); ;
            }
        }

        public static void RotateImage(bool direction)
        {
            if (currentActiveFiles.Length > 0 && !IsReloading)
            {
                FileInfo fileInfo = currentActiveFiles[currentIndex];
                if (!File.Exists(fileInfo.FullName))
                {
                    MelonLogger.Error("File not found: " + fileInfo.FullName);
                    return;
                }
                DateTime lastWriteTime = fileInfo.LastWriteTime;
                System.Drawing.Image image = System.Drawing.Image.FromFile(fileInfo.FullName);
                if (direction)
                    image.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
                else
                    image.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);
                image.Save(fileInfo.FullName);
                image.Dispose();
                File.SetLastWriteTime(fileInfo.FullName, lastWriteTime);
                Update(true);
            }
        }

        public static void SendToDiscordWebhook(string webhookName, DiscordWebhookConfiguration webhookConfig)
        {
            if (currentIndex < currentActiveFiles.Length && !IsReloading)
            {
                FileInfo fileInfo = currentActiveFiles[currentIndex];
                if (!File.Exists(fileInfo.FullName))
                {
                    MelonLogger.Error("File not found: " + fileInfo.FullName);
                    return;
                }

                MelonLogger.Msg("Uploading " + fileInfo.Name + " to Discord [" + webhookName + "]...");

                DateTime creationTime = Configuration.UseFileCreationTimeEntry.Value ? fileInfo.CreationTime : fileInfo.LastWriteTime;

                string username = webhookConfig.SetUsername.Value ? (webhookConfig.Username.Value.Replace("{vrcname}", APIUser.CurrentUser.displayName)) : "null";
                string message = webhookConfig.SetMessage.Value ? (webhookConfig.Message.Value.Replace("{vrcname}", APIUser.CurrentUser.displayName).Replace("{creationtime}", creationTime.ToString(webhookConfig.CreationTimeFormat.Value))) : "null";

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "/Executables/DiscordWebhook.exe";
                processStartInfo.Arguments = "\""
                    + webhookConfig.WebhookURL.Value + "\" \""
                    + webhookConfig.SetUsername.Value.ToString() + "\" \""
                    + username + "\" \""
                    + webhookConfig.SetMessage.Value.ToString() + "\" \""
                    + message + "\" \""
                    + fileInfo.FullName + "\" \""
                    + fileInfo.Name + "\"";
                processStartInfo.UseShellExecute = false;
                processStartInfo.RedirectStandardError = true;

                AsyncProcessProvider.StartProcess(processStartInfo, new Action<bool, int>((hasExited, exitCode) =>
                {
                    if (hasExited && exitCode == 0)
                        MelonLogger.Msg("File " + fileInfo.Name + " was uploaded to Discord [" + webhookName + "].");
                    else
                        MelonLogger.Error("Error while uploading file " + fileInfo.Name + " to Discord [" + webhookName + "]. Process exited with " + exitCode);
                }), "DiscordWebhook").NoAwait();
            }
        }

        public static async Task ReloadFiles()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            InfoText.text = "Indexing...";

            await TaskProvider.YieldToBackgroundTask();

            if (currentCategory == ImageMenuCategory.TODAY)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectoryEntry.Value);
                DateTime dateTime = DateTime.Now.Date.AddHours(Configuration.TodayHourOffsetEntry.Value);
                currentActiveFiles = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => Extensions.Any(f.Extension.EndsWith) && !f.Directory.Name.Equals("Favorites") && (Configuration.UseFileCreationTimeEntry.Value ? f.CreationTime : f.LastWriteTime) >= dateTime).OrderBy(f => (Configuration.UseFileCreationTimeEntry.Value ? f.CreationTime : f.LastWriteTime)).ToArray();
            }
            else if (currentCategory == ImageMenuCategory.FAVORITES)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectoryEntry.Value + "\\Favorites");
                if (!Directory.Exists(Configuration.ScreenshotDirectoryEntry.Value + "\\Favorites"))
                    Directory.CreateDirectory(Configuration.ScreenshotDirectoryEntry.Value + "\\Favorites");
                currentActiveFiles = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => Extensions.Any(f.Extension.EndsWith)).OrderBy(f => (Configuration.UseFileCreationTimeEntry.Value ? f.CreationTime : f.LastWriteTime)).ToArray();
            }
            else
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectoryEntry.Value);
                currentActiveFiles = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => Extensions.Any(f.Extension.EndsWith) && !f.Directory.Name.Equals("Favorites")).OrderBy(f => (Configuration.UseFileCreationTimeEntry.Value ? f.CreationTime : f.LastWriteTime)).ToArray();
            }

            if (currentIndex >= currentActiveFiles.Length)
                currentIndex = 0;

            IsReloading = false;
            await TaskProvider.YieldToMainThread();
            Update(true);
        }
    }
}
