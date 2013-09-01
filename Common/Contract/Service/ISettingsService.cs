using MyDocs.Common.Contract.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface ISettingsService
    {
        IFolder PhotoFolder { get; }
        IApplicationDataContainer SettingsContainer { get; }
        bool IsSyncEnabled { get; set; }
    }
}
