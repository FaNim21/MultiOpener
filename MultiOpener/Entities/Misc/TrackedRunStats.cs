
namespace MultiOpener.Entities.Misc
{
    public class TrackedRunStats
    {
        public int Count { get; set; }

        public string? Date { get; set; }
        public string? TimeZone { get; set; }

        public string? RTA { get; set; }
        public string? IGT { get; set; }
        public string? RetimedIGT { get; set; }

        /* SPLITS */
        public string? NetherTime { get; set; }
        public string? Structure1 { get; set; }
        public string? Structure1Name { get; set; }
        public string? Structure2 { get; set; }
        public string? Structure2Name { get; set; }
        public string? NetherExit { get; set; }
        public string? Stronghold { get; set; }
        public string? EndEnter { get; set; }

        /* STATS */
        public int Deaths { get; set; }
        public int BlazeRods { get; set; }
        public int KilledBlazes { get; set; }
        public int ObsidianPlaced { get; set; }
        public int EnderEyeUsed { get; set; }

        public string? TimeSincePrevious { get; set; }

        public int PlayedSincePrev { get; set; }
        public string? RTASincePrevious { get; set; }

        public int WallResetsSincePrevious { get; set; }
        public string? WallTimeSincePrevious { get; set; }

        public string? BreakTimeSincePrevious { get; set; }
    }
}