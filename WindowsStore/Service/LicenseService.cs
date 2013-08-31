using MyDocs.Common.Contract.Service;
using MyDocs.WindowsStore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace MyDocs.WindowsStore.Service
{
    public class LicenseService : ILicenseService
    {
        public async Task<bool> TryGetLicenseAsync(string featureName)
        {
            if (Global.LicenseInformation.ProductLicenses[featureName].IsActive) {
                return true;
            }

            try {
                var result = await Global.RequestProductPurchaseAsync(featureName);
                
                return (result.Status == ProductPurchaseStatus.AlreadyPurchased)
                    || (result.Status == ProductPurchaseStatus.Succeeded);
                
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
