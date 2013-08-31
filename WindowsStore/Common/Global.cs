using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace MyDocs.WindowsStore.Common
{
    public static class Global
    {
        public static LicenseInformation LicenseInformation
        {
#if DEBUG
            get { return CurrentAppSimulator.LicenseInformation; }
#else
            get { return CurrentApp.LicenseInformation; }
#endif

        }

        public static async Task<PurchaseResults> RequestProductPurchaseAsync(string featureName)
        {
#if DEBUG
            return await CurrentAppSimulator.RequestProductPurchaseAsync(featureName);
#else
            return await CurrentApp.RequestProductPurchaseAsync(featureName);
#endif
        }
    }
}
