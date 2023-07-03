using MultiOpener.ViewModels;
using NuGet.Versioning;
using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiOpener.Utils
{
    public class UpdateChecker
    {
        private const string OWNER = "FaNim21";
        private const string REPO = "MultiOpener";

        public async Task<bool> CheckForUpdates(string? version = null)
        {
            if (string.IsNullOrEmpty(version))
                version = Consts.Version[1..];

            try
            {
                var gitHubClient = new GitHubClient(new ProductHeaderValue("MultiOpener"));
                var releases = await gitHubClient.Repository.Release.GetAll(OWNER, REPO);

                var latestRelease = releases.OrderByDescending(r => r.PublishedAt).FirstOrDefault();

                if (latestRelease != null && !IsUpToDate(latestRelease.TagName, version))
                {
                    StartViewModel.Log($"Found new update - {latestRelease.TagName}", Entities.ConsoleLineOption.Warning);
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            StartViewModel.Log($"You are up to date - {version}");
            return false;
        }

        private bool IsUpToDate(string latestTag, string currentTag)
        {
            NuGetVersion latest = new(latestTag);
            NuGetVersion current = new(currentTag);

            return latest <= current;
        }
    }
}