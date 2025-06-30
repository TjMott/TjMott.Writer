using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CefNet;
using System;

namespace TjMott.Writer.ViewModels.CefInstall
{
    public class CefBeginViewModel : ViewModelBase
    {
        public void Continue()
        {
            // First do some checks.
            if (!PlatformInfo.IsWindows && !PlatformInfo.IsLinux)
            {
                CefInstallViewModel.Instance.CurrentViewModel = new UnsupportedOsViewModel("Your operating system is not supported. It must be 64-bit Windows or Linux.");
                return;
            }
            if (!Environment.Is64BitOperatingSystem)
            {
                CefInstallViewModel.Instance.CurrentViewModel = new UnsupportedOsViewModel("You are running a 32-bit operating system, but TJ Mott's Writer requires a 64-bit OS.");
                return;
            }
            else if (!Environment.Is64BitProcess)
            {
                CefInstallViewModel.Instance.CurrentViewModel = new UnsupportedOsViewModel("64-bit OS detected, but this process is running as 32-bit. Make sure you have the 64-bit .NET Runtime installed.");
                return;
            }

            // Checks passed.
            CefInstallViewModel.Instance.CurrentViewModel = new DownloadViewModel();
        }

        public void Cancel()
        {
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }
    }
}
