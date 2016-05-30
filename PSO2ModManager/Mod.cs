using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace PSO2ModManager
{
    public class Mod: INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string File { get; set; }
        public string Slug { get; set; }
        public string Thumbnail { get; set; }
        public Dictionary<string, string> ContentsMD5 { get; set; }

        [IgnoreDataMember]
        public string ToolInfo { get; set; } = "";

        [IgnoreDataMember]
        public static readonly string ModBrokenMessage = "(Broken)";

        [IgnoreDataMember]
        public static readonly string ModUpdateMessage = "(Update Available)";
        
        [IgnoreDataMember]
        public static readonly string[] ModSettingsFiles = new string[] { "settings.csv", "targets.csv", "options.csv" };

        [IgnoreDataMember]
        public static readonly string InstallPath = AppDomain.CurrentDomain.BaseDirectory + "\\mods\\";

        [IgnoreDataMember]
        public static readonly string ImagePath = AppDomain.CurrentDomain.BaseDirectory + "\\thumbnails\\";

        [IgnoreDataMember]
        public static readonly string BackupPath = AppDomain.CurrentDomain.BaseDirectory + "\\backups\\";

        public bool UpdateAvailable {
            get {
                return updateAvailable;
            }

            set {
                if (value == true && this.ToolInfo == null) {
                    this.ToolInfo = ModUpdateMessage;
                } else if (value == true && !this.ToolInfo.Contains(ModUpdateMessage)) {
                    this.ToolInfo += ModUpdateMessage;
                }
                updateAvailable = value;
                NotifyPropertyChanged("UpdateAvailable");

            }
        }

        [IgnoreDataMember]
        public bool Busy {
            get {
                return busy;
            }

            set {
                busy = value;
                NotifyPropertyChanged("Busy");
            }
        }

        private bool busy = false;
        private bool updateAvailable = false;

        public Mod(string id, string name, DateTime date, string description, string author, string url, string file, string slug, string thumbnail)
        {
            this.Id = id;
            this.Name = name;
            this.Date = date;
            this.Description = description;
            this.Author = author;
            this.Url = url;
            this.File = file;
            this.Slug = slug;
            this.Thumbnail = thumbnail;
        }

        /// <summary>
        /// Checks if a mod is valid. Note: This is kinda placeholderish. 
        /// Note that it doesn't check if the mod data is correct or well installed.
        /// </summary
        public bool isValid() {
            if (Id == null || Name == null || Date == null || Description == null || Author == null || Url == null || File == null || Slug == null || Thumbnail == null) {
                return false;
            }                
            return true;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

}
