using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TjMott.Writer.ViewModels
{
    public interface IGetWordCount
    {
        Task<long> GetWordCountAsync();
    }
}
