using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reactive;

namespace TjMott.Writer.ViewModels
{
    internal class PasswordInputViewModel : ViewModelBase
    {
        private string _password = "";
        public string Password { get => _password; set => this.RaiseAndSetIfChanged(ref _password, value); }
        
        private string _confirmPassword = "";
        public string ConfirmPassword { get => _confirmPassword; set => this.RaiseAndSetIfChanged(ref _confirmPassword, value); }

        public ReactiveCommand<Window, Unit> OkCommand { get; }
        public ReactiveCommand<Window, Unit> CancelCommand { get; }

        public PasswordInputViewModel()
        {
            OkCommand = ReactiveCommand.Create<Window>(okButtonClicked, 
                this.WhenAnyValue(i => i.Password, i => ConfirmPassword, (pass, confirmPass) =>
                {
                    return pass == confirmPass && !string.IsNullOrWhiteSpace(pass);
                }));
            CancelCommand = ReactiveCommand.Create<Window>(cancelButtonClicked);
        }

        private void okButtonClicked(Window dialogOwner)
        {
            if (Password == ConfirmPassword && !string.IsNullOrWhiteSpace(Password))
                dialogOwner.Close(true);
        }

        private void cancelButtonClicked(Window dialogOwner)
        {
            dialogOwner.Close(false);
        }
    }
}
