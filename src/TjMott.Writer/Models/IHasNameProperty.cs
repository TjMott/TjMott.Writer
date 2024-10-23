using System;
using System.Threading.Tasks;

namespace TjMott.Writer.Models
{
    public interface IHasNameProperty
    {
        string Name { get; set; }
        Task SaveAsync();
        Task LoadAsync();
    }
}
