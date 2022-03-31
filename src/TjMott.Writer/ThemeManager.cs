using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TjMott.Writer.Properties;

namespace TjMott.Writer
{
    public static class ThemeManager
    {
        public static event EventHandler ThemeChanged;
        private static Theme _selectedTheme;
        public static Theme SelectedTheme
        {
            get { return _selectedTheme; }
        }


        public enum Theme
        {
            Light,
            Dark
        }

        public static void InitTheme()
        {
            if (string.IsNullOrWhiteSpace(AppSettings.Default.theme))
            {
                SetTheme(Theme.Light);
            }
            else if (AppSettings.Default.theme == "Light")
            {
                SetTheme(Theme.Light);
            }
            else if (AppSettings.Default.theme == "Dark")
            {
                SetTheme(Theme.Dark);
            }
        }

        public static void SetTheme(Theme t)
        {
            _selectedTheme = t;
            switch (t)
            {
                case Theme.Dark:
                    AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, AdonisUI.ResourceLocator.DarkColorScheme);
                    AppSettings.Default.theme = "Dark";
                    AppSettings.Default.Save();
                    break;
                case Theme.Light:
                    AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, AdonisUI.ResourceLocator.LightColorScheme);
                    AppSettings.Default.theme = "Light";
                    AppSettings.Default.Save();
                    break;
            }
            if (ThemeChanged != null)
                ThemeChanged(null, new EventArgs());
        }
    }
}
