using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace TjMott.Writer.Models
{
    public interface ISortable : INotifyPropertyChanged
    {
        long SortIndex { get; set; }
        Task SaveAsync();
    }
}
