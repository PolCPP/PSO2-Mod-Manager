using System.Windows;
using CefSharp;

namespace PSO2ModManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App() {
            var settings = new CefSettings();
            // Uncomment for version 49
            //settings.CefCommandLineArgs.Add("disable-gpu", "1");
            //Cef.Initialize(settings, shutdownOnProcessExit: true, performDependencyCheck: true);
            Cef.Initialize();

        }

    }
}