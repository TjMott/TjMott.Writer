using ReactiveUI;

namespace TjMott.Writer.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        protected void OnPropertyChanged(string propertyName)
        {
            this.RaisePropertyChanged(propertyName);
        }

        public bool IsDebugMode
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
