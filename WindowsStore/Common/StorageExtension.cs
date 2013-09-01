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
			var tempFolder = new WindowsStoreFolder(ApplicationData.Current.TemporaryFolder);
			var localFolder = new WindowsStoreFolder(ApplicationData.Current.LocalFolder);

			return new ApplicationDataCompositeValue {
				{ "Id", doc.Id },
				{ "Category", doc.Category },
				
				{ "Tags", doc.Tags.Count > 0 ? doc.Tags.ToArray() : null },
				{ "DateAdded", (DateTimeOffset)doc.DateAdded },
				{ "Lifespan", doc.Lifespan },
				{ "HasLimitedLifespan", doc.HasLimitedLifespan },
				
				{ "PhotoTitles", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.Title).ToArray() : null },

				{ "PhotoPreviewNames", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.Preview.GetRelativePath()).ToArray() : null },
				{ "PhotoPreviewIsTemp", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.Preview.IsInFolder(tempFolder)).ToArray() : null },
				{ "PhotoPreviewIsLocal", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.Preview.IsInFolder(localFolder)).ToArray() : null },

				{ "PhotoFileNames", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.File.GetRelativePath()).ToArray() : null },
				{ "PhotoIsTemp", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.File.IsInFolder(tempFolder)).ToArray() : null },
				{ "PhotoIsLocal", doc.Photos.Count > 0 ? doc.Photos.Select(p => p.File.IsInFolder(localFolder)).ToArray() : null }
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

			if (data.ContainsKey("PhotoFileNames")
					&& data.ContainsKey("PhotoIsLocal")
					&& data.ContainsKey("PhotoIsTemp")) {
				string[] photoTitles = data.ContainsKey("PhotoTitles") ? (string[])data["PhotoTitles"] : null;

				string[] photoPreviewNames = data.ContainsKey("PhotoPreviewNames") ? (string[])data["PhotoPreviewNames"] : null;
				bool[] photoPreviewIsLocal = data.ContainsKey("PhotoPreviewIsLocal") ? (bool[])data["PhotoPreviewIsLocal"] : null;
				bool[] photoPreviewIsTemp = data.ContainsKey("PhotoPreviewIsTemp") ? (bool[])data["PhotoPreviewIsTemp"] : null;

				string[] photoFileNames = (string[])data["PhotoFileNames"];
				bool[] photoIsLocal = (bool[])data["PhotoIsLocal"];
				bool[] photoIsTemp = (bool[])data["PhotoIsTemp"];

				if (photoFileNames != null && photoIsLocal != null && photoIsTemp != null) {
					for (int i = 0; i < photoFileNames.Length; i++) {
						try {
							var title = photoTitles != null ? photoTitles[i] : null;

							IStorageFolder fileFolder;
							if (photoIsLocal[i]) {
								fileFolder = ApplicationData.Current.LocalFolder;
							}
							else if (photoIsTemp[i]) {
								fileFolder = ApplicationData.Current.TemporaryFolder;
							}
							else {
								fileFolder = ApplicationData.Current.RoamingFolder;
							}

							WindowsStoreFile preview = null;
							if (photoPreviewNames != null && photoPreviewIsLocal != null && photoPreviewIsTemp != null && photoPreviewNames[i] != photoFileNames[i]) {
								IStorageFolder previewFolder;
								if (photoPreviewIsLocal[i]) {
									previewFolder = ApplicationData.Current.LocalFolder;
								}
								else if (photoPreviewIsTemp[i]) {
									previewFolder = ApplicationData.Current.TemporaryFolder;
								}
								else {
									previewFolder = ApplicationData.Current.RoamingFolder;
								}
								string previewPath = Path.Combine(previewFolder.Path, photoPreviewNames[i]);
								StorageFile previewFile = await StorageFile.GetFileFromPathAsync(previewPath);
								preview = new WindowsStoreFile(previewFile);
							}

							string filePath = Path.Combine(fileFolder.Path, photoFileNames[i]);
							StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
							doc.Photos.Add(new Photo(title, new WindowsStoreFile(file), preview));
						}
						catch (FileNotFoundException) {
							// should not occur, unless the user manually deleted the file
							// TODO collect exceptions and inform user afterwards
						}
					}
				}
			}
			return doc;
		}

		public static bool IsImage(this IStorageItem file)
		{
			return new[] { ".bmp", ".gif", ".jpeg", ".jpg", ".png" }.Contains(Path.GetExtension(file.Path), StringComparer.OrdinalIgnoreCase);
		}
	}
}
