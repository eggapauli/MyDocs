using MyDocs.Common.Model;
using MyDocs.WindowsStore.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                await MigrateFromBuiltInDbToLexDb();
            }
            MigrateFromVersion = CurrentVersion;
        }

        private static async Task MigrateFromBuiltInDbToLexDb()
        {
            var builtInDb = new ApplicationDataContainerDocumentStorage(new SettingsService());
            var documents = await builtInDb.GetAllDocumentsAsync();

            using (var db = new Lex.Db.DbInstance("mydocs.lex.db")) {
                db.Map<Document>()
                    .Automap(x => x.Id, true)
                    .WithIndex("Category", x => x.Category)
                    .WithIndex("Tags", x => x.Tags);
                db.Initialize();
                db.Purge();
                db.Save(documents);
            }
        }
    }
}
