using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Kebler.Services
{
    public class GitHubApi
    {
        private readonly HttpClient client = new HttpClient();

        public GitHubApi()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", "update from github release");
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        public async Task DownloadFile(string url, string destinationFileName)
        {
            using (var stream = await client.GetStreamAsync(url))
            {
                using (var file = new FileStream(destinationFileName, FileMode.Create))
                {
                    stream.CopyTo(file);
                }
            }
        }

        public static string ExtractDownloadUrl(JObject json)
        {
            return json["assets"].FirstOrDefault(x => x["name"].ToString().Contains("Release"))["browser_download_url"]
                .ToObject<string>();
        }

        public static Version ExtractVersion(JObject json)
        {
            return new Version(json["name"].ToObject<string>());
        }

        public async Task<JObject> GetJSONAsync(string gitHubDirectory)
        {
            return JObject.Parse(await client.GetStringAsync($"https://api.github.com/{gitHubDirectory}"));
        }

        public async Task<JObject> GetLatestReleaseJSONAsync(string user, string repository)
        {
            return await GetJSONAsync($"repos/{user}/{repository}/releases/latest");
        }
    }
}