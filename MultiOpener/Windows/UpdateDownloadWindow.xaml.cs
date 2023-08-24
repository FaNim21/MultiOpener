using System.Windows;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Reflection;
using MultiOpener.ViewModels;
using Octokit;

namespace MultiOpener.Windows;

public partial class UpdateDownloadWindow : Window
{
    private readonly string GitHubApiUrl = "https://api.github.com/repos/FaNim21/MultiOpener/releases/latest";
    private readonly string multiOpenerExePath;
    private readonly string newExePath;

    private const string OWNER = "FaNim21";
    private const string REPO = "MultiOpener";


    public UpdateDownloadWindow()
    {
        InitializeComponent();

        multiOpenerExePath = Assembly.GetExecutingAssembly().Location;
        newExePath = Path.Combine(Consts.AppdataPath, "MultiOpener.exe");

        if (File.Exists(newExePath))
            File.Delete(newExePath);

        Task.Run(Initialize);
    }

    private async Task Initialize()
    {
        try
        {
            var latestRelease = await GetLatestReleaseInfo();
            var asset = latestRelease.Assets![0];
            if (asset == null)
            {
                StartViewModel.Log("MultiOpener.exe not found in latest release.", Entities.ConsoleLineOption.Error);
                return;
            }

            var assetBytes = await DownloadAssetBytes(asset.BrowserDownloadUrl);
            ReplaceExe(assetBytes);

            //File.Copy(tempExePath, multiOpenerExePath, true);
            //File.Delete(tempExePath);

            //Process.Start(OldExePath);

            //System.Windows.Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            StartViewModel.Log($"An error occurred: {ex.Message}", Entities.ConsoleLineOption.Error);
        }
    }

    private async Task<Release> GetLatestReleaseInfo()
    {
        var gitHubClient = new GitHubClient(new ProductHeaderValue("MultiOpener"));
        var releases = await gitHubClient.Repository.Release.GetAll(OWNER, REPO);
        return releases[0];
    }

    private async Task<byte[]> DownloadAssetBytes(string url)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsByteArrayAsync();
        else
            throw new Exception($"Failed to download asset. Status code: {response.StatusCode}");
    }

    private void ReplaceExe(byte[] assetBytes)
    {
        File.WriteAllBytes(newExePath, assetBytes);
    }
}
