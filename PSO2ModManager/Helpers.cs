using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using WPFLocalizeExtension.Extensions;
using System.Globalization;
using System.Diagnostics;

namespace PSO2ModManager {
    internal class Helpers {
        public static string CheckMD5 (string filename) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead (filename)) {
                    return string.Concat(md5.ComputeHash (stream).Select(x => x.ToString("X2", CultureInfo.InvariantCulture)));
                }
            }
        }
        #region i18n Functions
        /// <summary>
        /// Get localized resource.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetLocalizedValue<T> (string key) {
            return LocExtension.GetLocalizedValue<T> (Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);
        }
        /// <summary>
        /// Shortcut
        /// </summary>
        /// <param name="key">resource name</param>
        /// <returns></returns>
        public static string _ (string key) {
            dynamic i8n = GetLocalizedValue<string>(key);
            if (i8n == null)
            {
				Debugger.Break();
            }
            return i8n;
        }
        #endregion
        #region PSO2Dir Functions
        public static bool ValidatePSO2Dir (string PSO2Dir) {
            if (!Directory.Exists (PSO2Dir)) {
                return false;
            }
            if (Path.GetFileName (PSO2Dir) != "win32" ||
                !Directory.GetParent (Directory.GetParent (PSO2Dir).FullName).GetFiles().Select (x => x.Name == "pso2.exe").Contains (true)) {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Detect PSO2 Dir via AIDA's registory key
        /// </summary>
        /// <returns></returns>
        public static string DetectPSODir() {
            RegistryKey regkey = Registry.CurrentUser.OpenSubKey (@"Software\AIDA", false);
            if (regkey == null) return null;
            return (string) regkey.GetValue ("PSO2Dir");
        }
        #endregion

        /// <summary>
        /// Download Patch file via Official site
        /// </summary>
        /// <param name="fileName">Patch file name</param>
        /// <param name="destPath">destination</param>
        /// <returns></returns>
        public async static Task DownloadPatch (string fileName, string destPath) {
            const string PATCH_BASE_URI = "http://download.pso2.jp/patch_prod/patches/data/win32/";
            var request = WebRequest.Create (PATCH_BASE_URI + fileName);
            var response = await request.GetResponseAsync();
            var stream = response.GetResponseStream();

            using (var file = new FileStream (destPath + fileName, FileMode.OpenOrCreate, FileAccess.Write)) {
                int read;
                byte[] buffer = new byte[1024];
                while ((read = stream.Read (buffer, 0, buffer.Length)) > 0) {
                    file.Write (buffer, 0, read);
                }
            }
        }
        /// <summary>
        /// Async File Copy
        /// It seems the 32bit values work with atomic writes
        /// </summary>
        /// <see cref="https://www.codeproject.com/Tips/530253/Async-File-Copy-in-Csharp"/>
        public class FileCopy {
            //const string file = @"d:\AppSettings.dat";

            #region KeepThem10MultiplyToAvoidDoubleCopy
            const int sizeBufferCopy = 10485760; //100 MB  Size of the memory buffer
            const int sizeCyclicRead = 1048576; //10 MB   
            #endregion

            private UInt64 indRd, indWr, nrReadings, nrWritings, nrReadsWrites, nrBytesWereRead;
            private bool finishedRd = false, startCopy = true;

            byte[] BufferCopy; //reference only, will be assign by the ReadBytes

            private string destinationFile, sourceFile;

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)")]
            public FileCopy (string s, string d) {
                //BufferCopy = new byte[sizeBufferCopy];
                Console.WriteLine("From: " + s + " To: " + d);
                destinationFile = d;
                sourceFile = s;
            }

            /// <summary>
            /// Copy
            /// </summary>
            /// <returns></returns>
            public bool StartCopy() {
                bool retValue = false;

                //improve check the validity of the file names
                if ((destinationFile != string.Empty) && (sourceFile != string.Empty)) {
                    Thread newThreadCopy = new Thread (ReadWorker);
                    Thread newThreadWrite = new Thread (WriteWorker);

                    newThreadCopy.Start(); //start to read Async to BufferCopy
                    newThreadWrite.Start(); //start to write Async from BufferCopy

                    retValue = true;
                }
                return retValue;
            }

            /// <summary>
            /// WriteWorker
            /// </summary>
            private void WriteWorker() {
                //const string fileName = @"f:\AppSettingsCopy.dat";
                UInt16 SleepTime = 10;

                UInt64 nrBytesWriten = sizeCyclicRead;

                do {
                    Thread.Sleep (10);
                } while (indRd == 0); //Wait for reading to begin

                using (BinaryWriter writer = new BinaryWriter (File.Open (destinationFile, FileMode.Create))) {
                    //check int conversion
                    do {
                        if ((finishedRd == false) && (nrWritings >= (nrReadings - 1)))
                        //prevent writting beyond readIndex, improve with a better condition
                        {
                            Thread.Sleep (SleepTime);
                            SleepTime += 10; //increase 100ms
                            //keep sleeping until Read index progress
                        } else {
                            if (indWr >= sizeBufferCopy) {
                                indWr = 0;
                            }

                            //last incomplete reading
                            if (nrWritings == nrReadsWrites - 1) {
                                nrBytesWriten = nrBytesWereRead;
                            }

                            writer.Write (BufferCopy, (Int32) indWr, (Int32) nrBytesWriten);
                            indWr += sizeCyclicRead;
                            ++nrWritings;
                        }

                    } while (nrWritings < nrReadsWrites);
                } //end using
            }

            /// <summary>
            /// Thread to Read the file
            /// </summary>
            private void ReadWorker() {
                FileStream f = File.Open (sourceFile, FileMode.Open);

                UInt64 Length = (UInt64) f.Length, pos = 0;
                BufferCopy = new byte[sizeBufferCopy];
                nrReadsWrites = Length / sizeCyclicRead + 1;
                UInt16 SleepTime = 10;

                using (BinaryReader binR = new BinaryReader (f)) {
                    while (pos < Length) {
                        startCopy = false;
                        if ((startCopy) || (indRd >= (indWr + sizeBufferCopy - 1)))
                        //the size of the cyclicc buffer is 10 multiple of the bigger BufferCopy
                        {
                            //pause the Read thread, read could be faster
                            //let the buffer to read more in advance
                            Thread.Sleep (SleepTime);
                            SleepTime += 10; //increase 100ms
                            //keep sleeping until Writes index progress
                        } else {
                            //read cicly 1024 bytes, hopefully ReadBytes uses the same buffer for consecutive readings
                            byte[] BufferRd = binR.ReadBytes (sizeCyclicRead);
                            nrBytesWereRead = (UInt64) BufferRd.Length; //still in int size limits

                            //check if the indRd goes out range, begin writing from the begining,
                            // Avoid 2 copies by keeping the indexes multiple by 10
                            if (indRd >= sizeBufferCopy) {
                                indRd = 0;
                            }

                            //improve - without copy !!!
                            Array.Copy (BufferRd, 0, BufferCopy, (Int64) indRd, (Int64) nrBytesWereRead);

                            indRd += nrBytesWereRead;
                            pos += nrBytesWereRead;
                            ++nrReadings;
                        }
                    } //endwhile
                } //end using
                //Reading the file Done
                finishedRd = true;
            }
        }
    }
}
