using System;
using System.ComponentModel;

namespace PSO2ModManager
{
    public class ModPresenter : INotifyPropertyChanged
    {
        public string Id {
            get {
                return id;
            }
            private set {
                id = value;
                NotifyPropertyChanged("Id");
                NotifyPropertyChanged("CanViewOnline");
            }
        }

        public string Thumbnail {
            get {
                return thumbnail;
            }
            private set {
                thumbnail = value;
                NotifyPropertyChanged("Thumbnail");
            }
        }

        public string Description {
            get {
                return description;
            }
            private set {
                description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public string Name {
            get {
                return name;
            }
            private set {
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        public bool CanDelete {
            get {
                return canDelete;
            }
            private set {
                canDelete = value;
                NotifyPropertyChanged("CanDelete");
            }
        }

        public bool CanUpdate {
            get {
                return canUpdate;
            }
            private set {
                canUpdate = value;
                NotifyPropertyChanged("CanUpdate");
            }
        }

        public bool CanInstallUninstall {
            get {
                return canInstallUninstall;
            }
            private set {
                canInstallUninstall = value;
                NotifyPropertyChanged("CanInstallUninstall");
            }
        }

        public String InstallUpdateBtnValue {
            get {
                return installUpdateBtnValue;
            }
            private set {
                installUpdateBtnValue = value;
                NotifyPropertyChanged("InstallUpdateBtnValue");
            }
        }

        public bool CanViewOnline {
            get {
                return id != string.Empty;
            }
        }

        private string id = string.Empty;
        private string thumbnail = string.Empty;
        private string description = string.Empty;
        private string name = string.Empty;
        private bool canDelete = false;
        private bool canUpdate = false;
        private bool canInstallUninstall = false;
        private bool installed = false;
        private string installUpdateBtnValue = "Install";

        protected void NotifyPropertyChanged(String propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Setup(Mod m, bool mInstalled) {
            if (m == null) {
                Id = string.Empty;
                Thumbnail = string.Empty;
                Description = string.Empty;
                installed = false;
                CanInstallUninstall = false;
                CanUpdate = false;
            } else {
                installed = mInstalled;
                Id = m.Id;
                Thumbnail = AppDomain.CurrentDomain.BaseDirectory + "\\thumbnails\\" + m.Thumbnail;
                Description = "<!DOCTYPE html><html lang='en-US'><head><meta charset='UTF-8'></head><body><style> * { margin:0 } </style>" + String.Format("<p><strong>{0} by {1}</strong></p><p>{2}</p></body></html>", m.Name, m.Author, m.Description);
                if (mInstalled) {
                    InstallUpdateBtnValue = "Uninstall";
                } else {
                    InstallUpdateBtnValue = "Install";
                }
                CanInstallUninstall = true;
                CanUpdate = m.UpdateAvailable;
                CanDelete = true;
            }
        }
    }
}