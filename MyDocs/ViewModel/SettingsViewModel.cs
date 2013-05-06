using MyDocs.Contract.Service;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDocs.ViewModel
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
