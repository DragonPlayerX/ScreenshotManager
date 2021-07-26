using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MelonLoader;
using HarmonyLib;
using UnhollowerRuntimeLib.XrefScans;

using ScreenshotManager.Config;

namespace ScreenshotManager.Core
{
    public static class FileOrganization
    {

        // This code is inspired from PhotoOrganization (https://github.com/dave-kun/PhotoOrganization)

        public static void OrganizeAll()
        {
            MelonLogger.Msg("Organizing files...");
            DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectoryEntry.Value);
            FileInfo[] files = directoryInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).ToArray();
            int movedFiles = 0;
            foreach (FileInfo file in files)
            {
                DateTime creationTime = file.LastWriteTime;
                string directory = Configuration.ScreenshotDirectoryEntry.Value + "/" + creationTime.ToString(Configuration.FileOrganizationFolderEntry.Value);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string newFile = directory + "/VRChat_" + creationTime.ToString(Configuration.FileOrganizationFileEntry.Value) + file.Extension;
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
            MelonLogger.Msg("Resetting organization...");
            DirectoryInfo directoryInfo = new DirectoryInfo(Configuration.ScreenshotDirectoryEntry.Value);
            FileInfo[] files = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories).Where(f => !f.Directory.Name.Equals("Favorites") && !Configuration.ScreenshotDirectoryEntry.Value.EndsWith(f.Directory.Name)).ToArray();
            int movedFiles = 0;
            foreach (FileInfo file in files)
            {
                DateTime creationTime = file.LastWriteTime;
                string newFile = Configuration.ScreenshotDirectoryEntry.Value + "/VRChat_" + creationTime.ToString("yyyy-MM-dd_HH-mm-ss.fff") + file.Extension;
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
            MethodInfo method = typeof(VRC.UserCamera.CameraUtil).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(it => it.GetParameters().Length == 2 && XrefScanner.XrefScan(it).Any(jt => jt.Type == XrefType.Global && jt.ReadAsObject()?.ToString() == "{0}/VRChat/VRChat_{1}x{2}_{3}.png"));
            ScreenshotManagerMod.Instance.HarmonyInstance.Patch(method, new HarmonyMethod(typeof(FileOrganization).GetMethod(nameof(DirectoryPatch), BindingFlags.Static | BindingFlags.NonPublic)));
            MelonLogger.Msg("Patched screenshot directory method.");
        }

        private static bool DirectoryPatch(ref string __result)
        {
            if (Configuration.FileOrganizationEntry.Value)
                __result = Configuration.ScreenshotDirectoryEntry.Value + "/" + DateTime.Now.ToString(Configuration.FileOrganizationFolderEntry.Value) + "/VRChat_" + DateTime.Now.ToString(Configuration.FileOrganizationFileEntry.Value) + ".png";
            else
                __result = Configuration.ScreenshotDirectoryEntry.Value + "/VRChat_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fff") + ".png";
            return false;
        }
    }
}
