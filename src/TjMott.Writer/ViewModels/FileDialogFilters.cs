using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TjMott.Writer.ViewModels
{
    public static class FileDialogFilters
    {
        public static FileDialogFilter DatabaseFilter
        {
            get
            {
                return new FileDialogFilter() { Name = "Writer Database (*.wdb)", Extensions = new List<string>() { "wdb" } };
            }
        }
    }
}
