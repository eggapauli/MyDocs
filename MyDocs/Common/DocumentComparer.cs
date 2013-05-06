using MyDocs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common
{
	public class DocumentComparer : IComparer<Document>
	{
		public int Compare(Document x, Document y)
		{
			if (x is AdDocument || y is AdDocument) {
				return 0;
			}

			int result = y.DateAdded.CompareTo(x.DateAdded);
			if (result != 0) {
				return result;
			}

			return x.Id.CompareTo(y.Id);
		}
	}
}
