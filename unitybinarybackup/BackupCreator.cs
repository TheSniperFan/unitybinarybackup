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

        private List<string> fileList = new List<string>();
        private HashSet<string> directoryList = new HashSet<string>();
        private List<string> metaFileList = new List<string>();

        private long backupSize = 0;

        public BackupCreator() {
            GetFileTypeList();
        }

        /// <summary>
        /// Simulates a backup process. No files will be copied.
        /// </summary>
        /// <returns>Success</returns>
        public bool Simulate() {
            BuildFileList();
            BuildDirectoryList();
            BuildMetaFileList();

            Console.WriteLine(string.Format("\n\n\nSummary:\n{0} file(s), {1} directorie(s) and {2} metafile(s) were selected for backing up.\nRaw backup size: {3:0.00} MB",
                fileList.Count, directoryList.Count, metaFileList.Count, backupSize / 1048576.0f));

            return true;
        }

        /// <summary>
        /// Creates a backup.
        /// </summary>
        /// <param name="backupName">Name of the backup</param>
        /// <returns>Success</returns>
        public bool Backup(string backupName) {
            return false;
        }

        private bool BuildFileList() {
            string[] foundFiles;
            foreach (string type in typeList) {
                foundFiles = Directory.GetFiles("Assets", type, SearchOption.AllDirectories);
                foreach (string file in foundFiles) {
                    fileList.Add(file);
                    backupSize += (new FileInfo(file).Length);
                }
            }

            return fileList.Count > 0;
        }

        private void BuildDirectoryList() {
            string absoluteAssetPath = Path.GetFullPath("Assets");
            Uri absoluteAssetPathUri = new Uri(absoluteAssetPath, UriKind.Absolute);
            
            string parent;
            
            foreach (string file in fileList) {
                parent = file;
                do {
                    parent = Directory.GetParent(parent).ToString();
                    if (parent != absoluteAssetPath) {
                        Uri parentUri = new Uri(parent, UriKind.Absolute);
                        directoryList.Add(Uri.UnescapeDataString(absoluteAssetPathUri.MakeRelativeUri(parentUri).ToString()));
                    }
                } while (parent != absoluteAssetPath);
            }
        }

        private bool BuildMetaFileList() {
            string metafile = "";
            foreach (string file in fileList) {
                if (GetMetaFile(file, ref metafile)) {
                    metaFileList.Add(metafile);
                }
                else {
                    return false;
                }
            }
            foreach (string dir in directoryList) {
                if (GetMetaFile(dir, ref metafile)) {
                    metaFileList.Add(metafile);
                }
                else {
                    return false;
                }
            }

            return true;
        }

        private bool GetMetaFile(string file, ref string metafile) {
            if (File.Exists(file + ".meta")) {
                metafile = file + ".meta";
                return true;
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Prints all the filetypes we want to back up
        /// </summary>
        private void PrintFileTypeList() {
            foreach (string type in typeList)
                Console.Write(type.Split('.')[1] + " ");

            Console.WriteLine("\n");
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
                        typeList.Add(currentline);
                    }
                }
            }
            if (typeList.Count == 0) {
                Console.WriteLine("Nothing to do!");
            }
            else {
                Console.Write("The following filetypes will be backed up: ");
                PrintFileTypeList();
            }
        }
    }
}
