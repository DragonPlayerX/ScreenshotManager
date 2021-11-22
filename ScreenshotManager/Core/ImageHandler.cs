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
using ScreenshotManager.Utils;
using ScreenshotManager.UI;

namespace ScreenshotManager.Core
{
    public static class ImageHandler
    {

        public enum ImageMenuCategory
        {
            All,
            Today,
            Yesterday,
            Favorites
        }

        public enum Direction
        {
            Left,
            Right
        }

        private static readonly Dictionary<ImageMenuCategory, string> Titles = new Dictionary<ImageMenuCategory, string>
        {
            { ImageMenuCategory.All, "All Pictures" },
            { ImageMenuCategory.Today, "Today's Pictures" },
            { ImageMenuCategory.Yesterday, "Yesterdays's Pictures" },
            { ImageMenuCategory.Favorites, "Favorite Pictures" }
        };

        private static readonly Dictionary<ImageMenuCategory, int> IndexCache = new Dictionary<ImageMenuCategory, int>
        {
            { ImageMenuCategory.All, 0 },
            { ImageMenuCategory.Today, 0 },
            { ImageMenuCategory.Yesterday, 0 },
            { ImageMenuCategory.Favorites, 0 }
        };

        public static readonly string[] Extensions = new string[] { ".png", ".jpeg" };

        public static string TitleText
        {
            get => MenuManager.InfoButtonHeader.LeftText;
            set => MenuManager.InfoButtonHeader.LeftText = value;
        }

        public static string InfoText
        {
            get => MenuManager.InfoButtonHeader.RightText;
            set => MenuManager.InfoButtonHeader.RightText = value;
        }

        public static ImageMenuCategory CurrentCategory;

        private static ImageWrapper singleImageWrapper;
        private static ImageWrapper secondarySingleImageWrapper;
        private static ImageWrapper[] multiImageWrappers = new ImageWrapper[9];

        private static List<FileInfo> fileChache = new List<FileInfo>();
        private static FileInfo selectedFile;
        private static int currentIndex = 0;

        public static bool IsReloading { get; private set; } = false;

        public static void Init()
        {
            CurrentCategory = (ImageMenuCategory)Configuration.LastCategory.Value;

            singleImageWrapper = new ImageWrapper(MenuManager.SingleImageContainer.transform.Find("SingleImage_Mask/Image").gameObject);
            secondarySingleImageWrapper = new ImageWrapper(MenuManager.SecondaryImageContainer.transform.Find("SingleImage_Mask/Image").gameObject);

            Toggle.ToggleEvent toggleViewAction = MenuManager.TabButton.SubMenu.RectTransform.Find("ScrollRect/Viewport/VerticalLayoutGroup/Buttons_Actions/Button_View").GetComponent<Toggle>().onValueChanged;
            for (int i = 1; i <= 9; i++)
            {
                int finalI = i;
                multiImageWrappers[i - 1] = new ImageWrapper(MenuManager.MultiImageContainer.transform.Find("MultiImage_Mask_" + finalI + "/Image").gameObject);
                multiImageWrappers[i - 1].Image.GetComponent<Button>().onClick.AddListener(new Action(() =>
                {
                    if (currentIndex + (finalI - 1) < fileChache.Count)
                    {
                        currentIndex += (finalI - 1);
                        toggleViewAction.Invoke(false);
                        Update(false);
                    }
                }));
            }
        }

        public static void AddFile(FileInfo fileInfo)
        {
            if (CurrentCategory == ImageMenuCategory.All || CurrentCategory == ImageMenuCategory.Today)
            {
                fileChache.Add(fileInfo);
                Update(false);
            }
        }

        public static bool UpdateCurrentSelectedFile()
        {
            selectedFile = fileChache.Count > 0 ? fileChache[currentIndex] : null;
            MenuManager.ActionsMenuInfoButtonHeader.LeftText = "Image " + (currentIndex + 1) + " of " + fileChache.Count;

            if (selectedFile == null)
                return false;

            System.Drawing.Image image = System.Drawing.Image.FromFile(selectedFile.FullName);

            MenuManager.ImageCreationTimeText.text = Configuration.UseFileCreationTime.Value ? selectedFile.CreationTime.ToString("HH:mm:ss dd.MM.yyyy") : selectedFile.LastWriteTime.ToString("HH:mm:ss dd.MM.yyyy");
            MenuManager.ImageSizeText.text = image.Width + "x" + image.Height + " - " + (selectedFile.Length / 1024f / 1024f).ToString("0.00") + " MB";
            MenuManager.ImageWorldNameText.text = GetWorldTag(selectedFile, image);
            return true;
        }

