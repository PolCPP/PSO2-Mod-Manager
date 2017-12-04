using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ServiceStack.Text;
using SourceChord.Lighty;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WPFLocalizeExtension.Engine;

namespace PSO2ModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged {
        #region INotifyPropertyChanged Implementation
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises a new <see cref="E:INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void RaisePropertyChanged (string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        public ModManager Mods { get; set; }
        public ModPresenter SelectedPresenter { get; set; } = new ModPresenter();
        private DispatcherTimer updatesTimer;
        private InlineDialog d;

        public MainWindow() {
            // Initilalize Inline Dialog
            d = InlineDialog.Instance();

            LocalizeDictionary.Instance.Culture = new CultureInfo (App.locale);

            // Initialize Mod Manager
            if (ModManager.CheckForSettings()) {
                Mods = new ModManager();
            } else {
                MessageBox.Show ("This is a very early version of the mod tool," +
                    "so it looks like crap, and while it shouldn't, it could" +
                    "make your pso2 explode in a thousand darkers.\n Also remember" +
                    "that Sega doesn't approve of mods, so don't come crying to" +
                    "Rupi if they ban you.  You've been warmed \n -Rupi ",
                    "Important!");
                Mods = new ModManager (GetPSO2Dir());
            }

            InitializeComponent();

            Mods.OnSelectionChanged += ModChanged;

            ValidateUrlInput();

            // For some reason RegisterJsObject doesn't work so we're stream a json object 
            // to the page title, once we have a new download action.
            Browser.TitleChanged += Browser_TitleChanged;
            Browser.BrowserSettings.AcceptLanguageList = App.locale;
            Browser.Address = String.Format ("http://pso2mod.com/?app={0}&lang={1}", "true", App.locale.Substring (0, 2));
        }

        /// <summary>
        /// Callback to update the Download progressbar.
        /// </summary>
        public void DownloadProgress(object sender, System.Net.DownloadProgressChangedEventArgs e) {
            d.UpdateProgressDialogValue (Convert.ToDouble(e.ProgressPercentage * 0.01));
        }

        /// <summary>
        /// Starts a mod download.
        /// </summary>
        private async Task DownloadMod (string url) {
            if (Mods.Downloading) {
                await d.PromptAsync (Helpers._ ("Error.Title"), Helpers._ ("Error.MultipleDownloadProcess"));
                return;
            }

            // Add Event handler
            Mods.OnDownloadStart += DownloadStart;
            Mods.OnDownloadPercentPercentChanged += DownloadProgress;
            Mods.OnDownloadComplete += DownloadComplete;
            // Download progress
            await Mods.DownloadMod (url);
            // Remove Event Handler
            Mods.OnDownloadPercentPercentChanged -= DownloadProgress;
            Mods.OnDownloadComplete -= DownloadComplete;
            Mods.OnDownloadStart -= DownloadStart;
        }

        private async void DownloadStart (object sender, EventArgs e) {
            await d.OpenProgressDialog (Helpers._ ("Dialog.WaitTitle"), Helpers._ ("Dialog.Downloading"));
        }

        /// <summary>
        /// Updates a mod
        /// </summary>
        private async void UpdateSelectedMod() {
            await Mods.UpdateMod();
        }

        /// <summary>
        /// Callback when the download progress
        /// </summary>
        private async void DownloadComplete(object sender, EventArgs e)
        {
            dynamic u = (ModManager.OnDownloadCompleteArgs)e;
            bool Success = false;
            string ErrorMessage = null;

            if (u != null) {
                Success = u.Success;
                ErrorMessage = u.ErrorMessage;
            } else {
                return;
            }

            if (!Success) {
                await d.PromptAsync (Helpers._("Error.DownloadingTitle"), ErrorMessage);
            } else {
                InstalledModsTab.Focus();
            }
            DownloadUrlTextbox.Text = "";
            ValidateUrlInput();
            await d.CloseProgressDialog();
        }

