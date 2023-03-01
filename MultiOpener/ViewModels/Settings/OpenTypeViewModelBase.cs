using MultiOpener.ListView;

namespace MultiOpener.ViewModels.Settings
{
    public abstract class OpenTypeViewModelBase : BaseViewModel
    {
        private string? _applicationPathField;
        public string? ApplicationPathField
        {
            get { return _applicationPathField; }
            set
            {
                _applicationPathField = value;
                OnPropertyChanged(nameof(ApplicationPathField));
            }
        }

        private string? _delayAfterTimeField;
        public string? DelayAfterTimeField
        {
            get { return _delayAfterTimeField; }
            set
            {
                _delayAfterTimeField = value;
                OnPropertyChanged(nameof(DelayAfterTimeField));
            }
        }

        private string? _delayBeforeTimeField;
        public string? DelayBeforeTimeField
        {
            get { return _delayBeforeTimeField; }
            set
            {
                _delayBeforeTimeField = value;
                OnPropertyChanged(nameof(DelayBeforeTimeField));
            }
        }

        public virtual void UpdatePanelField(OpenItem currentChosen)
        {
            ApplicationPathField = currentChosen.PathExe;
            DelayBeforeTimeField = currentChosen.DelayBefore.ToString();
            DelayAfterTimeField = currentChosen.DelayAfter.ToString();
        }
    }
}
