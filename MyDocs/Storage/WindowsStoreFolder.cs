using MyDocs.Common.Contract.Storage;
using Windows.Storage;

namespace MyDocs.WindowsStoreFrontend.Storage
{
	public class WindowsStoreFolder : IFolder
	{
		public StorageFolder Folder { get; private set; }

		public string Path
		{
			get { return Folder.Path; }
		}

		public WindowsStoreFolder(StorageFolder folder)
		{
			Folder = folder;
		}
	}
}
