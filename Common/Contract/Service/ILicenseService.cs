using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface ILicenseService
    {
        Task Unlock(string featureName);
    }
}
