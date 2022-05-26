using CefNet;
using System;
using System.IO;

namespace TjMott.Writer
{
    internal class CefNetAppImpl : CefNetApplication
    {
        internal static CefNetAppImpl CefInstance { get; private set; }
        internal static void Initialize()
        {
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
                throw new ApplicationException("Unsupported operating system detected.");
            }
            if (!Environment.Is64BitOperatingSystem)
            {
                throw new ApplicationException("This application requires a 64-bit operating system.");
            }
            if (!Environment.Is64BitProcess)
            {
                throw new ApplicationException("Process started as a 32-bit process on a 64-bit OS. 64-bit process required.");
            }
            CefSettings settings = new CefSettings();
            settings.MultiThreadedMessageLoop = true;
            settings.WindowlessRenderingEnabled = true;
            settings.ResourcesDirPath = Path.Join(cefPath, "resources");
            settings.LocalesDirPath = Path.Join(cefPath, "resources", "locales");
            //settings.LogSeverity = CefLogSeverity.Debug;
            settings.LogSeverity = CefLogSeverity.Disable;

            string libPath = Path.Combine(cefPath, "lib");
            CefInstance = new CefNetAppImpl();
            CefInstance.Initialize(libPath, settings);
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

            //if (PlatformInfo.IsLinux)
            {
                // Needed to prevent crash on Linux.
                commandLine.AppendSwitch("no-zygote");
                commandLine.AppendSwitch("no-sandbox");
            }
        }
    }
}
