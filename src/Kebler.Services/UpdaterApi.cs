using System;
using System.Linq;
using System.Threading.Tasks;
using static Kebler.Services.GitHubApi;

namespace Kebler.Services
{
    public static class UpdaterApi
    { 
        public static async Task<Tuple<bool, Release>> Check(string user, string repository, Version currentVersion, bool preRelease = false)
        {
            if (preRelease)
            {
                try
                {
                    var gitHub = new GitHubApi();
                    var rel = await gitHub.GetLatestPreReleaseJSONAsync(user, repository);
                    var release = rel.First(x => x.prerelease);

                    return new Tuple<bool, Release>(currentVersion < release.name, release);
                }
                catch (Exception ex)
                {
                    return new Tuple<bool, Release>(false, null);
                }

            }
            else
            {
                try
                {
                    var gitHub = new GitHubApi();
                    var release = await gitHub.GetLatestReleaseJSONAsync(user, repository);
                    return new Tuple<bool, Release>(currentVersion < release.name, release);
                }
                catch (Exception ex)
                {
                    return new Tuple<bool, Release>(false, null);
                }
            }

        }
    }
}