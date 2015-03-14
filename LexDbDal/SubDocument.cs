using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }
}
