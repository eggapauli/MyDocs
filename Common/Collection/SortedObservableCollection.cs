using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyDocs.Common.Collection
{
    public class SortedObservableCollection<T> : ObservableCollection<T>
    {
        private readonly IComparer<T> comparer;

        public SortedObservableCollection(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        public SortedObservableCollection(IEnumerable<T> collection, IComparer<T> comparer)
            : base(collection.OrderBy(item => item, comparer))
        {
            this.comparer = comparer;
        }

        protected override void InsertItem(int index, T item)
        {
            bool added = false;
            for (int idx = 0; idx < Count; idx++) {
                if (comparer.Compare(item, Items[idx]) <= 0) {
                    base.InsertItem(idx, item);
                    added = true;
                    break;
                }
            }

            if (!added) {
                base.InsertItem(index, item);
            }
        }
    }
}
