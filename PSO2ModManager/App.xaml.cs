using System.Globalization;
using System.Windows;
using CefSharp;
using System;

namespace PSO2ModManager {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        internal static string locale;
        public App() {
            // i18n settings
            locale = CultureInfo.CurrentCulture.ToString();
            var settings = new CefSettings {
                // Uncomment for version 49
                //CefCommandLineArgs.Add("disable-gpu", "1"),
                // Sync browser locale to system
                LocalesDirPath = System.Windows.Forms.Application.StartupPath + "\\locales",
                Locale = locale
		    };
            System.IO.Directory.CreateDirectory(settings.LocalesDirPath);
            Cef.Initialize(settings);
            //Cef.Initialize(settings, shutdownOnProcessExit: true, performDependencyCheck: true);
		}
	}
}
