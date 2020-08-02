using System;
using System.ComponentModel;

namespace TjMott.Writer.Model
{
    public interface ISortable : INotifyPropertyChanged
    {
        long SortIndex { get; set; }
        void Save();
    }
}
