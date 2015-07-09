using MyDocs.Common;
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
        public string Title { get; set; }
        
        public Uri File { get; set; }
        
        public List<Uri> Photos { get; set; }

        public SubDocument() { }

        public SubDocument(string title, Uri file, IEnumerable<Uri> photos)
        {
            Title = title;
            File = file;
            Photos = photos.ToList();
        }

        public static SubDocument FromLogic(Logic.SubDocument subDocument)
        {
            return new SubDocument(subDocument.Title, subDocument.File.GetUri(), subDocument.Photos.Select(p => p.File.GetUri()));
        }

        public async Task<Logic.SubDocument> ToLogic()
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(File);
            var photoTasks =
                Photos
                .Select(StorageFile.GetFileFromApplicationUriAsync)
                .Select(x => x.AsTask());
            var photos = await Task.WhenAll(photoTasks);
            return new Logic.SubDocument(file, photos.Select(p => new Logic.Photo(p)));
        }
    }
}
