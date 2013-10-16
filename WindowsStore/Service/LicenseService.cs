using MyDocs.Common.Contract.Service;
using MyDocs.Common.Model;
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
        public async Task<LicenseStatus> TryGetLicenseAsync(string featureName)
        {
            if (Global.LicenseInformation.ProductLicenses[featureName].IsActive) {
                return LicenseStatus.Unlocked;
            }

            try {
                var result = await Global.RequestProductPurchaseAsync(featureName);
                
                return (result.Status == ProductPurchaseStatus.AlreadyPurchased)
                    || (result.Status == ProductPurchaseStatus.Succeeded) ? LicenseStatus.Unlocked : LicenseStatus.Locked;
                
            }
            catch (Exception) {
                return LicenseStatus.Error;
            }
        }
    }
}
