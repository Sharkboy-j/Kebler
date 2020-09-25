using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kebler.Services
{
    public static class UpdaterApi
    {
        static JObject latestReleaseJson;
        public static async Task<(bool, Version)> CheckAsync(string user, string repository, Version currentVersion)
        {
            try
            {
                var gitHub = new GitHubApi();
                latestReleaseJson = await gitHub.GetLatestReleaseJSONAsync(user, repository);
                var version = GitHubApi.ExtractVersion(latestReleaseJson);

                return (currentVersion < version, version);
            }
            catch(Exception ex)
            {
                return (false,new Version());
            }
          

        }

        public static (bool, Version) Check(string user, string repository, Version currentVersion)
        {
            try
            {
                var gitHub = new GitHubApi();
                latestReleaseJson = gitHub.GetLatestReleaseJSONAsync(user, repository).Result;
                var version = GitHubApi.ExtractVersion(latestReleaseJson);

                return (currentVersion < version, version);
            }
            catch (Exception ex)
            {
                return (false, new Version());
            }


        }

        public static string GetlatestUri()
        {
            var updateUrl = GitHubApi.ExtractDownloadUrl(latestReleaseJson);
            return updateUrl;
        }
    }
}
