using System.Globalization;
using System.Windows;
using CefSharp;

namespace PSO2ModManager {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        public static string locale;
        public App() {
            // i18n settings
            locale = CultureInfo.CurrentCulture.ToString();

            var settings = new CefSettings();
            // Uncomment for version 49
            //settings.CefCommandLineArgs.Add("disable-gpu", "1");
            //Cef.Initialize(settings, shutdownOnProcessExit: true, performDependencyCheck: true);

            // Sync browser locale to system
            settings.LocalesDirPath = System.Windows.Forms.Application.StartupPath + "\\locales";
            settings.Locale = locale;
            Cef.Initialize (settings);
        }
    }
}
