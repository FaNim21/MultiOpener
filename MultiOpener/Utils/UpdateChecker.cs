using NuGet.Versioning;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Utils
{
    public class GitHubRelease
    {
        public string tag_name { get; set; }
        public string name { get; set; }
        public string body { get; set; }
        public string html_url { get; set; }
        // Add other properties as needed
    }

    public class UpdateChecker
    {
        private const string OWNER = "FaNim21";
        private const string REPO = "MultiOpener";

        private const string GitHubApiUrl = $"https://api.github.com/repos/{OWNER}/{REPO}/releases";

        public async Task CheckForUpdates()
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MultiOpener"); // Add your app name as User Agent
                var response = await httpClient.GetAsync(GitHubApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    GitHubRelease release = null;
                    try
                    {
                        //TODO: 2 nie dziala deserializowanie json przez fakt ze ten string jest bledny?
                        release = JsonSerializer.Deserialize<GitHubRelease>(json);
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }

                    Trace.WriteLine(release.tag_name);

                    if (IsNewerVersionAvailable(release.tag_name, Consts.Version))
                        ShowUpdateNotification(release.name, release.body, release.html_url);
                }
            }
        }

        private bool IsNewerVersionAvailable(string latestVersion, string currentVersion)
        {
            var latest = new NuGetVersion(latestVersion);
            var current = new NuGetVersion(currentVersion);
            return latest > current;
        }

        private void ShowUpdateNotification(string releaseName, string releaseNotes, string releaseUrl)
        {
            Trace.WriteLine(releaseName + "\n" + releaseUrl);
        }
    }
}