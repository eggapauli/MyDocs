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
        public async Task Unlock(string featureName)
        {
            if (Global.LicenseInformation.ProductLicenses[featureName].IsActive) {
                return;
            }

            PurchaseResults result;
            var errorMessage = string.Format("Feature {0} couldn't be unlocked.", featureName);
            try {
                result = await Global.RequestProductPurchaseAsync(featureName);
            }
            catch (Exception e) {
                throw new LicenseStatusException(errorMessage, LicenseStatus.Error, e);
            }

            if (result.Status != ProductPurchaseStatus.AlreadyPurchased &&
                result.Status != ProductPurchaseStatus.Succeeded) {
                    throw new LicenseStatusException(errorMessage, LicenseStatus.Locked);
            }
        }
    }
}
