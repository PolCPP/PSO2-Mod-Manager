using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ServiceStack.Text;
using SourceChord.Lighty;

namespace PSO2ModManager {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        public ModManager Mods { get; set; }
        public ModPresenter SelectedPresenter { get; set; } = new ModPresenter ();
        private DispatcherTimer updatesTimer;

        private ProgressDialogController DownloadProgressDialog;

        public MainWindow () {
            if (ModManager.CheckForSettings ()) {
                Mods = new ModManager ();
            } else {
                MessageBox.Show ("This is a very early version of the mod tool," +
                    "so it looks like crap, and while it shouldn't, it could" +
                    "make your pso2 explode in a thousand darkers.\n Also remember" +
                    "that Sega doesn't approve of mods, so don't come crying to" +
                    "Rupi if they ban you.  You've been warmed \n -Rupi ",
                    "Important!");
                Mods = new ModManager (GetPSO2Dir ());
            }

            InitializeComponent ();

            Mods.OnDownloadPercentPercentChanged += DownloadProgress;
            Mods.OnDownloadComplete += DownloadComplete;
            Mods.OnSelectionChanged += ModChanged;

            ValidateUrlInput ();

            // For some reason RegisterJsObject doesn't work so we're stream a json object 
            // to the page title, once we have a new download action.
            Browser.TitleChanged += Browser_TitleChanged;
        }

        /// <summary>
        /// Callback to update the Download progressbar.
        /// </summary>
        public void DownloadProgress (int value) {
            DownloadProgressDialog.SetProgress (System.Convert.ToDouble (value * 0.01));
        }

        /// <summary>
        /// Starts a mod download.
        /// </summary>
        private async void DownloadMod (string url) {
            DownloadProgressDialog = await this.ShowProgressAsync ("Please wait...", "Now downloading mod.");
            DownloadProgressDialog.SetIndeterminate ();
            //DownloadProgressDialog.SetCancelable(true);

            if (DownloadProgressDialog.IsCanceled)
                return; //canceled progressdialog auto closes.

            if (!Mods.Downloading) {
                Mods.DownloadMod (url);
            } else {
                ShowMessageBox ("Currently downloading another mod. Please wait until it's installed", "Error");
            }
        }

        /// <summary>
        /// Updates a mod
        /// </summary>
        private void UpdateSelectedMod () {
            //DownloadBar.Visibility = Visibility.Visible;
            Mods.UpdateMod ();
        }

        /// <summary>
        /// Callback when the download progress
        /// </summary>
        private void DownloadComplete (bool success, string errorMessage = null) {
            DownloadProgressDialog.SetIndeterminate ();
            if (!success) {
                ShowMessageBox (errorMessage, "Error downloading mod");
            } else {
                DownloadUrlTextbox.Text = "";
                ValidateUrlInput ();
                InstalledModsTab.Focus ();

            }
            DownloadProgressDialog.CloseAsync ();
        }

        /// <summary>
        /// Kinda validates the url.
        /// </summary>
        private void ValidateUrlInput () {
            if (DownloadUrlTextbox.Text == "" || !DownloadUrlTextbox.Text.ToLower ().StartsWith ("http://")) {
                DownloadModBtn.IsEnabled = false;
            } else {
                DownloadModBtn.IsEnabled = true;
            }
        }

