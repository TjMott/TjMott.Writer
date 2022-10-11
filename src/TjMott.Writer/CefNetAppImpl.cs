using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CefNet;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TjMott.Writer
{
    internal class CefNetAppImpl : CefNetApplication
    {
        internal static string CefDownloadUrl
        {
            get
            {
                if (PlatformInfo.IsWindows)
                {
                    return "https://cef-builds.spotifycdn.com/cef_binary_102.0.9+g1c5e658+chromium-102.0.5005.63_windows64_minimal.tar.bz2";
                }
                else if (PlatformInfo.IsLinux)
                {
                    return "https://cef-builds.spotifycdn.com/cef_binary_102.0.9+g1c5e658+chromium-102.0.5005.63_linux64_minimal.tar.bz2";
                }
                else
                {
                    throw new ApplicationException("Unsupported operating system.");
                }
            }
        }

        internal static string CefAssetPath
        {
            get
            {
                if (PlatformInfo.IsWindows)
                {
                    return Path.Join(Directory.GetCurrentDirectory(), "Assets", "cef-win64");
                }
                else if (PlatformInfo.IsLinux)
                {
                    return Path.Join(Directory.GetCurrentDirectory(), "Assets", "cef-linux64");
                }
                else
                {
                    throw new ApplicationException("Unsupported operating system.");
                }
            }
        }

        internal static string CefCookiePath
        {
            get
            {
                return Path.Combine(Directory.GetCurrentDirectory(), "cefinstalled");
            }
        }

        internal static bool InitSuccess { get; private set; } = false;
        internal static bool IsCefInstalled { get; private set; }
        internal static string InitErrorMessage { get; private set; }
        internal static CefNetAppImpl CefInstance { get; private set; }
        internal static bool Initialize()
        {
            try
            {
                // NOTE: As of version 102.0.22146.158, CefNet requires libx11-dev to be installed.
                
                if (!PlatformInfo.IsWindows && !PlatformInfo.IsLinux)
                {
                    InitErrorMessage = "Unsupported operating system detected.";
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (!Environment.Is64BitOperatingSystem)
                {
                    InitErrorMessage = "This application requires a 64-bit operating system.";
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (!Environment.Is64BitProcess)
                {
                    InitErrorMessage = "Process started as a 32-bit process on a 64-bit OS. 64-bit process required.";
                    InitSuccess = false;
                    return InitSuccess;
                }

                // Check for the file we place to indicate the user installed CEF.
                if (!File.Exists(CefCookiePath))
                {
                    IsCefInstalled = false;
                    InitErrorMessage = string.Format("CEF is not installed in your application directory.");
                    InitSuccess = false;
                    return InitSuccess;
                }
                else
                {
                    IsCefInstalled = true;
                }

                string cefPath = CefAssetPath;
                string libPath = Path.Combine(cefPath, "lib");
                string resourcesPath = Path.Join(cefPath, "resources");
                string localesPath = Path.Join(cefPath, "resources", "locales");

                // Used to test CEF initialization error display stuff.
                //InitErrorMessage = "Pretending that CEF failed to initialize, to test error messages and how we inform the user of this.";
                //InitSuccess = false;
                //return InitSuccess;

                // Sanity check on common CEF issues.
                if (!Directory.Exists(cefPath))
                {
                    InitErrorMessage = string.Format("Expected to find CEF at {0}, but that directory does not exist.", cefPath);
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (!Directory.Exists(libPath))
                {
                    InitErrorMessage = string.Format("Expected to find CEF libraries at {0}, but that directory does not exist.", libPath);
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (!Directory.Exists(resourcesPath))
                {
                    InitErrorMessage = string.Format("Expected to find CEF resources at {0}, but that directory does not exist.", resourcesPath);
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (!Directory.Exists(localesPath))
                {
                    InitErrorMessage = string.Format("Expected to find CEF locales at {0}, but that directory does not exist.", localesPath);
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (Directory.GetFiles(libPath).Length == 0)
                {
                    InitErrorMessage = string.Format("CEF library folder {0} is empty!", libPath);
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (Directory.GetFiles(resourcesPath).Length == 0)
                {
                    InitErrorMessage = string.Format("CEF resources folder {0} is empty!", resourcesPath);
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (!Directory.Exists(localesPath))
                {
                    InitErrorMessage = string.Format("CEF locales folder {0} is empty!", localesPath);
                    InitSuccess = false;
                    return InitSuccess;
                }
                if (!File.Exists(Path.Join(libPath, "icudtl.dat")))
                {
                    InitErrorMessage = string.Format("File {0} does not exist!", Path.Join(libPath, "icudtl.dat"));
                    InitSuccess = false;
                    return InitSuccess;
                }

                CefSettings settings = new CefSettings();
                settings.MultiThreadedMessageLoop = true;
                settings.WindowlessRenderingEnabled = true;
                settings.ResourcesDirPath = resourcesPath;
                settings.LocalesDirPath = localesPath;
                //settings.LogSeverity = CefLogSeverity.Debug;
                settings.LogSeverity = CefLogSeverity.Disable;

                CefInstance = new CefNetAppImpl();
                CefInstance.Initialize(libPath, settings);
                InitSuccess = true;
                return InitSuccess;
            }
            catch (Exception ex)
            {
                InitErrorMessage = ex.Message;
                InitSuccess = false;
                return InitSuccess;
            }
        }
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            base.OnBeforeCommandLineProcessing(processType, commandLine);

            /* Privacy stuff. CEF has zero reason to talk to anything on the network since we're only
             * loading local data in it, so try to turn off/disable anything that talks to the Internet.
             * Switches found at https://peter.sh/experiments/chromium-command-line-switches/
             */
            commandLine.AppendSwitchWithValue("check-for-update-interval", int.MaxValue.ToString());
            commandLine.AppendSwitchWithValue("connectivity-check-url", "");
            commandLine.AppendSwitchWithValue("crash-server-url", "");
            commandLine.AppendSwitchWithValue("cryptauth-http-host", "");
            commandLine.AppendSwitchWithValue("cryptauth-v2-devicesync-http-host", "");
            commandLine.AppendSwitchWithValue("cryptauth-v2-enrollment-http-host", "");
            commandLine.AppendSwitch("disable-background-networking");
            commandLine.AppendSwitch("disable-component-cloud-policy");
            commandLine.AppendSwitch("disable-component-update");
            commandLine.AppendSwitch("disable-crash-reporter");
            commandLine.AppendSwitch("disable-device-discovery-notifications");
            commandLine.AppendSwitch("disable-gaia-services");
            commandLine.AppendSwitch("disable-notifications");
            commandLine.AppendSwitch("disable-speech-api");
            commandLine.AppendSwitch("disable-speech-synthesis-api");
            commandLine.AppendSwitch("disable-sync");
            commandLine.AppendSwitchWithValue("external-metrics-collection-interval", int.MaxValue.ToString());
            commandLine.AppendSwitchWithValue("gcm-checkin-url", "");
            commandLine.AppendSwitchWithValue("gcm-mcs-endpoint", "");
            commandLine.AppendSwitchWithValue("gcm-registration-url", "");
            commandLine.AppendSwitchWithValue("google-apis-url", "");
            commandLine.AppendSwitchWithValue("google-base-url", "");
            commandLine.AppendSwitchWithValue("metrics-upload-interval", int.MaxValue.ToString());
            commandLine.AppendSwitch("no-pings");
            commandLine.AppendSwitchWithValue("override-metrics-upload-url", "");
            commandLine.AppendSwitchWithValue("realtime-reporting-url", "");

            // Allows changing URL during print.
            commandLine.AppendSwitch("allow-file-access-from-files");

            //if (PlatformInfo.IsLinux)
            {
                // Needed to prevent crash on Linux.
                commandLine.AppendSwitch("no-zygote");
                commandLine.AppendSwitch("no-sandbox");
            }
        }

        internal static void RestartAndInstallCef()
        {
            // Do we have write access to the program directory? If so,
            // we do not need to elevate/sudo the install process.
            bool elevate = false;
            try
            {
                string fileCheck = Path.Combine(Directory.GetCurrentDirectory(), "accessCheck.temp");
                using (var vs = File.Create(fileCheck)) { } ; // Throws UnauthorizedAccessException if directory is not writable.
                File.Delete(fileCheck);
            }
            catch (UnauthorizedAccessException)
            {
                // Exception thrown while creating file. Need to elevate.
                elevate = true;
            }
           
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.UseShellExecute = true;
            if (PlatformInfo.IsWindows)
            {
                psi.FileName = Path.Combine(Directory.GetCurrentDirectory(), "TjMott.Writer.exe");
                if (elevate)
                    psi.Verb = "runas";
                psi.ArgumentList.Add("-installcef");
            }
            else if (PlatformInfo.IsLinux)
            {
                if (elevate)
                {
                    psi.FileName = "bash";
                    psi.FileName = Path.Combine(Directory.GetCurrentDirectory(), "elevate-install-cef.sh"); // pkexec sounds like a better option but I couldn't get it to work.
                }
                else
                {
                    psi.FileName = Path.Combine(Directory.GetCurrentDirectory(), "TjMott.Writer");
                }
                psi.ArgumentList.Add("-installcef");
            }
            else
            {
                throw new ApplicationException("Unsupported operating system.");
            }
            
            Process.Start(psi);
            (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).Shutdown();
        }
    }
}
