using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

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

        public virtual bool Validate()
        {
            if (!File.Exists(PathExe))
            {
                MessageBox.Show($"You set a path to file that not exist in {Name}", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            if (DelayAfter < 0 || DelayBefore < 0)
            {
                MessageBox.Show($"You set delay lower than 0 in {Name}", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            if (DelayAfter > 999999 || DelayBefore > 99999)
            {
                MessageBox.Show($"Your delay can't be higher than 99999 in {Name}", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Warning);
                return true;
            }

            return false;
        }

        public virtual async Task Open()
        {

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

        public override bool Validate()
        {
            if (!Path.GetFileName(PathExe).Equals("MultiMC.exe"))
            {
                MessageBox.Show($"You set wrong path to MultiMC exe file in {Name}", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            if (DelayBetweenInstances > 99999 || DelayBetweenInstances < 0)
            {
                MessageBox.Show($"Delay between openning instances should be between 0 and 99999 in {Name}", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            if (Quantity > 32 || Quantity < 0)
            {
                MessageBox.Show($"Amount of instances should be between 0 and 32 in {Name}", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            int amount = 0;
            for (int i = 0; i < Names.Count; i++)
            {
                var current = Names[i];
                if (string.IsNullOrEmpty(current))
                    amount++;
            }

            if (amount > 0)
            {
                MessageBox.Show($"You didn't set names for {amount} instances in {Name}", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                return true;
            }

            return base.Validate();
        }

        public override async Task Open()
        {
            
        }
    }
}
