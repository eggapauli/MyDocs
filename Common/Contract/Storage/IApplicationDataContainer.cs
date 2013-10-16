using System.Collections.Generic;

namespace MyDocs.Common.Contract.Storage
{
    public interface IApplicationDataContainer
    {
        IApplicationDataContainer CreateContainer(string name);
        IDictionary<string, object> Values { get; }
    }
}
