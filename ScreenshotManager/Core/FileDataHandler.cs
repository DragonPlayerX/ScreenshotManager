using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MelonLoader;
using HarmonyLib;

using ScreenshotFileHandler = VRC.UserCamera.CameraUtil.ObjectNPrivateSealedStObUnique;

namespace ScreenshotManager.Core
{
    public static class FileDataHandler
    {

        private static readonly byte[] PngSignature = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };

        public static bool UseLFS = false;

        public static void Init()
        {
            UseLFS = MelonHandler.Mods.Any(mod => mod.Info.Name.Equals("Lag Free Screenshots"));

            if (!UseLFS)
                ScreenshotManagerMod.Instance.HarmonyInstance.Patch(typeof(ScreenshotFileHandler).GetMethod("_TakeScreenShot_b__1"), postfix: new HarmonyMethod(typeof(FileDataHandler).GetMethod(nameof(DefaultVRCScreenshotResultPatch), BindingFlags.Static | BindingFlags.NonPublic)));
        }

        private static void DefaultVRCScreenshotResultPatch(ScreenshotFileHandler __instance) => ImageHandler.AddFile(new FileInfo(__instance.field_Public_String_0));

        public static string ReadPngChunk(string file)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(file);

                byte[] fileSignature = bytes.Take(8).ToArray();

                if (!Enumerable.SequenceEqual(fileSignature, PngSignature))
                {
                    MelonLogger.Warning("No PNG signature found in file: " + file);
                    return null;
                }

                // Skipping file signature
                int index = 8;

                while (index < bytes.Length)
                {
                    // Reading length of data and type of chunk
                    byte[] lengthDataField = bytes.Skip(index).Take(4).ToArray();
                    byte[] typeDataField = bytes.Skip(index + 4).Take(4).ToArray();
                    int length = (lengthDataField[0] << 24) | (lengthDataField[1] << 16) | (lengthDataField[2] << 8) | lengthDataField[3];
                    string type = Encoding.UTF8.GetString(typeDataField);

                    if (type.Equals("iTXt"))
                    {
                        // Skip keyword and flag data and encode content
                        int bytesToSkip = Encoding.UTF8.GetByteCount("Description") + 5;
                        byte[] dataField = bytes.Skip(index + 8 + bytesToSkip).Take(length - bytesToSkip).ToArray();
                        return Encoding.UTF8.GetString(dataField);
                    }

                    if (type.Equals("IEND"))
                        return null;

                    index += length + 12;
                }
                return null;
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                MelonLogger.Error("Error while reading png chunks");
                return null;
            }
        }

        public static string ReadJpegProperty(Image image)
        {
            try
            {
                // Reading EXIF property
                PropertyItem property = image.GetPropertyItem(0x9286);
                return property == null ? null : Encoding.Unicode.GetString(property.Value);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
