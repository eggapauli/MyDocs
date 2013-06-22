using MyDocs.Common.Contract.Storage;
using System.Collections.Generic;
using Windows.Storage;

namespace MyDocs.WindowsStoreFrontend.Storage
{
	public class WindowsStoreApplicationDataContainer : IApplicationDataContainer
	{
		public ApplicationDataContainer Container { get; private set; }

		public WindowsStoreApplicationDataContainer(ApplicationDataContainer container)
		{
			Container = container;
		}

		public IDictionary<string, object> Values { get { return Container.Values; } }

		public IApplicationDataContainer CreateContainer(string name)
		{
			var container = Container.CreateContainer(name, ApplicationDataCreateDisposition.Always);
			return new WindowsStoreApplicationDataContainer(container);
		}
	}
}
