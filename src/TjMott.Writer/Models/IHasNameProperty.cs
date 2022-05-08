using System;

namespace TjMott.Writer.Models
{
    public interface IHasNameProperty
    {
        string Name { get; set; }
        void Save();
        void Load();
    }
}
