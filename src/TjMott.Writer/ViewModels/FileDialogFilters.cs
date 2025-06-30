using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace TjMott.Writer.ViewModels
{
    public static class FileDialogFilters
    {
#pragma warning disable CS0618 // Type or member is obsolete
        public static FileDialogFilter DatabaseFilter
        {
            get
            {
                return new FileDialogFilter() { Name = "Writer Database (*.wdb)", Extensions = new List<string>() { "wdb" } };
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
