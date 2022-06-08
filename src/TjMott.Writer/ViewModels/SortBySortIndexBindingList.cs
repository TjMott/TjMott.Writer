using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TjMott.Writer.Models;

namespace TjMott.Writer.ViewModels
{
    public class SortBySortIndexBindingList<T> : ObservableCollection<T> where T : ISortable
    {
        private bool _ignoreSortIndexChanged = false;

        public void AddToEnd(T item)
        {
            if (Count == 0)
            {
                item.SortIndex = 0;
                base.Add(item);
                item.PropertyChanged += Item_PropertyChanged;
                return;
            }
            long newIndex = this.Max(i => i.SortIndex) + 1;
            item.SortIndex = newIndex;
            base.Add(item);
            item.PropertyChanged += Item_PropertyChanged;
        }

        public new void Add(T item)
        {
            if (Count == 0)
            {
                base.Add(item);
                item.PropertyChanged += Item_PropertyChanged;
                return;
            }

            for (int i = 0; i < Count; i++)
            {
                if (this[i].SortIndex >= item.SortIndex)
                {
                    this.InsertItem(i, item);
                    FixSortIndices(i);
                    item.PropertyChanged += Item_PropertyChanged;
                    return;
                }
            }

            if (item.SortIndex < Count)
            {
                item.SortIndex = Count;
            }
            base.Add(item);
            item.PropertyChanged += Item_PropertyChanged;
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
            base.Remove(other);
            base.Insert(index, other);
            item.SortIndex--;
            item.SaveAsync();
            other.SortIndex++;
            other.SaveAsync();
            _ignoreSortIndexChanged = false;
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
            base.Remove(other);
            base.Insert(index, other);
            item.SortIndex++;
            item.SaveAsync();
            other.SortIndex--;
            other.SaveAsync();
            _ignoreSortIndexChanged = false;
        }

        public void FixSortIndices(long startIndex)
        {
            // Fix sort indices to be consecutive.
            _ignoreSortIndexChanged = true;
            for (int i = (int)startIndex; i < Count; i++)
            {
                this[i].SortIndex = i;
                this[i].SaveAsync();
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
