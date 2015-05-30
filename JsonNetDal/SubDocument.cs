using MyDocs.Common;
using MyDocs.Common.Contract.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Logic = MyDocs.Common.Model.Logic;

namespace JsonNetDal
{
    public class SubDocument
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string File { get; set; }

        public List<string> Photos { get; set; }

        public SubDocument() { }

        public SubDocument(string title, string file, IEnumerable<string> photos)
        {
            Title = title;
            File = file;
            Photos = photos.ToList();
        }

        public static SubDocument FromLogic(Logic.SubDocument subDocument)
        {
            return new SubDocument(subDocument.Title, subDocument.File.GetUri().AbsoluteUri, subDocument.Photos.Select(p => p.File.GetUri().AbsoluteUri));
        }

        public async Task<Logic.SubDocument> ToLogic()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(File));
            var photoTasks =
                Photos
                .Select(p => new Uri(p))
                .Select(StorageFile.GetFileFromApplicationUriAsync)
                .Select(x => x.AsTask());
            var photos = await Task.WhenAll(photoTasks);
            return new Logic.SubDocument(file, photos.Select(p => new Logic.Photo(p)));
        }
    }
}
