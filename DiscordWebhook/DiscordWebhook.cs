using System;
using System.IO;
using System.Drawing;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;

namespace DiscordWebhook
{
    public static class DiscordWebhook
    {
        public static int Main(string[] args)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                MultipartFormDataContent data = new MultipartFormDataContent();

                if (bool.Parse(args[1]))
                    data.Add(new StringContent(args[2]), "username");
                if (bool.Parse(args[3]))
                    data.Add(new StringContent(args[4]), "content");

                byte[] fileBytes = File.ReadAllBytes(args[5]);

                int compressionThreshold = int.Parse(args[7], CultureInfo.InvariantCulture);
                if (compressionThreshold != -1 && fileBytes.Length > compressionThreshold * 1024 * 1024)
                {
                    int oldLength = fileBytes.Length;

                    Image image = Image.FromFile(args[5]);
                    string tempFile = Path.GetTempPath() + Guid.NewGuid() + ".jpeg";
                    image.Save(tempFile, System.Drawing.Imaging.ImageFormat.Jpeg);
                    image.Dispose();

                    fileBytes = File.ReadAllBytes(tempFile);
                    File.Delete(tempFile);

                    Console.WriteLine("Image was compressed from " + (oldLength / 1024f / 1024f).ToString("0.00", CultureInfo.InvariantCulture) + " MB to " + (fileBytes.Length / 1024f / 1024f).ToString("0.00", CultureInfo.InvariantCulture) + " MB.");
                }

                data.Add(new ByteArrayContent(fileBytes, 0, fileBytes.Length), "Picture", args[6]);

                Task<HttpResponseMessage> responseTask = httpClient.PostAsync(args[0], data);
                responseTask.Wait();

                httpClient.Dispose();

                if (responseTask.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.Error.WriteLine("File is too large. " + (fileBytes.Length / 1024f / 1024f).ToString("0.00", CultureInfo.InvariantCulture) + " MB");
                    return 1;
                }
            }
            catch (Exception e)
            {
                Console.Error.Write(e);
                return 1;
            }
            return 0;
        }
    }
}
