using System;
using System.Net.Http;

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
                byte[] fileBytes = System.IO.File.ReadAllBytes(args[5]);
                data.Add(new ByteArrayContent(fileBytes, 0, fileBytes.Length), "Picture", args[6]);
                httpClient.PostAsync(args[0], data).Wait();
                httpClient.Dispose();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
            return 0;
        }
    }
}
