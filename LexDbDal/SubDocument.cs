using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logic = MyDocs.Common.Model.Logic;

namespace LexDbDal
{
    public class SubDocument
    {
        public string Title { get; set; }

        public Uri File { get; set; }

        public ICollection<Uri> Photos { get; set; }

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
    }
}
