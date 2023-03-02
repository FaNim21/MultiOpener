using MultiOpener.ListView;

namespace MultiOpener.ViewModels.Settings
{
    public class SettingsOpenInstancesModelView : OpenTypeViewModelBase
    {
        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }


        public SettingsOpenInstancesModelView()
        {

        }

        public override void UpdatePanelField(OpenItem currentChosen)
        {
            base.UpdatePanelField(currentChosen);

            if (currentChosen is OpenInstance instance)
            {
                Quantity = instance.Quantity;
            }
        }
    }
}
