using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PSO2ModManager
{
    internal static class Helpers
    {
        public static string CheckMD5(string filename) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(filename)) {
                    return string.Concat(md5.ComputeHash(stream).Select(x => x.ToString("X2")));
                }
            }
        }

        public static bool ValidatePSO2Dir(string PSO2Dir) {
            if (!Directory.Exists(PSO2Dir)) {
                return false;
            }
            if (Path.GetFileName(PSO2Dir) != "win32" ||
                !Directory.GetParent(Directory.GetParent(PSO2Dir).FullName).GetFiles().Select(x => x.Name == "pso2.exe").Contains(true)) {
                return false;
            }
            return true;
        }
    }
}