using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.Common.Contract.Service
{
    public interface IFileConverter
    {
        Task<StorageFile> ToFile(Uri File);
    }
}
