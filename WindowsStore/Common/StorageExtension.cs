using MyDocs.Common.Contract.Storage;
using MyDocs.Common.Model.Logic;
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

            Func<Document, string[]> getPhotoRelativePaths = d => {
                if (d.SubDocuments.Count == 0) {
                    return null;
                }

                return d.SubDocuments
                    .Select(sd => String.Join("|", sd.Photos.Select(p => p.File.GetRelativePath())))
                    .ToArray();
            };

            Func<Uri, string, bool> isInFolder = (uri, folder) => uri.Segments[1] == folder;

            Func<Document, IFolder, string[]> getPhotoIsInFolder = (d, folder) => {
                if (d.SubDocuments.Count == 0) {
                    return null;
                }

                return doc.SubDocuments
                    .Select(sd => String.Join("|", sd.Photos.Select(p => p.File.IsInFolder(folder))))
                    .ToArray();
            };

            return new ApplicationDataCompositeValue {
                { "Id", doc.Id },
                { "Category", doc.Category },
                
                { "Tags", doc.Tags.Count > 0 ? doc.Tags.ToArray() : null },
                { "DateAdded", (DateTimeOffset)doc.DateAdded },
                { "Lifespan", doc.Lifespan },
                { "HasLimitedLifespan", doc.HasLimitedLifespan },
                
                { "PhotoTitles", doc.SubDocuments.Count > 0 ? doc.SubDocuments.Select(p => p.Title).ToArray() : null },

                { "PhotoPreviewNames", getPhotoRelativePaths(doc) },
                { "PhotoPreviewIsTemp", getPhotoIsInFolder(doc, tempFolder) },
                { "PhotoPreviewIsLocal", getPhotoIsInFolder(doc, localFolder) },

                { "PhotoFileNames", doc.SubDocuments.Count > 0 ? doc.SubDocuments.Select(p => p.File.GetRelativePath()).ToArray() : null },
                { "PhotoIsTemp", doc.SubDocuments.Count > 0 ? doc.SubDocuments.Select(p => p.File.IsInFolder(tempFolder)).ToArray() : null },
                { "PhotoIsLocal", doc.SubDocuments.Count > 0 ? doc.SubDocuments.Select(p => p.File.IsInFolder(localFolder)).ToArray() : null }
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
            var doc = new Document(
                (Guid)data["Id"],
                data.ContainsKey("Category") ? (string)data["Category"] : null,
                ((DateTimeOffset)data["DateAdded"]).Date,
                (TimeSpan)data["Lifespan"],
                (bool)data["HasLimitedLifespan"],
                data.ContainsKey("Tags") ? (string[])data["Tags"] : new string[0]
            );

            if (data.ContainsKey("PhotoFileNames")
                    && data.ContainsKey("PhotoIsLocal")
                    && data.ContainsKey("PhotoIsTemp")) {
                string[] photoTitles = data.ContainsKey("PhotoTitles") ? (string[])data["PhotoTitles"] : null;

                string[][] photoPreviewNames = data.ContainsKey("PhotoPreviewNames") ? ((string[])data["PhotoPreviewNames"]).Select(joined => joined.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)).ToArray() : null;
                bool[][] photoPreviewIsLocal = data.ContainsKey("PhotoPreviewIsLocal") ? ((string[])data["PhotoPreviewIsLocal"]).Select(joined => joined.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(flag => Boolean.Parse(flag)).ToArray()).ToArray() : null;
                bool[][] photoPreviewIsTemp = data.ContainsKey("PhotoPreviewIsTemp") ? ((string[])data["PhotoPreviewIsTemp"]).Select(joined => joined.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(flag => Boolean.Parse(flag)).ToArray()).ToArray() : null;

                string[] photoFileNames = (string[])data["PhotoFileNames"];
                bool[] photoIsLocal = (bool[])data["PhotoIsLocal"];
                bool[] photoIsTemp = (bool[])data["PhotoIsTemp"];

                if (photoFileNames != null && photoIsLocal != null && photoIsTemp != null) {
                    for (int i = 0; i < photoFileNames.Length; i++) {
                        try {
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

                            ICollection<IFile> previews = null;
                            if (photoPreviewNames != null && photoPreviewIsLocal != null && photoPreviewIsTemp != null) {
                                previews = new List<IFile>();
                                for (var j = 0; j < photoPreviewNames[i].Length; j++) {
                                    IStorageFolder previewFolder;
                                    if (photoPreviewIsLocal[i][j]) {
                                        previewFolder = ApplicationData.Current.LocalFolder;
                                    }
                                    else if (photoPreviewIsTemp[i][j]) {
                                        previewFolder = ApplicationData.Current.TemporaryFolder;
                                    }
                                    else {
                                        previewFolder = ApplicationData.Current.RoamingFolder;
                                    }
                                    string previewPath = Path.Combine(previewFolder.Path, photoPreviewNames[i][j]);
                                    StorageFile previewFile = await StorageFile.GetFileFromPathAsync(previewPath);
                                    previews.Add(new WindowsStoreFile(previewFile));
                                }
                            }

                            string filePath = Path.Combine(fileFolder.Path, photoFileNames[i]);
                            StorageFile file = await StorageFile.GetFileFromPathAsync(filePath);
                            var title = photoTitles != null ? photoTitles[i] : file.DateCreated.ToString("G");

                            doc.SubDocuments.Add(new SubDocument(title, new WindowsStoreFile(file), previews.Select(p => new Photo(p))));
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
