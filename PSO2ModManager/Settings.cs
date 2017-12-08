using System;
using System.Collections.Generic;

namespace PSO2ModManager
{
    public class Settings
    {
        public string PSO2Dir { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<Mod> AvailableMods { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
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
                if (!m.IsValid()) {
                    return false;
                }
            }
            foreach (var m in InstalledMods) {
                if (!m.IsValid()) {
                    return false;
                }
            }
            return true;
        }
    }
}