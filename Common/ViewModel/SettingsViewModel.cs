using GalaSoft.MvvmLight;
using MyDocs.Common.Contract.Service;

namespace MyDocs.Common.ViewModel
{
	public class SettingsViewModel : ViewModelBase
	{
		private ISettingsService settingsService;

		public bool SyncEnabled
		{
			get { return settingsService.IsSyncEnabled; }
			set
			{
				if (settingsService.IsSyncEnabled != value) {
					settingsService.IsSyncEnabled = value;
					RaisePropertyChanged(() => SyncEnabled);
				}
			}
		}

		public SettingsViewModel(ISettingsService settingsService)
		{
			this.settingsService = settingsService;
		}
	}
}
