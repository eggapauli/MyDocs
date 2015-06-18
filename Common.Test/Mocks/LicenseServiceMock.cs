using MyDocs.Common.Contract.Service;
using System;
using System.Threading.Tasks;

namespace Common.Test.Mocks
{
    class LicenseServiceMock : ILicenseService
    {
        public Func<string, Task> UnlockFunc = async delegate { await Task.Yield(); };
        public Task Unlock(string featureName)
        {
            return UnlockFunc(featureName);
        }
    }
}
