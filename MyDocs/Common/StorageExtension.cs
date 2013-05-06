using MyDocs.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace MyDocs.Common
{
	public enum FileSize
	{
		SMALL,
		BIG
	}

	public static class StorageExtension
	{
		private struct StoragePhoto
		{
			public string FileName;
			public bool IsLocal;

			public StoragePhoto(StorageFile file)
			{
				FileName = Path.GetFileName(file.Path);
				IsLocal = file.IsInFolder(ApplicationData.Current.LocalFolder);
			}
		}

		public static ApplicationDataCompositeValue ConvertToStoredDocument(this Document doc)
		{
			return new ApplicationDataCompositeValue {
				new KeyValuePair<string, object>("Id", doc.Id),
				new KeyValuePair<string, object>("Category", doc.Category),
				new KeyValuePair<string, object>("PhotoFileNames", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.File.GetRelativePath()).ToArray() : null),
				new KeyValuePair<string, object>("PhotoIsLocal", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.File.IsInFolder(ApplicationData.Current.LocalFolder)).ToArray() : null),
				new KeyValuePair<string, object>("Tags", doc.Tags.ToArray()),
				new KeyValuePair<string, object>("DateAdded", (DateTimeOffset)doc.DateAdded),
				new KeyValuePair<string, object>("Lifespan", doc.Lifespan),
				new KeyValuePair<string, object>("HasLimitedLifespan", doc.HasLimitedLifespan)
			};
		}

		public static async Task<Document> ConvertToDocumentAsync(this ApplicationDataCompositeValue data)
		{
			Document doc = new Document(
				(Guid)data["Id"],
				(string)data["Category"],
				((DateTimeOffset)data["DateAdded"]).Date,
				(TimeSpan)data["Lifespan"],
				(bool)data["HasLimitedLifespan"],
				(string[])data["Tags"]
			);

			// not working properly because if a FileNotFoundException occurs, no photo is loaded into the collection
			//List<Task<StorageFile>> tasks = new List<Task<StorageFile>>();
			//foreach (string path in (string[])data["Photos"]) {
			//	tasks.Add(StorageFile.GetFileFromPathAsync(path).AsTask());
			//}
			//try {
			//	doc.Photos = new ObservableCollection<StorageFile>(await Task.WhenAll(tasks));
			//}
			//catch (FileNotFoundException e) {
			//	// TODO ?
			//}

			string[] photoFileNames = (string[])data["PhotoFileNames"];
			bool[] photoIsLocal = (bool[])data["PhotoIsLocal"];
			if (photoFileNames != null && photoIsLocal != null) {
				for (int i = 0; i < photoFileNames.Length; i++) {
					try {
						StorageFile file;
						if (photoIsLocal[i]) {
							string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, photoFileNames[i]);
							file = await StorageFile.GetFileFromPathAsync(path);
						}
						else {
							string path = Path.Combine(ApplicationData.Current.RoamingFolder.Path, photoFileNames[i]);
							file = await StorageFile.GetFileFromPathAsync(path);
						}
						doc.Photos.Add(new Photo(file));
					}
					catch (FileNotFoundException) {
						// should not occur, unless the user manually deleted the file
					}
				}
			}

			return doc;
		}

		public static async Task<BitmapImage> GetResizedBitmapImageAsync(this StorageFile photo, FileSize fileSize = FileSize.SMALL)
		{
			int size;
			switch (fileSize) {
				case FileSize.BIG: size = (int)Math.Max(Window.Current.Bounds.Width, Window.Current.Bounds.Height); break;
				case FileSize.SMALL:
				default: size = 250; break;
			}
			BitmapImage bmp = new BitmapImage();
			using (StorageItemThumbnail thumbnail = await photo.GetThumbnailAsync(ThumbnailMode.SingleItem, (uint)size)) {
				await bmp.SetSourceAsync(thumbnail);
				return bmp;
			}
		}

		public static bool IsInFolder(this IStorageItem file, StorageFolder folder)
		{
			return Path.GetDirectoryName(file.Path).StartsWith(folder.Path);
		}

		public static string GetRelativePath(this IStorageItem file)
		{
			string folderPath = String.Empty;
			if (file.IsInFolder(ApplicationData.Current.LocalFolder)) {
				folderPath = ApplicationData.Current.LocalFolder.Path;
			}
			else if (file.IsInFolder(ApplicationData.Current.TemporaryFolder)) {
				folderPath = ApplicationData.Current.TemporaryFolder.Path;
			}
			else if (file.IsInFolder(ApplicationData.Current.RoamingFolder)) {
				folderPath = ApplicationData.Current.RoamingFolder.Path;
			}
			return file.Path.Substring(folderPath.Length + 1);
		}
	}
}
