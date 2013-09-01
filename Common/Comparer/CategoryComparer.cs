using MyDocs.Common.Model;
using System.Collections.Generic;

namespace MyDocs.Common.Comparer
{
    public class CategoryComparer : IComparer<Category>
    {
        public int Compare(Category x, Category y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
