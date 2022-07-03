using CefNet;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TjMott.Writer
{
    internal class CefNetAppImpl : CefNetApplication
    {
        internal static bool InitSuccess { get; private set; } = false;
        internal static string InitErrorMessage { get; private set; }
        internal static CefNetAppImpl CefInstance { get; private set; }
        internal static bool Initialize()
        {
            try
            {
                // NOTE: As of version 102.0.22146.158, CefNet requires libx11-dev to be installed.
                string cefPath = Directory.GetCurrentDirectory();
                if (PlatformInfo.IsWindows)
                {
                    cefPath = Path.Join(cefPath, "Assets", "cef-win64");
                }
                else if (PlatformInfo.IsLinux)
                {
                    cefPath = Path.Join(cefPath, "Assets", "cef-linux64");
                }
                else
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
    }
}
