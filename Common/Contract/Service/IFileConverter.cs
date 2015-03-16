using MyDocs.Common.Contract.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common.Contract.Service
{
    public interface IFileConverter
    {
        Task<IFile> ToFile(Uri File);
    }
}
