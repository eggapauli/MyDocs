using MyDocs.Common.Contract.Storage;
using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface ISettingsService
    {
        IFolder PhotoFolder { get; }
        IFolder TempFolder { get; }
        ApplicationDataContainer SettingsContainer { get; }
        bool IsSyncEnabled { get; set; }
    }
}
