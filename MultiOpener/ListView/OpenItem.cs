using System.Collections.ObjectModel;
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

        public OpenItem(OpenItem item)
        {
            Name = item.Name;
            PathExe = item.PathExe;
            DelayBefore = item.DelayBefore;
            DelayAfter = item.DelayAfter;
            Type = item.Type;
        }
    }

    public class OpenInstance : OpenItem
    {
        public int Quantity { get; set; }
        public int DelayBetweenInstances { get; set; }
        public ObservableCollection<string> Names { get; set; }


        [JsonConstructor]
        public OpenInstance(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = default, int Quantity = 0, ObservableCollection<string> Names = null, int DelayBetweenInstances = 0) : base(Name, PathExe, DelayBefore, DelayAfter, Type)
        {
            this.Quantity = Quantity;
            if (Names == null)
                this.Names = new ObservableCollection<string>();
            else
                this.Names = Names;
            this.DelayBetweenInstances = DelayBetweenInstances;
        }

        public OpenInstance(OpenInstance instance) : base(instance)
        {
            Quantity = instance.Quantity;
            if (Names == null)
                Names = new ObservableCollection<string>();
            else
                Names = instance.Names;
            DelayBetweenInstances = instance.DelayBetweenInstances;
        }
    }
}
