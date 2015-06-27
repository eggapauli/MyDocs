using MyDocs.WindowsStore.Service;
using Splat;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace MyDocs.WindowsStore
{
    public static class MigrationHelper
    {
        private static Version CurrentVersion
        {
            get
            {
                var v = Package.Current.Id.Version;
                return new Version(v.Major, v.Minor, v.Build, v.Revision);
            }
        }

        const string latestMigrationVersionKey = "migratedTo";
        private static Version MigrateFromVersion
        {
            get
            {
                var version = (string)ApplicationData.Current.LocalSettings.Values[latestMigrationVersionKey];
                return version != null ? Version.Parse(version) : null;
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values[latestMigrationVersionKey] = value.ToString();
            }
        }

        public static async Task Migrate()
        {
            if (MigrateFromVersion == null) {
                await MigrateFromBuiltInDbToJsonDb();
            }
            MigrateFromVersion = CurrentVersion;
        }

        private static async Task MigrateFromBuiltInDbToJsonDb()
        {
            var builtInDb = Locator.Current.GetService<ApplicationDataContainerDocumentStorageService>();
            var documents = await builtInDb.GetAllDocumentsAsync();

            await Locator.Current.GetService<JsonNetDal.JsonDocumentDb>().Setup(documents);
        }
    }
}
