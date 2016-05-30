using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PSO2ModManager
{
    public class Settings
    {
        public string PSO2Dir { get; set; }
        public List<Mod> AvailableMods { get; set; }
        public List<Mod> InstalledMods { get; set; }

        public static readonly string SettingsPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "settings.json";

        /// <summary>
        /// Checks if the updates file isn't damaged 
        /// </summary>
        public bool IsValid() {
            if (AvailableMods == null || InstalledMods == null || PSO2Dir == null) {
                return false;
            }
            if (Helpers.ValidatePSO2Dir(PSO2Dir) == false) {
                return false;
            }
            foreach (var m in AvailableMods) {
                if (!m.isValid()) {
                    return false;
                }
            }
            foreach (var m in InstalledMods) {
                if (!m.isValid()) {
                    return false;
                }
            }
            return true;
        }
    }
}