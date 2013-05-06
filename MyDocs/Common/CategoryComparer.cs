using MyDocs.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDocs.Common
{
	public class CategoryComparer : IComparer<Category>
	{
		public int Compare(Category x, Category y)
		{
			return x.Name.CompareTo(y.Name);
		}
	}
}
