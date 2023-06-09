namespace MultiOpener.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        private int _timeLookingForInstancesData;
        public int TimeLookingForInstancesData
        {
            get { return _timeLookingForInstancesData; }
            set
            {
                _timeLookingForInstancesData = value;
                OnPropertyChanged(nameof(TimeLookingForInstancesData));
            }
        }

        public OptionsViewModel()
        {

        }
    }
}
