using System;
using System.ComponentModel;
using System.Linq;
using TjMott.Writer.Model;

namespace TjMott.Writer.ViewModel
{
    public class SortBySortIndexBindingList<T> : BindingList<T> where T : ISortable
    {
        private bool _ignoreSortIndexChanged = false;

        public void AddToEnd(T item)
        {
            item.PropertyChanged += Item_PropertyChanged;
            if (Count == 0)
            {
                item.SortIndex = 0;
                base.Add(item);
                return;
            }
            long newIndex = this.Max(i => i.SortIndex) + 1;
            item.SortIndex = newIndex;
            Add(item);
        }

        public new void Add(T item)
        {
            item.PropertyChanged += Item_PropertyChanged;
            if (Count == 0)
            {
                base.Add(item);
                return;
            }

            for (int i = 0; i < Count; i++)
            {
                if (this[i].SortIndex >= item.SortIndex)
                {
                    this.InsertItem(i, item);
                    FixSortIndices(i);
                    return;
                }
            }

            if (item.SortIndex < Count)
            {
                item.SortIndex = Count;
            }
            base.Add(item);
        }

        public new void Remove(T item)
        {
            item.PropertyChanged -= Item_PropertyChanged;
            base.Remove(item);
            FixSortIndices(item.SortIndex);
        }

        public bool CanMoveItemUp(T item)
        {
            if (!Contains(item))
                return false;

            return IndexOf(item) > 0;
        }

        public void MoveItemUp(T item)
        {
            if (!CanMoveItemUp(item))
                return;

            int index = IndexOf(item);
            T other = this[index - 1];
            _ignoreSortIndexChanged = true;
            item.SortIndex--;
            item.Save();
            _ignoreSortIndexChanged = false;
            other.SortIndex++;
            other.Save();
        }

        public bool CanMoveItemDown(T item)
        {
            if (!Contains(item))
                return false;

            return IndexOf(item) < Count - 1;
        }

        public void MoveItemDown(T item)
        {
            if (!CanMoveItemDown(item))
                return;

            int index = IndexOf(item);
            T other = this[index + 1];
            _ignoreSortIndexChanged = true;
            item.SortIndex++;
            item.Save();
            _ignoreSortIndexChanged = false;
            other.SortIndex--;
            other.Save();
        }

        public void FixSortIndices(long startIndex)
        {
            // Fix sort indices to be consecutive.
            _ignoreSortIndexChanged = true;
            for (int i = (int)startIndex; i < Count; i++)
            {
                this[i].SortIndex = i;
                this[i].Save();
            }
            _ignoreSortIndexChanged = false;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SortIndex" && !_ignoreSortIndexChanged)
            {
                T item = (T)sender;
                Remove(item);
                Add(item);
            }
        }
    }
}
