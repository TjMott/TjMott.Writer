using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;

namespace TjMott.Writer.ViewModels.CefInstall
{
    public class UnsupportedOsViewModel : ViewModelBase
    {
        private string _message = "";
        public string Message { get => _message; private set => this.RaiseAndSetIfChanged(ref _message, value); }

        public UnsupportedOsViewModel(string message)
        {
            Message = message;
        }

        public void Close()
        {
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }
    }
}
