using MyDocs.Common.Contract.Service;
using MyDocs.Common.Contract.Storage;
using MyDocs.WindowsStore.Storage;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
	public class SettingsService : ISettingsService
	{
		private enum FolderType { Local, Roaming }
		private static readonly string syncEnabledKey = "isSyncEnabled";

		public IFolder PhotoFolder
		{
			get
			{
				if (IsSyncEnabled) {
					return new WindowsStoreFolder(ApplicationData.Current.RoamingFolder);
				}
				return new WindowsStoreFolder(ApplicationData.Current.LocalFolder);
			}
		}

		public IApplicationDataContainer SettingsContainer
		{
			get
			{
				if (IsSyncEnabled) {
					return new WindowsStoreApplicationDataContainer(ApplicationData.Current.RoamingSettings);
				}
				return new WindowsStoreApplicationDataContainer(ApplicationData.Current.LocalSettings);
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