        /// <summary>
        /// Shows a Folderbrowser dialog and gets PSO2 Directory, if it fails it closes the application.
        /// </summary>
        public string GetPSO2Dir () {
            string folderPath = "";
            while (!Helpers.ValidatePSO2Dir (folderPath)) {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog ();
                fbd.Description = "Select the pso2 data/win32 directory";
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                fbd.ShowNewFolderButton = false;

                if (fbd.ShowDialog () == System.Windows.Forms.DialogResult.OK) {
                    folderPath = fbd.SelectedPath;
                } else {
                    Environment.Exit (-1);
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
        public void ModChanged () {
            SelectedPresenter.Setup (Mods.SelectedMod, Mods.IsInstalled (Mods.SelectedMod));
        }

        /// <summary>
        /// Event hook that enables the updates button after
        /// certain time passes, and stops the time.
        /// </summary>
        private void ReenableUpdates (object sender, EventArgs e) {
            updatesTimer.Stop ();
            CheckForUpdatesBtn.IsEnabled = true;
        }

        /// <summary>
        /// Asks the mod manager to check for updates
        /// </summary>
        private async void CheckForUpdates () {
            var controller = await this.ShowProgressAsync ("Please wait...", "Now checking update.");
            controller.SetIndeterminate ();

            Mods.OnError += UpdateCheckError;
            bool success = await Mods.CheckForUpdates ();
            Mods.OnError -= UpdateCheckError;

            if (controller.IsCanceled)
                return; //canceled progressdialog auto closes.

            if (success) {
                CheckForUpdatesBtn.IsEnabled = false;
                updatesTimer = new System.Windows.Threading.DispatcherTimer ();
                updatesTimer.Tick += new EventHandler (ReenableUpdates);
                updatesTimer.Interval = new TimeSpan (0, 5, 0);
                updatesTimer.Start ();
            }

            await controller.CloseAsync ();
        }

        private void UpdateCheckError (string message) {
            ShowMessageBox (message, "Error downloading mod");
        }

        private async void ShowMessageBox (string message, string title) {
            MessageDialogResult result = await this.ShowMessageAsync (title, message);
        }

        private void FindAndInstallMod () {
            System.Windows.Forms.OpenFileDialog fd = new System.Windows.Forms.OpenFileDialog ();

            // Set filter options and filter index.
            fd.Filter = "PSO2 Mod files (.zip)|*.zip";
            fd.FilterIndex = 1;
            fd.Multiselect = true;

            // Process input if the user clicked OK.
            if (fd.ShowDialog () == System.Windows.Forms.DialogResult.OK) {
                Mods.AddLocalMod (fd.FileName);
                InstalledModsTab.Focus ();
            }
        }

        #region Input Events

        private void DownloadUrlTextbox_TextChanged (object sender, TextChangedEventArgs e) {
            ValidateUrlInput ();
        }

        private void DownloadModBtn_Click (object sender, RoutedEventArgs e) {
            DownloadMod (DownloadUrlTextbox.Text);
        }

        private void CheckForUpdatesBtn_Click (object sender, RoutedEventArgs e) {
            CheckForUpdates ();
        }

        private void InstallUninstallBtn_Click (object sender, RoutedEventArgs e) {
            Mods.ToggleMod ();
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
            Mods.Delete ();
        }

        private void UpdateBtn_Click (object sender, RoutedEventArgs e) {
            UpdateSelectedMod ();
        }

        private void ViewSiteBtn_Click (object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start ("http://pso2mod.com/?p=" + SelectedPresenter.Id);
        }

        private void Browser_TitleChanged (object sender, DependencyPropertyChangedEventArgs e) {
            DownloadAction duh = new DownloadAction ();
            duh.Url = "http://google.com";
            try {
                JsonSerializer.SerializeToString<DownloadAction> (duh);
                DownloadAction da = JsonSerializer.DeserializeFromString<DownloadAction> (e.NewValue.ToString ());
                if (da.Url != null) {
                    DownloadMod (da.Url);
                }
            } catch {
                // Not valid json: Note it would be better to just run a json validation method,
                // but there doesn't seem to be anything on servicestack.text for that
            }
        }

        private void InstallLocalModBtn_Click (object sender, RoutedEventArgs e) {
            FindAndInstallMod ();
        }

        #endregion Input Events

        private void ModImage_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e) {
            // show FrameworkElement.
            var image = new Image ();
            image.Source = ModImage.Source;
            LightBox.Show (this, image);
        }

        private void Browser_LoadingStateChanged (object sender, CefSharp.LoadingStateChangedEventArgs e) {
            if (e.IsLoading)
            {
                this.Dispatcher.Invoke(() => {
                    ProgressRing.Visibility = Visibility.Visible;
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    ProgressRing.Visibility = Visibility.Hidden;
                });
            }
        }
    }
}