        public static void Next()
        {
            if (IsReloading)
                return;
            if (currentIndex + (Configuration.MultiView.Value ? 9 : 1) < fileChache.Count)
                currentIndex += (Configuration.MultiView.Value ? 9 : 1);
            else
                currentIndex = 0;
            Update(true);
        }

        public static void Previous()
        {
            if (IsReloading)
                return;
            if (currentIndex - (Configuration.MultiView.Value ? 9 : 1) >= 0)
                currentIndex -= (Configuration.MultiView.Value ? 9 : 1);
            else
            {
                currentIndex = Configuration.MultiView.Value ? ((fileChache.Count - 1) / 9) * 9 : fileChache.Count - 1;
                if (currentIndex < 0)
                    currentIndex = 0;
            }
            Update(true);
        }

        public static void SetMultiView()
        {
            if (IsReloading)
                return;

            currentIndex = (currentIndex / 9) * 9;
            Update(true);
        }

        public static void SelectLatest()
        {
            currentIndex = fileChache.Count - 1;
            Update(true);
        }

        public static void Update(bool fetchImages)
        {
            if (Configuration.MultiView.Value)
                InfoText = (currentIndex / 9 + 1) + "/" + ((fileChache.Count - 1) / 9 + 1);
            else
                InfoText = (currentIndex + 1) + "/" + fileChache.Count;

            TitleText = Titles[CurrentCategory];

            if (!fetchImages)
                return;

            FetchCurrentImages();
        }

        public static void FetchCurrentImages()
        {
            if (Configuration.MultiView.Value)
            {
                for (int i = currentIndex; i < currentIndex + 9; i++)
                {
                    if (i < fileChache.Count)
                    {
                        string file = fileChache[i].FullName;
                        ImageWrapper imageWrapper = multiImageWrappers[i - currentIndex];
                        MelonCoroutines.Start(LoadImage(file, imageWrapper));
                    }
                    else
                    {
                        DestroyTexture(multiImageWrappers[i - currentIndex]);
                    }
                }
            }
            else
            {
                if (currentIndex < fileChache.Count)
                {
                    string file = fileChache[currentIndex].FullName;
                    MelonCoroutines.Start(LoadImage(file, singleImageWrapper));
                }
                else
                {
                    currentIndex = 0;
                    DestroyTexture(singleImageWrapper);
                }
            }
        }

        public static void FetchSecondaryImage() => MelonCoroutines.Start(LoadImage(selectedFile.FullName, secondarySingleImageWrapper, 0.5f));

        private static IEnumerator LoadImage(string file, ImageWrapper imageWrapper, float size = 1)
        {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture("file:///" + file, true);
            UnityWebRequestAsyncOperation asyncOperation = webRequest.BeginWebRequest();
            while (!asyncOperation.isDone)
                yield return null;
            if (!webRequest.isNetworkError && !webRequest.isHttpError)
            {
                DestroyTexture(imageWrapper, size);
                imageWrapper.Image.color = new Color(1, 1, 1, 1);
                imageWrapper.Mask.color = new Color(0, 0, 0, 1);
                imageWrapper.Image.texture = DownloadHandlerTexture.GetContent(webRequest);
                if (imageWrapper.Image.texture.height > imageWrapper.Image.texture.width)
                    imageWrapper.Image.transform.parent.localScale = new Vector3(0.316f * size, 1 * size, 1);
                else
                    imageWrapper.Image.transform.parent.localScale = new Vector3(1 * size, 1 * size, 1);
            }
            else
            {
                MelonLogger.Warning("Failed to load image: " + file);
                DestroyTexture(imageWrapper, size);
                imageWrapper.Image.color = new Color(1, 1, 1, 1);
                imageWrapper.Image.color = new Color(1, 1, 1, 1);
                imageWrapper.Image.texture = MenuManager.ErrorTexture;
                imageWrapper.Image.transform.parent.localScale = new Vector3(1 * size, 1 * size, 1);
            }
        }

        private static void DestroyTexture(ImageWrapper imageWrapper, float size = 1)
        {
            imageWrapper.Image.color = new Color(1, 1, 1, 0.1f);
            imageWrapper.Mask.color = new Color(0, 0, 0, 0.1f);
            imageWrapper.Image.transform.parent.localScale = new Vector3(1 * size, 1 * size, 1);
            if (imageWrapper.Image.texture != null)
            {
                if (!imageWrapper.Image.texture.Equals(MenuManager.ErrorTexture))
                    Object.Destroy(imageWrapper.Image.texture);
                imageWrapper.Image.texture = null;
            }
        }

