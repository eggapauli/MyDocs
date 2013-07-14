using MyDocs.Common.Contract.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
	public interface IFilePickerService
	{
		Task<IEnumerable<IFile>> PickMultipleFilesAsync();
	}
}
