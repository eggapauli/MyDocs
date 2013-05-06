using MyDocs.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common
{
	public class SettingsService : ISettingsService
	{
		private enum FolderType { Local, Roaming }
		private static readonly string syncEnabledKey = "isSyncEnabled";

		public StorageFolder PhotoFolder
		{
			get
			{
				if (IsSyncEnabled) {
					return ApplicationData.Current.RoamingFolder;
				}
				return ApplicationData.Current.LocalFolder;
			}
		}

		public ApplicationDataContainer SettingsContainer
		{
			get
			{
				if (IsSyncEnabled) {
					return ApplicationData.Current.RoamingSettings;
				}
				return ApplicationData.Current.LocalSettings;
			}
		}

		public bool IsSyncEnabled
		{
			get { return GetSetting(syncEnabledKey, false); }
			set { SetSetting(syncEnabledKey, value); }
		}

		private T GetSetting<T>(string key, T defaultValue)
		{
			object value;
			if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out value)) {
				return (T)value;
			}
			else if (ApplicationData.Current.RoamingSettings.Values.TryGetValue(key, out value)) {
				return (T)value;
			}
			return defaultValue;
		}

		private void SetSetting(string key, object value)
		{
			ApplicationData.Current.RoamingSettings.Values[key] = value;
		}
	}
}
