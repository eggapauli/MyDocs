using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logic = MyDocs.Common.Model.Logic;

namespace LexDbDal
{
    public class SubDocument
    {
        public Guid Id { get; set; }

        public Guid DocumentId { get; set; }

        public string Title { get; set; }

        public string File { get; set; }

        public List<string> Photos { get; set; }

        public SubDocument() { }

        public SubDocument(Guid documentId, string title, string file, IEnumerable<string> photos)
        {
            DocumentId = documentId;
            Title = title;
            File = file;
            Photos = photos.ToList();
        }

        public static SubDocument FromLogic(Logic.SubDocument subDocument, Guid documentId)
        {
            return new SubDocument(documentId, subDocument.Title, subDocument.File.GetUri().AbsoluteUri, subDocument.Photos.Select(p => p.File.GetUri().AbsoluteUri));
        }

        public static IEnumerable<SubDocument> FromLogic(Logic.Document document)
        {
            return document.SubDocuments.Select(sd => SubDocument.FromLogic(sd, document.Id));
        }

        public async Task<Tuple<Guid, Logic.SubDocument>> ToLogic(IFileConverter fileConverter)
        {
            var file = await fileConverter.ToFile(new Uri(File));
            var photos = await Task.WhenAll(Photos.Select(p => new Uri(p)).Select(fileConverter.ToFile));
            return Tuple.Create(DocumentId, new Logic.SubDocument(file, photos.Select(p => new Logic.Photo(p))));
        }
    }
}
