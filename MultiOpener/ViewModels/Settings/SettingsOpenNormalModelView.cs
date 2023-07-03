using MultiOpener.Entities.Open;
using System;

namespace MultiOpener.ViewModels.Settings
{
    public class SettingsOpenNormalModelView : OpenTypeViewModelBase
    {
        public override Type ItemType { get; set; } = typeof(OpenItem);

        public SettingsOpenNormalModelView() : base()
        {
            
        }

    }
}
