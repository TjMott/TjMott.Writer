using Avalonia.Controls;
using ReactiveUI;

namespace TjMott.Writer.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected void OnPropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }
    }
}
