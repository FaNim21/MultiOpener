using System.Collections.Generic;

namespace MultiOpener.Entities.Misc
{
    public class TrackedRunStats
    {
        /// <summary>
        /// DO OBECNEJ DATY ZALEZNEJ OD STREFY TRZEBA UZYC:
        /// TimeZoneInfo localTimeZone = TimeZoneInfo.Local;
        /// DateTime currentDateInLocalTimeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);
        /// </summary>
        public string? Date { get; set; }

        public string? TimeZone { get; set; }

        public string? RTA { get; set; }
        public string? IGT { get; set; }
        public string? RetimedIGT { get; set; }

        /* SPLITS */
        public string? NetherTime { get; set; }
        public string? Structure1 { get; set; }
        public string? Structure2 { get; set; }
        public string? NetherExit { get; set; }
        public string? Stronghold { get; set; }
        public string? EndEnter { get; set; }

        /* STATS */
        /// <summary>
        /// minecraft:blaze_rod w stats i PickedUp
        /// </summary>
        public int BlazeRods { get; set; }
        /// <summary>
        /// minecraft:blaze w stats i Killed
        /// </summary>
        public int KilledBlazes { get; set; }

        public int WallResetsSincePrevious { get; set; }
        public int PlayedSincePrev { get; set; }
        /// <summary>
        /// Total time minus break time
        /// </summary>
        public string? TimeSincePrevious { get; set; }
        public string? RTASincePrevious { get; set; }
        public string? BreakTimeSincePrevious { get; set; }
        public string? WallTimeSincePrevious { get; set; }


        /*public void UpdateSplits(List<(string name, int IGT)> timelines)
        {
            //TODO: 0 
        }*/
    }
}