        public static void ChangeCategory(ImageMenuCategory category)
        {
            if (category == CurrentCategory)
                return;
            if (IsReloading)
                return;

            IndexCache[CurrentCategory] = currentIndex;

            CurrentCategory = category;

            Configuration.LastCategory.Value = Array.IndexOf(Enum.GetValues(typeof(ImageMenuCategory)), CurrentCategory);

            TitleText = Titles[CurrentCategory];
            currentIndex = IndexCache[CurrentCategory];
            ReloadFiles().NoAwait();
        }

        public static bool IsCurrentFavorite()
        {
            if (selectedFile != null)
            {
                if (!File.Exists(selectedFile.FullName))
                {
                    MelonLogger.Error("File not found: " + selectedFile.FullName);
                    return false;
                }
                return selectedFile.Directory.Name.Equals("Favorites");
            }
            else
            {
                return false;
            }
        }

        public static void ChangeFavoriteState()
        {
            if (selectedFile != null && !IsReloading)
            {
                if (!File.Exists(selectedFile.FullName))
                {
                    MelonLogger.Error("File not found: " + selectedFile.FullName);
                    return;
                }

                string path;

                if (selectedFile.Directory.Name.Equals("Favorites"))
                {
                    if (Configuration.FileOrganization.Value)
                    {
                        DateTime creationTime = selectedFile.LastWriteTime;
                        string directory = Configuration.ScreenshotDirectory.Value + "/" + creationTime.ToString(Configuration.FileOrganizationFolder.Value);
                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);
                        path = directory + "/VRChat_" + creationTime.ToString(Configuration.FileOrganizationFile.Value) + selectedFile.Extension;
                    }
                    else
                    {
                        path = Configuration.ScreenshotDirectory.Value + "/" + selectedFile.Name;
                    }
                }
                else
                {
                    if (!Directory.Exists(Configuration.ScreenshotDirectory.Value + "/Favorites"))
                    {
                        Directory.CreateDirectory(Configuration.ScreenshotDirectory.Value + "/Favorites");
                    }
                    path = Configuration.ScreenshotDirectory.Value + "/Favorites/" + selectedFile.Name;
                }

                MenuManager.FavoriteButton.ButtonComponent.enabled = false;

                File.Move(selectedFile.FullName, path);

                selectedFile = new FileInfo(path);

                ReloadFiles().NoAwait(new Action(() => MenuManager.FavoriteButton.ButtonComponent.enabled = true));
            }
        }

        public static void ShowFileInExplorer()
        {
            if (selectedFile != null && !IsReloading)
            {
                if (!File.Exists(selectedFile.FullName))
                {
                    MelonLogger.Error("File not found: " + selectedFile.FullName);
                    return;
                }
                Process.Start("explorer.exe", "/select, \"" + selectedFile.FullName + "\""); ;
            }
        }

