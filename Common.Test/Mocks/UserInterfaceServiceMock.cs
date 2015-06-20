using MyDocs.Common.Contract.Service;
using System.Threading.Tasks;

namespace Common.Test.Mocks
{
    class UserInterfaceServiceMock : IUserInterfaceService
    {
        public async Task ShowErrorAsync(string msgKey)
        {
            await Task.Yield();
        }

        public async Task ShowNotificationAsync(string msgKey)
        {
            await Task.Yield();
        }
    }
}
