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

        private IFolder localFolder = new WindowsStoreFolder(ApplicationData.Current.LocalFolder);
        private IFolder roamingFolder = new WindowsStoreFolder(ApplicationData.Current.RoamingFolder);
        private IFolder tempFolder = new WindowsStoreFolder(ApplicationData.Current.TemporaryFolder);

        private ApplicationDataContainer roamingSettings = ApplicationData.Current.RoamingSettings;
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public IFolder PhotoFolder
        {
            get { return IsSyncEnabled ? roamingFolder : localFolder; }
        }

        public IFolder TempFolder
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