        public static void RotateImage(Direction direction)
        {
            if (fileChache.Count > currentIndex && !IsReloading)
            {
                FileInfo fileInfo = fileChache[currentIndex];
                if (!File.Exists(fileInfo.FullName))
                {
                    MelonLogger.Error("File not found: " + fileInfo.FullName);
                    return;
                }

                DateTime lastWriteTime = fileInfo.LastWriteTime;

                System.Drawing.Image image = System.Drawing.Image.FromFile(fileInfo.FullName);

                string data = null;
                if (fileInfo.Extension.Equals(Extensions[0]))
                    data = FileDataHandler.ReadPngChunk(fileInfo.FullName);
                else if (fileInfo.Extension.Equals(Extensions[1]))
                    data = FileDataHandler.ReadJpegProperty(image);

                if (direction == Direction.Right)
                    image.RotateFlip(System.Drawing.RotateFlipType.Rotate90FlipNone);
                else
                    image.RotateFlip(System.Drawing.RotateFlipType.Rotate270FlipNone);

                if (data != null)
                {
                    if (fileInfo.Extension.Equals(Extensions[0]))
                    {
                        image.Save(fileInfo.FullName, System.Drawing.Imaging.ImageFormat.Png);
                        bool result = FileDataHandler.WritePngChunk(fileInfo.FullName, data);

                        if (!result)
                            MelonLogger.Warning("Failed to write image metadata. Image will be saved without any data.");
                    }
                    else if (fileInfo.Extension.Equals(Extensions[1]))
                    {
                        image.Save(fileInfo.FullName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                }
                else
                {
                    image.Save(fileInfo.FullName);
                }

                image.Dispose();
                File.SetLastWriteTime(fileInfo.FullName, lastWriteTime);
                Update(true);
            }
        }

        public static void UploadToGallery(Action onUploading = null, Action onSuccess = null, Action onError = null)
        {
            if (selectedFile != null && !IsReloading)
            {
                if (!File.Exists(selectedFile.FullName))
                {
                    MelonLogger.Error("File not found: " + selectedFile.FullName);
                    onError?.Invoke();
                    return;
                }

                MelonLogger.Msg("Uploading " + selectedFile.Name + " to VRChat Gallery...");
                onUploading?.Invoke();
                UnhollowerBaseLib.Il2CppStructArray<byte> data = File.ReadAllBytes(selectedFile.FullName);
                ApiImage.UploadImage(data, new Action<ApiModelContainer<ApiFile>>(file =>
                {
                    MelonLogger.Msg("File " + selectedFile.Name + " was uploaded to VRChat Gallery.");
                    onSuccess?.Invoke();
                }), new Action<string>(err =>
                {
                    MelonLogger.Msg("Error while uploading file " + selectedFile.Name + " to VRChat Gallery. Error: " + err);
                    onError?.Invoke();
                }));
            }
        }

        public static void SendToDiscordWebhook(string webhookName, DiscordWebhookConfiguration webhookConfig, Action onUploading = null, Action onSuccess = null, Action onError = null)
        {
            if (selectedFile != null && !IsReloading)
            {
                if (!File.Exists(selectedFile.FullName))
                {
                    MelonLogger.Error("File not found: " + selectedFile.FullName);
                    onError?.Invoke();
                    return;
                }

                MelonLogger.Msg("Uploading " + selectedFile.Name + " to Discord [" + webhookName + "]...");
                onUploading?.Invoke();

                DateTime creationTime = Configuration.UseFileCreationTime.Value ? selectedFile.CreationTime : selectedFile.LastWriteTime;

                string username = webhookConfig.SetUsername.Value ? (webhookConfig.Username.Value.Replace("{vrcname}", APIUser.CurrentUser.displayName)) : null;

                string message = null;
                if (webhookConfig.SetMessage.Value)
                {
                    string world = GetWorldTag(selectedFile, System.Drawing.Image.FromFile(selectedFile.FullName), true);
                    message = webhookConfig.Message.Value.Replace("{vrcname}", APIUser.CurrentUser.displayName)
                        .Replace("{creationtime}", creationTime.ToString(webhookConfig.CreationTimeFormat.Value))
                        .Replace("{world}", world ?? "<No World Tag>");

                    int iterations = 0;
                    while (message.Contains("{timestamp:"))
                    {
                        iterations++;
                        if (iterations > 32)
                            break;

                        int startIndex = message.IndexOf("{timestamp:");
                        int typeIndex = startIndex + 11;
                        int endIndex = message.IndexOf("}", startIndex) + 1;

                        char type = message[typeIndex];

                        message = message.Remove(startIndex, endIndex - startIndex);
                        message = message.Insert(startIndex, "<t:" + (long)(creationTime.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds + ":" + type + ">");
                    }
                }

                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "/Executables/DiscordWebhook.exe";
                processStartInfo.Arguments = "\""
                    + webhookConfig.WebhookURL.Value + "\" \""
                    + webhookConfig.SetUsername.Value.ToString() + "\" \""
                    + username + "\" \""
                    + webhookConfig.SetMessage.Value.ToString() + "\" \""
                    + message + "\" \""
                    + selectedFile.FullName + "\" \""
                    + selectedFile.Name + "\" "
                    + webhookConfig.CompressionThreshold.Value;

                AsyncProcessProvider.StartProcess(processStartInfo, new Action<bool, int>((hasExited, exitCode) =>
                {
                    if (hasExited && exitCode == 0)
                    {
                        MelonLogger.Msg("File " + selectedFile.Name + " was uploaded to Discord [" + webhookName + "].");
                        onSuccess.Invoke();
                    }
                    else
                    {
                        MelonLogger.Error("Error while uploading file " + selectedFile.Name + " to Discord [" + webhookName + "]. Process exited with " + exitCode);
                        onError.Invoke();
                    }
                }), "DiscordWebhook").NoAwait();
            }
        }

        public static bool ImportToSteam()
        {
            if (selectedFile != null && !IsReloading)
            {
                if (!File.Exists(selectedFile.FullName))
                {
                    MelonLogger.Error("File not found: " + selectedFile.FullName);
                    return false;
                }

                System.Drawing.Image image = System.Drawing.Image.FromFile(selectedFile.FullName);
                return SteamIntegration.ImportScreenshot(selectedFile.FullName, GetWorldTag(selectedFile, image, true));
            }
            return false;
        }

        public static void DeletePicture()
        {
            if (selectedFile != null && !IsReloading)
            {
                if (!File.Exists(selectedFile.FullName))
                {
                    MelonLogger.Error("File not found: " + selectedFile.FullName);
                    Update(false);
                    return;
                }
                File.Delete(selectedFile.FullName);
                ReloadFiles().NoAwait();
            }
        }

        public static string GetWorldTag(FileInfo fileInfo, System.Drawing.Image image, bool defaultNull = false)
        {
            string data = null;
            if (fileInfo.Extension.Equals(Extensions[0]))
            {
                image.Dispose();
                data = FileDataHandler.ReadPngChunk(fileInfo.FullName);
            }
            else if (fileInfo.Extension.Equals(Extensions[1]))
            {
                data = FileDataHandler.ReadJpegProperty(image);
                image.Dispose();
            }

            if (data != null)
            {
                string[] dataArray = data.Split(',');
                if (dataArray.Length >= 3)
                {
                    string world = dataArray.Skip(3).First();
                    if (world.Contains("|"))
                        world = world.Substring(0, world.IndexOf("|"));
                    data = world;
                }
                else
                {
                    data = defaultNull ? null : "Invalid tag";
                }
            }
            else
            {
                data = defaultNull ? null : "Not found";
            }

            return data;
        }

        public static async Task ReloadFiles()
        {
            if (IsReloading)
                return;
            IsReloading = true;
            InfoText = "Indexing...";
            UiManager.PushQuickMenuAlert("Indexing files...");

            await TaskProvider.YieldToBackgroundTask();

            if (CurrentCategory == ImageMenuCategory.Today)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectory.Value);
                DateTime dateTime = DateTime.Now.Date.AddHours(Configuration.TodayHourOffset.Value);
                fileChache = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => Extensions.Any(f.Extension.EndsWith) && !f.Directory.Name.Equals("Favorites") && (Configuration.UseFileCreationTime.Value ? f.CreationTime : f.LastWriteTime) >= dateTime).OrderBy(f => (Configuration.UseFileCreationTime.Value ? f.CreationTime : f.LastWriteTime)).ToList();
            }
            else if (CurrentCategory == ImageMenuCategory.Yesterday)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectory.Value);
                DateTime dateTime = DateTime.Now.Date.AddHours(Configuration.TodayHourOffset.Value).AddDays(-1);
                fileChache = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => Extensions.Any(f.Extension.EndsWith) && !f.Directory.Name.Equals("Favorites") && (Configuration.UseFileCreationTime.Value ? f.CreationTime : f.LastWriteTime) >= dateTime).OrderBy(f => (Configuration.UseFileCreationTime.Value ? f.CreationTime : f.LastWriteTime)).ToList();
            }
            else if (CurrentCategory == ImageMenuCategory.Favorites)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectory.Value + "\\Favorites");
                if (!Directory.Exists(Configuration.ScreenshotDirectory.Value + "\\Favorites"))
                    Directory.CreateDirectory(Configuration.ScreenshotDirectory.Value + "\\Favorites");
                fileChache = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => Extensions.Any(f.Extension.EndsWith)).OrderBy(f => (Configuration.UseFileCreationTime.Value ? f.CreationTime : f.LastWriteTime)).ToList();
            }
            else
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectory.Value);
                fileChache = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => Extensions.Any(f.Extension.EndsWith) && !f.Directory.Name.Equals("Favorites")).OrderBy(f => (Configuration.UseFileCreationTime.Value ? f.CreationTime : f.LastWriteTime)).ToList();
            }

            if (currentIndex >= fileChache.Count)
                currentIndex = 0;

            IsReloading = false;
            await TaskProvider.YieldToMainThread();
            Update(true);
            UiManager.PushQuickMenuAlert("Finished indexing.");
        }
    }
}
