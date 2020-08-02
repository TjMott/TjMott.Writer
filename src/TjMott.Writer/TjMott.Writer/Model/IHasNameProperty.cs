using System;

namespace TjMott.Writer.Model
{
    public interface IHasNameProperty
    {
        string Name { get; set; }
        void Save();
        void Load();
    }
}