        /// <summary>
        /// Kinda validates the url.
        /// </summary>
        private void ValidateUrlInput() {
            if (DownloadUrlTextbox.Text == "" || !DownloadUrlTextbox.Text.ToLower().StartsWith ("http://")) {
                DownloadModBtn.IsEnabled = false;
            } else {
                DownloadModBtn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Shows a Folderbrowser dialog and gets PSO2 Directory, if it fails it closes the application.
        /// </summary>
        public string GetPSO2Dir() {
            string folderPath = "";
            while (!Helpers.ValidatePSO2Dir (folderPath)) {
                dynamic fbd = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = Helpers._("Select the pso2 data/win32 directory"),
                    RootFolder = Environment.SpecialFolder.MyComputer,
                    SelectedPath = Helpers.DetectPSODir() + "\\data\\win32",
                    ShowNewFolderButton = false
                };

                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                    folderPath = fbd.SelectedPath;
                } else {
                    Environment.Exit (1);
                }
                if (!Helpers.ValidatePSO2Dir (folderPath)) {
                    MessageBox.Show ("This doesn't looks like the pso2 data/win32 folder. Try again", "Error");
                }
            }
            return folderPath;
        }

        /// <summary>
        /// Updates the mod presenter when the selected mod changes.
        /// </summary>
        public void ModChanged(object sender, EventArgs e)
        {
            SelectedPresenter.Setup (Mods.SelectedMod, Mods.IsInstalled (Mods.SelectedMod));
        }

        /// <summary>
        /// Event hook that enables the updates button after
        /// certain time passes, and stops the time.
        /// </summary>
        private void ReenableUpdates (object sender, EventArgs e) {
            updatesTimer.Stop();
            CheckForUpdatesBtn.IsEnabled = true;
        }

        /// <summary>
        /// Asks the mod manager to check for updates
        /// </summary>
        private async void CheckForUpdates() {
            await d.OpenProgressDialog (Helpers._ ("Dialog.WaitTitle"), Helpers._ ("Dialog.CheckingUpdate"));

            Mods.OnError += UpdateCheckError;
            bool success = await Mods.CheckForUpdates();
            Mods.OnError -= UpdateCheckError;

            if (success) {
                CheckForUpdatesBtn.IsEnabled = false;
                updatesTimer = new System.Windows.Threading.DispatcherTimer();
                updatesTimer.Tick += new EventHandler (ReenableUpdates);
                updatesTimer.Interval = new TimeSpan (0, 5, 0);
                updatesTimer.Start();
                await d.CloseProgressDialog();
            }
        }

        private async void UpdateCheckError(object sender, EventArgs e)
        {
            dynamic u = (ModManager.OnErrorArgs)e;
            String Message = "";
            if (u != null)
            {
                Message = u.Message;
            }
            await d.PromptAsync (Helpers._ ("Error.CheckingUpdate"), Message);
        }

