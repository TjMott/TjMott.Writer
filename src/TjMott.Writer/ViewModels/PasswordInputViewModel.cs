using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace TjMott.Writer.ViewModels
{
    internal class PasswordInputViewModel : ViewModelBase
    {
        private string _password = "";
        public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }

        public ReactiveCommand<Window, Unit> OkCommand { get; }

        public PasswordInputViewModel()
        {
            OkCommand = ReactiveCommand.Create<Window>(okButtonClicked);
        }

        private void okButtonClicked(Window dialogOwner)
        {

        }
    }
}
