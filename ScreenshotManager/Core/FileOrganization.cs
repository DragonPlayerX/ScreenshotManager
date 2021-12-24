using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using HarmonyLib;
using VRC.UserCamera;

using ScreenshotManager.Config;
using ScreenshotManager.Utils;

namespace ScreenshotManager.Core
{
    public static class FileOrganization
    {

        // This code is inspired from PhotoOrganization (https://github.com/dave-kun/PhotoOrganization)

        public static void OrganizeAll()
        {
            MelonLogger.Msg("Organizing files...");
            DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectory.Value);
            FileInfo[] files = directoryInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).ToArray();
            int movedFiles = 0;
            foreach (FileInfo file in files)
            {
                DateTime creationTime = Configuration.UseFileCreationTime.Value ? file.CreationTime : file.LastWriteTime;
                string directory = Configuration.ScreenshotDirectory.Value + "/" + creationTime.ToString(Configuration.FileOrganizationFolder.Value);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string newFile = directory + "/VRChat_" + creationTime.ToString(Configuration.FileOrganizationFile.Value) + file.Extension;
                if (!File.Exists(newFile))
                {
                    File.Move(file.FullName, newFile);
                    movedFiles++;
                }
            }

            MelonLogger.Msg("Organized " + movedFiles + " files.");
        }

        public static void Reset()
        {
            MelonLogger.Msg("Reset organization...");
            DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectory.Value);
            FileInfo[] files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => !f.Directory.Name.Equals("Favorites") && !Configuration.ScreenshotDirectory.Value.EndsWith(f.Directory.Name)).ToArray();
            int movedFiles = 0;
            foreach (FileInfo file in files)
            {
                DateTime creationTime = Configuration.UseFileCreationTime.Value ? file.CreationTime : file.LastWriteTime;
                string newFile = Configuration.ScreenshotDirectory.Value + "/VRChat_" + creationTime.ToString("yyyy-MM-dd_HH-mm-ss.fff") + file.Extension;
                if (!File.Exists(newFile))
                {
                    File.Move(file.FullName, newFile);
                    movedFiles++;
                }
            }

            MelonLogger.Msg("Moved " + movedFiles + " files back to main directory.");

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

            MelonLogger.Msg("Deleted " + deletedDirectories + " empty folders.");
        }

        public static void PatchMethod()
        {
            MethodInfo filePathMethod = typeof(CameraUtil).GetMethods(BindingFlags.Static | BindingFlags.Public).First(method => method.GetParameters().Length == 2 && XrefUtils.CheckForString(method, "{0}{1}VRChat_{2}x{3}_{4}.png"));
            ScreenshotManagerMod.Instance.HarmonyInstance.Patch(filePathMethod, new HarmonyMethod(typeof(FileOrganization).GetMethod(nameof(FilePathPatch), BindingFlags.Static | BindingFlags.NonPublic)));

            MelonLogger.Msg("Patched screenshot file method.");

            MethodInfo fileDirectoryMethod = typeof(CameraUtil).GetMethods(BindingFlags.Static | BindingFlags.Public).First(method => method.GetParameters().Length == 0 && XrefUtils.CheckForString(method, "yyyy-MM"));
            ScreenshotManagerMod.Instance.HarmonyInstance.Patch(fileDirectoryMethod, new HarmonyMethod(typeof(FileOrganization).GetMethod(nameof(DirectoryPathPatch), BindingFlags.Static | BindingFlags.NonPublic)));

            MelonLogger.Msg("Patched screenshot directory method.");

#if DEBUG
            MelonLogger.Msg("FileName Method: " + filePathMethod?.Name);
            MelonLogger.Msg("FolderName Method: " + fileDirectoryMethod?.Name);
#endif
        }

        private static bool FilePathPatch(ref string __result)
        {
            if (Configuration.FileOrganization.Value)
                __result = Configuration.ScreenshotDirectory.Value + "/" + DateTime.Now.ToString(Configuration.FileOrganizationFolder.Value) + "/VRChat_" + DateTime.Now.ToString(Configuration.FileOrganizationFile.Value) + ".png";
            else
                __result = Configuration.ScreenshotDirectory.Value + "/VRChat_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fff") + ".png";
            return false;
        }

        private static bool DirectoryPathPatch(ref string __result)
        {
            if (Configuration.FileOrganization.Value)
                __result = Configuration.ScreenshotDirectory.Value + "/" + DateTime.Now.ToString(Configuration.FileOrganizationFolder.Value);
            else
                __result = Configuration.ScreenshotDirectory.Value;
            return false;
        }
    }
}