        private void FindAndInstallMod() {
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog
            {
                // Set filter options and filter index.
                Filter = Helpers._ ("FileDialog.Filter") + " (.zip)|*.zip",
                FilterIndex = 1,
                Multiselect = true
            };

            // Process input if the user clicked OK.
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                Mods.AddLocalMod (fd.FileName);
                InstalledModsTab.Focus();
            }
        }

        #region Input Events

        private void DownloadUrlTextbox_TextChanged (object sender, TextChangedEventArgs e) {
            ValidateUrlInput();
        }

        private async void DownloadModBtn_Click (object sender, RoutedEventArgs e) {
            await DownloadMod (DownloadUrlTextbox.Text);
        }

        private void CheckForUpdatesBtn_Click (object sender, RoutedEventArgs e) {
            CheckForUpdates();
        }

        private void InstallUninstallBtn_Click (object sender, RoutedEventArgs e) {
            Mods.ToggleMod();
        }

        private void AvailableModsList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                Mods.SelectedMod = (Mod) e.AddedItems[0];
            }
        }

        private void InstalledModsList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count > 0) {
                Mods.SelectedMod = (Mod) e.AddedItems[0];
            }
        }

        private void DeleteBtn_Click (object sender, RoutedEventArgs e) {
            Mods.Delete();
        }

        private void UpdateBtn_Click (object sender, RoutedEventArgs e) {
            UpdateSelectedMod();
        }

        private void ViewSiteBtn_Click (object sender, RoutedEventArgs e) {
            //string url = "http://pso2mod.com/?lang={1}&p={0}";
            string url = "http://pso2mod.com/?p={0}";
            System.Diagnostics.Process.Start (String.Format(url, SelectedPresenter.Id));
        }

        private async void Browser_TitleChanged (object sender, DependencyPropertyChangedEventArgs e) {
            DownloadAction duh = new DownloadAction
            {
                Url = "http://google.com"
            };
            try {
                JsonSerializer.SerializeToString<DownloadAction> (duh);
                DownloadAction da = JsonSerializer.DeserializeFromString<DownloadAction> (e.NewValue.ToString());
                if (da.Url != null) {
                    await DownloadMod (da.Url);
                }
            } catch {
                // Not valid json: Note it would be better to just run a json validation method,
                // but there doesn't seem to be anything on servicestack.text for that
            }
        }

        private void InstallLocalModBtn_Click (object sender, RoutedEventArgs e) {
            FindAndInstallMod();
        }
        /// <summary>
        /// Toggle Setting Flyout
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingBtn_Click (object sender, RoutedEventArgs e) {
            settingsFlyout.IsOpen = !settingsFlyout.IsOpen;
        }

        #endregion Input Events

        /// <summary>
        /// Show Thumnail
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModImage_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e) {
            // show FrameworkElement.
            var image = new Image
            {
                Source = ModImage.Source
            };
            LightBox.Show (this, image);
        }
        /// <summary>
        /// Show ProgressRing and URL in Build-in WebBrowser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Browser_LoadingStateChanged (object sender, CefSharp.LoadingStateChangedEventArgs e) {
            if (e.IsLoading) {
                this.Dispatcher.Invoke (() => {
                    ProgressRing.Visibility = Visibility.Visible;
                    StatusBarText.Content = "Now Loading...";
                });
            } else {
                this.Dispatcher.Invoke (() => {
                    ProgressRing.Visibility = Visibility.Hidden;
                    StatusBarText.Content = Browser.Address;
                });
            }
        }

        #region DialogTask
        private sealed class InlineDialog {
            // Singleton
            private static readonly InlineDialog _singleInstance = new InlineDialog();
            // Progress Dialog Object
            public ProgressDialogController ProgressDlgCtl;
            // Parent Window
            private MainWindow w;
            // Check Multiple Dialog
            private bool multiple;
            /// <summary>
            /// Constructor
            /// </summary>
            private InlineDialog() {
                w = (MainWindow) App.Current.MainWindow;
            }
            /// <summary>
            /// Get Instance
            /// </summary>
            /// <returns></returns>
            public static InlineDialog Instance() {
                return _singleInstance;
            }
            /// <summary>
            /// Show Message Box
            /// </summary>
            /// <param name="title"></param>
            /// <param name="message"></param>
            /// <param name="style"></param>
            /// <param name="settings"></param>
            /// <returns></returns>
            public async Task<MessageDialogResult> PromptAsync (string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null) {
                return await w.ShowMessageAsync (title, message, style, settings);
            }
            public async Task OpenProgressDialog (string title, string message, bool isCancellable = false) {
                Console.WriteLine ("Open Progress Dialog.");
                if (multiple) return;
                ProgressDlgCtl = await w.ShowProgressAsync (title, message, isCancellable) as ProgressDialogController;
                ProgressDlgCtl.SetIndeterminate();
                multiple = true;
            }
            public async Task CloseProgressDialog (bool continueOnCaptureContext = false) {
                await ProgressDlgCtl.CloseAsync().ConfigureAwait (continueOnCaptureContext);
                multiple = false;
                Console.WriteLine ("Close Progress Dialog.");
            }
            public void UpdateProgressDialogValue (double value) {
                ProgressDlgCtl.SetProgress (value);
            }
        }
        #endregion

        private void MainTab_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            if (MainTab.SelectedIndex == 0) {
                StatusBarText.Content = "PSO2 Mod Manager";
            }
        }
    }
}
