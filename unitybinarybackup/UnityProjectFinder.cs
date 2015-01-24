using System;
using System.IO;
using System.Text.RegularExpressions;

namespace unitybinarybackup {
    static class UnityProjectTool {
        /// <summary>
        /// These directories should be in every unity project directorie
        /// </summary>
        private static string[] dirList = {"Assets", "Library", "ProjectSettings"};

        /// <summary>
        /// This performs a few simple checks to determine whether we're running from a valid
        /// project directory.
        /// </summary>
        /// <returns>Availabilty of necessary files and directories</returns>
        public static bool ValidateProject() {
            Console.WriteLine("Checking whether we're running from a valid directory...\n");
            
            bool result = ValidateUnityProject() && ValidateGitGitignore();
            Console.WriteLine(result ? "\nWorking directory seems to be valid.\n" : "\nThere were errors!\n");
            
            return result;
        }

        /// <summary>
        /// Checks whether the current working directory appears to be a Unity project.
        /// </summary>
        /// <returns>All #dirList directories were found</returns>
        private static bool ValidateUnityProject() {
            Console.WriteLine("Searching default Unity project folders...");

            foreach (string folder in dirList) {
                Console.Write(string.Format("Searching for \"{0}\" folder...", folder));
                if (!Directory.Exists(folder)) {
                    Console.WriteLine("MISSING");
                    return false;
                }
                else
                    Console.WriteLine("FOUND");
            }

            Console.WriteLine("All folders found!\n");
            return true;
        }

        /// <summary>
        /// Searches the .gitignore file and checks whether it contains a backup comment block (#!UBB!#)
        /// </summary>
        /// <returns>Is the .gitignore valid</returns>
        private static bool ValidateGitGitignore() {
            Console.Write("Searching .gitignore file...");
            if (!File.Exists(".gitignore")) {
                Console.WriteLine("MISSING");
                return false;
            }
            
            Console.WriteLine("FOUND");
            Console.Write("Checking whether .gitignore file is empty...");
            if (new FileInfo(".gitignore").Length == 0) {
                Console.WriteLine("EMPTY");
                return false;
            }
            else {
                Console.WriteLine("OK");
            }

            Console.Write("Searching for backup comment block...");
            bool containsBackupComment = false;
            using (StreamReader reader = new StreamReader(".gitignore")) {
                while (!reader.EndOfStream) {
                    // Line starts with # (gitignore comment) and contains #!UBB!# somewhere
                    if (Regex.IsMatch(reader.ReadLine(), @"^\#.*?\#!UBB!\#.*?$")) {
                        containsBackupComment = reader.Peek() > -1;
                        break;
                    }
                }
            }

            Console.WriteLine(containsBackupComment ? "FOUND" : "MISSING");
            if (!containsBackupComment) {
                return false;
            }

            Console.WriteLine(".gitignore file successfully validated!");
            return true;
        }
    }
}
