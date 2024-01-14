using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MultiOpener.Entities.Opened.ResetTracker
{
    public sealed class ResetTrackerExternalSource : OpenedResetTrackerProcess
    {
        private readonly string _trackerId;

        private readonly struct ResetAnalyticsResponse
        {
            public bool success { get; init; }
            public List<Session> session { get; init; }
        }
        private readonly struct Session
        {
            public Ops ops { get; init; }
        }
        private readonly struct Ops
        {
            public Tl[] tl { get; init; }
            public int rc { get; init; }
            public float fnph { get; init; }
            public long tp { get; init; }
            public long wt { get; init; }
        }
        private readonly struct Tl
        {
            public int time { get; init; }
            public int total { get; init; }
        }


        public ResetTrackerExternalSource(string trackerID) : base()
        {
            SessionData.UsingBuiltIn = false;
            _trackerId = trackerID;
        }

        public override void ActivateTracker()
        {
            if (IsTracking) return;

            _source = new();
            _token = _source.Token;

            IsTracking = true;
            UpdateStatus();

            StartViewModel.Log("Activated Tracker");
            _trackerTask = Task.Run(TrackStats, _token);
        }
        public override void DeactivateTracker()
        {
            if (_token.IsCancellationRequested || !IsTracking) return;

            _source.Cancel();
            IsTracking = false;

            if (_trackerTask is { IsCompleted: false })
            {
                try
                {
                    _trackerTask.Wait(_token);
                }
                catch { }
            }

            _source.Dispose();
            SessionData.Clear();
            StartViewModel.Log("Deactivated Tracker");
        }

        private async Task TrackStats()
        {
            updateFrequencySize = App.Config.UpdateResetTrackerFrequency / 1000;
            TimeToUpdateStats = updateFrequencySize;

            while (IsTracking)
            {
                try
                {
                    TimeToUpdateStats -= 1;
                    await Task.Delay(TimeSpan.FromSeconds(1), _token);
                }
                catch { break; }


                if (TimeToUpdateStats == 0)
                {
                    await OnTracking();
                    TimeToUpdateStats = updateFrequencySize;
                }
            }
        }

        private async Task OnTracking()
        {
            string apiUrl = $"https://jojoe-stats.vercel.app/api/sheet/{_trackerId}";

            try
            {
                using var httpClient = new HttpClient();
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl, _token);
                if (!response.IsSuccessStatusCode)
                    StartViewModel.Log("Error fetching stats: " + response.StatusCode, ConsoleLineOption.Error);

                string responseBody = await response.Content.ReadAsStringAsync(_token);
                ResetAnalyticsResponse apiResponse = JsonSerializer.Deserialize<ResetAnalyticsResponse>(responseBody)!;

                if (!apiResponse.success) return;

                Ops ops = apiResponse.session[0].ops;
                SessionData.WallResets = ops.rc;
                SessionData.RealNetherPerHour = ops.fnph;
                SessionData.TotalRTAPlayTimeMiliseconds = ops.tp;
                SessionData.WallTimeMiliseconds = ops.wt;

                Tl[] tl = ops.tl;

                SessionData.IronPickaxeCount = tl[2].total;

                SessionData.NetherEntersCount = tl[3].total;
                SessionData.NetherEntersTime = tl[3].time;

                SessionData.FirstStructureEntersCount = tl[4].total;
                SessionData.FirstStructureEntersTime = tl[4].time;

                SessionData.SecondStructureEntersCount = tl[5].total;
                SessionData.SecondStructureEntersTime = tl[5].time;

                SessionData.NetherExitEntersCount = tl[6].total;
                SessionData.NetherExitEntersTime = tl[6].time;

                SessionData.StrongholdEntersCount = tl[7].total;
                SessionData.StrongholdEntersTime = tl[7].time;

                SessionData.EndEntersCount = tl[8].total;
                SessionData.EndEntersTime = tl[8].time;
            }
            catch
            {
                StartViewModel.Log("Error fetching stats", ConsoleLineOption.Error);
            }
            lock (this)
            {
                SessionData.Update();
            }
        }

        public override async Task<bool> OpenProcess(CancellationToken token = default)
        {
            await base.OpenProcess(token);
            ActivateTracker();
            return await Task.FromResult(true);
        }
    }
}