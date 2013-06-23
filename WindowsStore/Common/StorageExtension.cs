using MyDocs.Common.Model;
using MyDocs.WindowsStore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyDocs.WindowsStore.Common
{
	public static class StorageExtension
	{
		public static ApplicationDataCompositeValue ConvertToStoredDocument(this Document doc)
		{
			return new ApplicationDataCompositeValue {
				{ "Id", doc.Id },
				{ "Category", doc.Category },
				{ "PhotoFileNames", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.File.GetRelativePath()).ToArray() : null },
				{ "PhotoIsTemp", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.File.IsInFolder(new WindowsStoreFolder(ApplicationData.Current.TemporaryFolder))).ToArray() : null },
				{ "PhotoIsLocal", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.File.IsInFolder(new WindowsStoreFolder(ApplicationData.Current.LocalFolder))).ToArray() : null },
				{ "Tags", doc.Tags.Count > 0 ? doc.Tags.ToArray() : null },
				{ "DateAdded", (DateTimeOffset)doc.DateAdded },
				{ "Lifespan", doc.Lifespan },
				{ "HasLimitedLifespan", doc.HasLimitedLifespan }
			};
		}

		public static void ConvertToRestorableDocument(this Document doc, IDictionary<string, object> container)
		{
			var data = doc.ConvertToStoredDocument();
			foreach (var entry in data) {
				container.Add(entry);
			}
		}

		public static async Task<Document> ConvertToDocumentAsync(this IDictionary<string, object> data)
		{
			Document doc = new Document(
				(Guid)data["Id"],
				data.ContainsKey("Category") ? (string)data["Category"] : null,
				((DateTimeOffset)data["DateAdded"]).Date,
				(TimeSpan)data["Lifespan"],
				(bool)data["HasLimitedLifespan"],
				data.ContainsKey("Tags") ? (string[])data["Tags"] : null
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

			if (data.ContainsKey("PhotoFileNames")
					&& data.ContainsKey("PhotoIsLocal")
					&& data.ContainsKey("PhotoIsTemp")) {
				string[] photoFileNames = (string[])data["PhotoFileNames"];
				bool[] photoIsLocal = (bool[])data["PhotoIsLocal"];
				bool[] photoIsTemp = (bool[])data["PhotoIsTemp"];

				if (photoFileNames != null && photoIsLocal != null && photoIsTemp != null) {
					for (int i = 0; i < photoFileNames.Length; i++) {
						try {
							StorageFile file;
							if (photoIsLocal[i]) {
								string path = Path.Combine(ApplicationData.Current.LocalFolder.Path, photoFileNames[i]);
								file = await StorageFile.GetFileFromPathAsync(path);
							}
							else if (photoIsTemp[i]) {
								string path = Path.Combine(ApplicationData.Current.TemporaryFolder.Path, photoFileNames[i]);
								file = await StorageFile.GetFileFromPathAsync(path);
							}
							else {
								string path = Path.Combine(ApplicationData.Current.RoamingFolder.Path, photoFileNames[i]);
								file = await StorageFile.GetFileFromPathAsync(path);
							}
							doc.Photos.Add(new Photo(new WindowsStoreFile(file)));
						}
						catch (FileNotFoundException) {
							// should not occur, unless the user manually deleted the file
						}
					}
				}
			}

			return doc;
		}
	}
}
