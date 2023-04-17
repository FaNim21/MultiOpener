using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NuGet.Versioning;
using Octokit;

namespace MultiOpener.Utils
{
    public class UpdateChecker
    {
        private const string OWNER = "FaNim21";
        private const string REPO = "MultiOpener";

        public async Task CheckForUpdates()
        {
            try
            {
                var gitHubClient = new GitHubClient(new ProductHeaderValue("YourAppName"));
                var releases = await gitHubClient.Repository.Release.GetAll(OWNER, REPO);

                var latestRelease = releases.OrderByDescending(r => r.PublishedAt).FirstOrDefault();

                if (latestRelease != null && !IsUpToDate(latestRelease.TagName))
                {
                    ShowNotification("Update Available", $"Version {latestRelease.TagName} is now available. Click here to download.", () =>
                    {
                        Process.Start(new ProcessStartInfo(latestRelease.HtmlUrl)
                        {
                            UseShellExecute = true
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check for updates: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsUpToDate(string latestTag)
        {
            NuGetVersion latest = new(latestTag);
            NuGetVersion current = new(Consts.Version[1..]);
            
            return latest > current;
        }

        private void ShowNotification(string title, string message, Action onClickAction)
        {
            onClickAction.Invoke();
        }
    }
}