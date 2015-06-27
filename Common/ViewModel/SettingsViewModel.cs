using MyDocs.Common.Contract.Service;
using ReactiveUI;

namespace MyDocs.Common.ViewModel
{
    public class SettingsViewModel : ReactiveObject
    {
        private ISettingsService settingsService;

        public bool SyncEnabled
        {
            get { return settingsService.IsSyncEnabled; }
            set
            {
                if (settingsService.IsSyncEnabled != value) {
                    settingsService.IsSyncEnabled = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public SettingsViewModel(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }
    }
}
