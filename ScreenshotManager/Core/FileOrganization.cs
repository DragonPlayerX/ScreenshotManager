using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MelonLoader;
using HarmonyLib;
using VRC.UserCamera;

using ScreenshotManager.Config;
using ScreenshotManager.Utils;
using ScreenshotManager.UI;
using ScreenshotManager.Tasks;

namespace ScreenshotManager.Core
{
    public static class FileOrganization
    {

        // This code is inspired from PhotoOrganization (https://github.com/dave-kun/PhotoOrganization)

        public static bool IsWorking;

        public static async Task OrganizeAll()
        {
            IsWorking = true;
            MelonLogger.Msg("Organizing files...");
            UiManager.PushQuickMenuAlert("Organizing files...");

            await TaskProvider.YieldToBackgroundTask();

            DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectory.Value);
            FileInfo[] files = directoryInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).ToArray();
            int movedFiles = 0;
            foreach (FileInfo file in files)
            {
                DateTime creationTime = Configuration.UseFileCreationTime.Value ? file.CreationTime : file.LastWriteTime;
                string directory = Configuration.ScreenshotDirectory.Value + "\\" + creationTime.ToString(Configuration.FileOrganizationFolderTimeFormat.Value);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string newFile = directory + "\\" + CreateFileName(file.FullName, creationTime) + file.Extension;
                if (!File.Exists(newFile))
                {
                    File.Move(file.FullName, newFile);
                    movedFiles++;
                }
            }

            await TaskProvider.YieldToMainThread();
            IsWorking = false;
            MelonLogger.Msg("Organized " + movedFiles + " files.");
            UiManager.PushQuickMenuAlert("Organized " + movedFiles + " files.");
        }

        public static async Task Reset()
        {
            IsWorking = true;
            MelonLogger.Msg("Reset organization...");
            UiManager.PushQuickMenuAlert("Reset organization...");

            await TaskProvider.YieldToBackgroundTask();

            DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectory.Value);
            FileInfo[] files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => !f.Directory.Name.Equals("Favorites") && !Configuration.ScreenshotDirectory.Value.EndsWith(f.Directory.Name)).ToArray();
            int movedFiles = 0;
            foreach (FileInfo file in files)
            {
                DateTime creationTime = Configuration.UseFileCreationTime.Value ? file.CreationTime : file.LastWriteTime;
                string newFile = Configuration.ScreenshotDirectory.Value + "\\" + creationTime.ToString("yyyy-MM-dd_HH-mm-ss.fff") + file.Extension;
                if (!File.Exists(newFile))
                {
                    File.Move(file.FullName, newFile);
                    movedFiles++;
                }
            }

            DirectoryInfo[] directories = directoryInfo.EnumerateDirectories().Where(d => !d.Name.Equals("Favorites")).ToArray();
            int deletedDirectories = 0;
            foreach (DirectoryInfo directoryToDelete in directories)
            {
                if (!Directory.EnumerateFileSystemEntries(directoryToDelete.FullName).Any())
                {
                    Directory.Delete(directoryToDelete.FullName);
                    deletedDirectories++;
                }
            }

            await TaskProvider.YieldToMainThread();

            IsWorking = false;
            MelonLogger.Msg("Moved " + movedFiles + " files back to main directory.");
            MelonLogger.Msg("Deleted " + deletedDirectories + " empty folders.");
            UiManager.PushQuickMenuAlert("Moved " + movedFiles + " files back to main directory.");
        }

        public static void PatchMethod()
        {
            MethodInfo filePathMethod = MethodUtils.FindMethod("FileName", () => typeof(CameraUtil).GetMethods(BindingFlags.Static | BindingFlags.Public).First(method => method.GetParameters().Length == 2 && MethodUtils.ContainsString(method, "{0}{1}VRChat_{2}x{3}_{4}.png")));
            if (filePathMethod != null)
            {
                ScreenshotManagerMod.Instance.HarmonyInstance.Patch(filePathMethod, new HarmonyMethod(typeof(FileOrganization).GetMethod(nameof(FilePathPatch), BindingFlags.Static | BindingFlags.NonPublic)));
                MelonLogger.Msg("Patched screenshot file method.");
            }
            else
            {
                MelonLogger.Warning("Failed to patch the screenshot file method. Organization of files will break!");
            }

            MethodInfo fileDirectoryMethod = MethodUtils.FindMethod("FolderName", () => typeof(CameraUtil).GetMethods(BindingFlags.Static | BindingFlags.Public).First(method => method.GetParameters().Length == 0 && MethodUtils.ContainsString(method, "yyyy-MM")));
            if (fileDirectoryMethod != null)
            {
                ScreenshotManagerMod.Instance.HarmonyInstance.Patch(fileDirectoryMethod, new HarmonyMethod(typeof(FileOrganization).GetMethod(nameof(DirectoryPathPatch), BindingFlags.Static | BindingFlags.NonPublic)));
                MelonLogger.Msg("Patched screenshot directory method.");
            }
            else
            {
                MelonLogger.Warning("Failed to patch the screenshot directory method. Organization of files will break!");
            }
#if DEBUG
            MelonLogger.Msg("FileName Method: " + filePathMethod?.Name);
            MelonLogger.Msg("FolderName Method: " + fileDirectoryMethod?.Name);
#endif
        }

        private static bool FilePathPatch(ref string __result, int __0, int __1)
        {
            if (Configuration.FileOrganization.Value)
                __result = Configuration.ScreenshotDirectory.Value + "\\" + DateTime.Now.ToString(Configuration.FileOrganizationFolderTimeFormat.Value) + "\\" + CreateFileName(__0, __1) + ".png";
            else
                __result = Configuration.ScreenshotDirectory.Value + "\\VRChat_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fff") + ".png";
            return false;
        }

        private static bool DirectoryPathPatch(ref string __result)
        {
            if (Configuration.FileOrganization.Value)
                __result = Configuration.ScreenshotDirectory.Value + "\\" + DateTime.Now.ToString(Configuration.FileOrganizationFolderTimeFormat.Value);
            else
                __result = Configuration.ScreenshotDirectory.Value;
            return false;
        }

        public static string CreateFileName(string filePath, DateTime creationTime)
        {
            if (!Configuration.FileOrganizationNameFormat.Value.Contains("{resolution}"))
                return CreateFileName(creationTime, 0, 0);

            int width = 0;
            int height = 0;
            try
            {
                Image image = Image.FromFile(filePath);
                width = image.Width;
                height = image.Height;
                image.Dispose();
            }
            catch
            {
                MelonLogger.Warning("Failed to load image: " + filePath);
            }
            return CreateFileName(creationTime, width, height);
        }

        private static string CreateFileName(int width, int height) => CreateFileName(DateTime.Now, width, height);

        private static string CreateFileName(DateTime creationTime, int width, int height)
        {
            return Configuration.FileOrganizationNameFormat.Value.Replace("{timestamp}", creationTime.ToString(Configuration.FileOrganizationFileTimeFormat.Value)).Replace("{resolution}", width + "x" + height);
        }
    }
}
