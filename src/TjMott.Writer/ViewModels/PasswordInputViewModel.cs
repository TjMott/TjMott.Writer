using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reactive;

namespace TjMott.Writer.ViewModels
{
    internal class PasswordInputViewModel : ViewModelBase
    {
        private string _password = "";
        public string Password 
        { 
            get => _password; 
            set => this.RaiseAndSetIfChanged(ref _password, value); 
        }

        private bool _dialogAccepted = false;
        public bool DialogAccepted
        {
            get => _dialogAccepted;
            private set => this.RaiseAndSetIfChanged(ref _dialogAccepted, value);
        }

        private string _prompt = "Password Required";
        public string Prompt
        {
            get => _prompt;
            set => this.RaiseAndSetIfChanged(ref _prompt, value);
        }
        public ReactiveCommand<Window, Unit> OkCommand { get; }
        public ReactiveCommand<Window, Unit> CancelCommand { get; }

        public PasswordInputViewModel()
        {
            OkCommand = ReactiveCommand.Create<Window>(okButtonClicked, 
                this.WhenAny(i => i.Password, (pass) => !string.IsNullOrWhiteSpace(pass.Value)));
            CancelCommand = ReactiveCommand.Create<Window>(cancelButtonClicked);
        }

        private void okButtonClicked(Window dialogOwner)
        {
            if (!string.IsNullOrWhiteSpace(Password))
            {
                DialogAccepted = true;
                dialogOwner.Close();
            }
        }

        private void cancelButtonClicked(Window dialogOwner)
        {
            dialogOwner.Close();
        }
    }
}
