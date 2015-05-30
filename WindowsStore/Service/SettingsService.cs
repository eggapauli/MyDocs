using MyDocs.Common.Contract.Service;
using Windows.Storage;

namespace MyDocs.WindowsStore.Service
{
    public class SettingsService : ISettingsService
    {
        private enum FolderType { Local, Roaming }
        private static readonly string syncEnabledKey = "isSyncEnabled";

        private IStorageFolder localFolder = ApplicationData.Current.LocalFolder;
        private IStorageFolder roamingFolder = ApplicationData.Current.RoamingFolder;
        private IStorageFolder tempFolder = ApplicationData.Current.TemporaryFolder;

        private ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public IStorageFolder PhotoFolder
        {
            get { return IsSyncEnabled ? roamingFolder : localFolder; }
        }

        public IStorageFolder TempFolder
        {
            get { return tempFolder; }
        }

        public ApplicationDataContainer SettingsContainer
        {
            get { return IsSyncEnabled ? roamingSettings : localSettings; }
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
            ApplicationData.Current.LocalSettings.Values[key] = value;
            if (IsSyncEnabled) {
                ApplicationData.Current.RoamingSettings.Values[key] = value;
            }
        }
    }
}
