using System.ComponentModel;
using System.Text.Json.Serialization;

namespace MultiOpener.ListView
{
    public enum OpenType
    {
        [Description("Normal")]
        Normal,
        [Description("Instances(MultiMC)")]
        InstancesMultiMC,
    }

    [JsonDerivedType(typeof(OpenItem), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(OpenInstance), typeDiscriminator: "instances")]
    public class OpenItem
    {
        public OpenType Type { get; set; }

        public string Name { get; set; }
        public string PathExe { get; set; }

        public int DelayBefore { get; set; }
        public int DelayAfter { get; set; }


        [JsonConstructor]
        public OpenItem(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default)
        {
            this.Name = Name;
            this.PathExe = PathExe;
            this.DelayBefore = DelayBefore;
            this.DelayAfter = DelayAfter;
            this.Type = Type;
        }
    }

    public class OpenInstance : OpenItem
    {
        public int Quantity { get; set; }
        public string[]? Names { get; set; }


        [JsonConstructor]
        public OpenInstance(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default, int Quantity = 0, string[]? Names = null) : base(Name, PathExe, DelayBefore, DelayAfter, Type)
        {
            this.Quantity = Quantity;
            if (Names == null)
                this.Names = new string[Quantity];
            else
                this.Names = Names;
        }
    }
}
