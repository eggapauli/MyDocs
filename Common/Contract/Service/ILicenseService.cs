using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface ILicenseService
    {
        Task<bool> TryGetLicenseAsync(string featureName);
    }
}
