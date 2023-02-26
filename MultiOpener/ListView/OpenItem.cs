using System.Text.Json.Serialization;

namespace MultiOpener.ListView
{
    public class OpenItem
    {
        public string Name { get; set; }
        public string PathExe { get; set; }
        public bool IsDelayAfter { get; set; }
        public int DelayAfter { get; set; }

        [JsonConstructor]
        public OpenItem(string Name = "", string PathExe = "", bool IsDelayAfter = false, int DelayAfter = 0)
        {
            this.Name = Name;
            this.PathExe = PathExe;
            this.IsDelayAfter = IsDelayAfter;
            this.DelayAfter = DelayAfter;
        }
    }
}
