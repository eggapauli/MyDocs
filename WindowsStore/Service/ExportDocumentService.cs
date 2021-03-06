﻿using MyDocs.Common;
using MyDocs.Common.Contract.Service;
using Serializable = MyDocs.Common.Model.Serializable;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MyDocs.WindowsStore.Service
{
    public class ExportDocumentService : IExportDocumentService
    {
        private readonly IDocumentDb documentDb;
        private readonly ITranslatorService translatorService;
        private readonly IFileSavePickerService fileSavePickerService;

        public ExportDocumentService(IDocumentDb documentDb, ITranslatorService translatorService, IFileSavePickerService fileSavePickerService)
        {
            this.documentDb = documentDb;
            this.translatorService = translatorService;
            this.fileSavePickerService = fileSavePickerService;
        }

        public async Task ExportDocuments()
        {
            var fileTypes = new Dictionary<string, IList<string>> {
                { translatorService.Translate("archive"), new List<string> { ".zip" } }
            };
            var savedFiles = new HashSet<string>();
            var zipFile = await fileSavePickerService.PickSaveFileAsync("MyDocs.zip", fileTypes);
            if (zipFile == null) {
                return;
            }

            var documents = await documentDb.GetAllDocumentsAsync();

            using (var zipFileStream = await zipFile.OpenStreamForWriteAsync())
            using (var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create)) {
                var metaInfoEntry = archive.CreateEntry("Documents.xml");
                using (var metaInfoStream = metaInfoEntry.Open()) {
                    var serializer = new DataContractSerializer(typeof(IEnumerable<Serializable.Document>), "Documents", "http://mydocs.eggapauli");
                    var serializedDocuments = documents.Select(Serializable.Document.FromModel);
                    serializer.WriteObject(metaInfoStream, serializedDocuments);
                }

                foreach (var document in documents) {
                    foreach (var photo in document.SubDocuments) {
                        var path = Path.Combine(document.GetHumanReadableDescription(), photo.File.Name);
                        if (savedFiles.Add(path)) {
                            var entry = archive.CreateEntry(path);
                            using (var photoReader = (await photo.File.OpenReadAsync()).AsStream())
                            using (var entryStream = entry.Open()) {
                                await photoReader.CopyToAsync(entryStream);
                            }
                        }
                    }
                }
            }
        }
    }
}
