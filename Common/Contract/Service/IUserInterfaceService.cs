using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IUserInterfaceService
    {
        Task ShowErrorAsync(string msgKey);

        Task ShowNotificationAsync(string msgKey);
    }
}
