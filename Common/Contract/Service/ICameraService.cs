using MyDocs.Common.Contract.Storage;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
	public interface ICameraService
	{
		Task<IFile> CaptureFileAsync();
	}
}
