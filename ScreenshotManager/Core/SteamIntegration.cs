using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MelonLoader;

namespace ScreenshotManager.Core
{
    public static class SteamIntegration
    {

        private const string STEAM_API_64 = "VRChat_Data\\Plugins\\x86_64\\steam_api64.dll";
        private static readonly string SteamClientVersion = "SteamClient020";
        private static readonly string ScreenshotInterfaceVersion = "STEAMSCREENSHOTS_INTERFACE_VERSION003";

        private static IntPtr steamScreenshotsInterfacePtr;

        public static bool Enabled { get; private set; }

        public static void Init()
        {
            MelonLogger.Msg("Loading Steam API...");
            try
            {
                int pipe = SteamAPI_GetHSteamPipe();
                int user = SteamAPI_GetHSteamUser();
                MelonLogger.Msg("[Steam API] SteamPipe: " + pipe + " SteamUser: " + user);

                if (pipe == 0 || user == 0)
                {
                    Enabled = false;
                    MelonLogger.Warning("Steam API is invalid. The Steam integration is now disabled.");
                    return;
                }

                IntPtr clientPtr = SteamInternal_CreateInterface(SteamClientVersion);
                MelonLogger.Msg("[Steam API] SteamClient: " + clientPtr);

                if (clientPtr == IntPtr.Zero)
                {
                    Enabled = false;
                    MelonLogger.Warning("Steam Client is invalid. The Steam integration is now disabled.");
                    return;
                }

                steamScreenshotsInterfacePtr = ISteamClient_GetISteamScreenshots(clientPtr, user, pipe, ScreenshotInterfaceVersion);
                MelonLogger.Msg("[Steam API] ScreenshotInterface: " + steamScreenshotsInterfacePtr);

                if (steamScreenshotsInterfacePtr == IntPtr.Zero)
                {
                    Enabled = false;
                    MelonLogger.Warning("Screenshot Interface is invalid. The Steam integration is now disabled.");
                    return;
                }

                Enabled = true;
            }
            catch (Exception e)
            {
                Enabled = false;
                MelonLogger.Error(e);
                MelonLogger.Error("An error occurred while loading Steam API. The Steam integration is now disabled.");
            }
        }

        public static bool ImportScreenshot(string file, string location = null, List<ulong> taggedUsers = null)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(file);

                if (!fileInfo.Extension.ToLower().Equals(".png") && fileInfo.Extension.ToLower().Equals(".jpeg") && !fileInfo.Extension.ToLower().Equals(".jpg"))
                {
                    MelonLogger.Warning("Type of " + file + " has to be .png or .jpeg");
                    return false;
                }

                string tempFile = Path.GetTempPath() + fileInfo.Name;
                File.Copy(file, tempFile);

                uint handle = SteamAPI_ISteamScreenshots_AddScreenshotToLibrary(steamScreenshotsInterfacePtr, tempFile, null, 1920, 1080);
                MelonLogger.Msg("[Steam API] Screenshot Handle: " + handle);
                if (location != null)
                    ISteamScreenshots_SetLocation(steamScreenshotsInterfacePtr, handle, location);
                
                if (taggedUsers != null)
                {
                    if (taggedUsers.Count > 32)
                        taggedUsers.RemoveRange(32, taggedUsers.Count - 32);

                    foreach (ulong userId in taggedUsers)
                    {
                        ISteamScreenshots_TagUser(steamScreenshotsInterfacePtr, handle, userId);
                    }
                }

                File.Delete(tempFile);
                return true;
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                MelonLogger.Error("An error occurred while importing " + file + " to Steam.");
                return false;
            }
        }

        [DllImport(STEAM_API_64, EntryPoint = "SteamAPI_GetHSteamPipe", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SteamAPI_GetHSteamPipe();

        [DllImport(STEAM_API_64, EntryPoint = "SteamAPI_GetHSteamUser", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SteamAPI_GetHSteamUser();

        [DllImport(STEAM_API_64, EntryPoint = "SteamInternal_CreateInterface", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SteamInternal_CreateInterface([MarshalAs(UnmanagedType.LPStr)] string version);

        [DllImport(STEAM_API_64, EntryPoint = "SteamAPI_ISteamClient_GetISteamScreenshots", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr ISteamClient_GetISteamScreenshots(IntPtr ptr, int hSteamuser, int hSteamPipe, [MarshalAs(UnmanagedType.LPStr)] string version);

        [DllImport(STEAM_API_64, EntryPoint = "SteamAPI_ISteamScreenshots_AddScreenshotToLibrary", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SteamAPI_ISteamScreenshots_AddScreenshotToLibrary(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string file, [MarshalAs(UnmanagedType.LPStr)] string thumbnail, int width, int height);

        [DllImport(STEAM_API_64, EntryPoint = "SteamAPI_ISteamScreenshots_SetLocation", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ISteamScreenshots_SetLocation(IntPtr ptr, uint hScreenshot, [MarshalAs(UnmanagedType.LPStr)] string location);

        [DllImport(STEAM_API_64, EntryPoint = "SteamAPI_ISteamScreenshots_TagUser", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool ISteamScreenshots_TagUser(IntPtr ptr, uint hScreenshot, ulong steamID);

    }
}
