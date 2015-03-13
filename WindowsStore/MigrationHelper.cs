using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore
{
    public static class MigrationHelper
    {
        public static void Migrate()
        {
            var currentVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            var migrateFromVersion = ApplicationData.Current.LocalSettings.Values["migratedTo"];
            if (migrateFromVersion == null) {
                MigrateFromBuiltInDbToSqlite();
            }
        }

        private static void MigrateFromBuiltInDbToSqlite()
        {
            // TODO implement migration
            //throw new NotImplementedException();
        }
    }
}
