using System;
using ReactiveUI;

namespace TjMott.Writer.ViewModels.CefInstall
{
    public class CefInstallViewModel : ViewModelBase
    {
        public static CefInstallViewModel Instance { get; private set; }

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel { get => _currentViewModel; set => this.RaiseAndSetIfChanged(ref _currentViewModel, value); }

        public CefInstallViewModel()
        {
            Instance = this;
            CurrentViewModel = new CefBeginViewModel();
        }
    }
}
