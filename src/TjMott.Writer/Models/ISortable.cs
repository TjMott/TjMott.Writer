using System;
using System.ComponentModel;

namespace TjMott.Writer.Models
{
    public interface ISortable : INotifyPropertyChanged
    {
        long SortIndex { get; set; }
        void Save();
    }
}
