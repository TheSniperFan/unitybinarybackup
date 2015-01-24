using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace unitybinarybackup {
    class BackupCreator {
        /// <summary>
        /// Contains all filetypes we want to back up
        /// </summary>
        private List<string> typeList = new List<string>();

        public BackupCreator() {
            GetFileTypeList();
        }

        /// <summary>
        /// Simulates a backup process. No files will be copied.
        /// </summary>
        /// <returns>Success</returns>
        public bool Simulate() {
            return false;
        }

        /// <summary>
        /// Creates a backup.
        /// </summary>
        /// <param name="backupName">Name of the backup</param>
        /// <returns>Success</returns>
        public bool Backup(string backupName) {
            return false;
        }

        /// <summary>
        /// Prints all the filetypes we want to back up
        /// </summary>
        private void PrintFileTypeList() {
            foreach (string type in typeList)
                Console.WriteLine(type);
        }

        /// <summary>
        /// Extract the filetypes we want to back up from the root gitignore file
        /// </summary>
        private void GetFileTypeList() {
            Console.WriteLine("Parsing .gitignore...");
            using (StreamReader reader = new StreamReader(".gitignore")) {
                // Find the backup comment block
                while (!Regex.IsMatch(reader.ReadLine(), @"^\#.*?\#!UBB!\#.*?$")) ;

                // Extract valid filetype entries (*.something)
                // Metafiles are ignored, since they will be backed up regardless
                while (!reader.EndOfStream) {
                    string currentline = reader.ReadLine();
                    if (Regex.IsMatch(currentline, @"^\*{1}.\w+$")) {
                        typeList.Add(currentline.Split('.')[1]);
                    }
                }
            }
            if (typeList.Count == 0) {
                Console.WriteLine("Nothing to do!");
            }
            else {
                Console.WriteLine("The following filetypes will be backed up:");
                PrintFileTypeList();
            }
        }
    }
}
