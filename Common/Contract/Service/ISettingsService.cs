using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface ISettingsService
    {
        IStorageFolder PhotoFolder { get; }
        IStorageFolder TempFolder { get; }
        ApplicationDataContainer SettingsContainer { get; }
        bool IsSyncEnabled { get; set; }
    }
}
