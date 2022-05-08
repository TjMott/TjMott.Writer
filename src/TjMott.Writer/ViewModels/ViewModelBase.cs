using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

namespace TjMott.Writer.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected void OnPropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }
        public static Window MainWindow { get; set; }
    }
}
