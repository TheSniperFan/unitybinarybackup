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

        /// <summary>
        /// Contains all files we want to back up
        /// </summary>
        private List<string> fileList = new List<string>();
        /// <summary>
        /// Contains every directory and subdirectory of files we back up
        /// </summary>
        private HashSet<string> directoryList = new HashSet<string>();
        /// <summary>
        /// Contains all the metafiles we back up
        /// </summary>
        private List<string> metaFileList = new List<string>();

        /// <summary>
        /// Backup size in byte
        /// </summary>
        private long backupSize = 0;

        public BackupCreator() {
            GetFileTypeList();
        }

        /// <summary>
        /// Simulates a backup process. No files will be copied.
        /// </summary>
        /// <returns>Success</returns>
        public bool Simulate() {
            return Backup(null, false, false);
        }

        /// <summary>
        /// Creates a backup.
        /// </summary>
        /// <param name="backupName">Name of the backup</param>
        /// <param name="compression">Compress the backup</param>
        /// <param name="writeBackup">Should the backup be written?</param>
        /// <returns>Success</returns>
        public bool Backup(string backupName, bool compression = false, bool writeBackup = true) {
            if (!BuildFileList()) {
                Console.WriteLine("No files found! Aborting...");
                return false;
            }

            BuildDirectoryList();

            if (!BuildMetaFileList()) {
                Console.WriteLine("Missing metafiles! Abroting...");
                return false;
            }

            Console.WriteLine(string.Format("\n\n\nSummary:\n{0} file(s), {1} directorie(s) and {2} metafile(s) were selected for backing up.\nRaw backup size: {3:0.00} MB",
                fileList.Count, directoryList.Count, metaFileList.Count, backupSize / 1048576.0f));


            return false;
        }

        private void CompressBackup() {
        }

        /// <summary>
        /// Recursively searches the 'Assets' folder for files to back up. Adds them to the list.
        /// </summary>
        /// <returns>Were any files found?</returns>
        private bool BuildFileList() {
            string[] foundFiles;
            Console.WriteLine("Searching for files in Assets...");
            foreach (string type in typeList) {
                Console.WriteLine(string.Format("Searching for files of type '{0}'", type.Split('.')[1]));
                foundFiles = Directory.GetFiles("Assets", type, SearchOption.AllDirectories);
                foreach (string file in foundFiles) {
                    fileList.Add(file);
                    backupSize += (new FileInfo(file).Length);
                }
            }

            Console.WriteLine(string.Format("Found {0} file(s).\n", fileList.Count));
            return fileList.Count > 0;
        }

        /// <summary>
        /// Retrieve all subdirectories where files were found.
        /// </summary>
        private void BuildDirectoryList() {
            string absoluteAssetPath = Path.GetFullPath("Assets");
            Uri absoluteAssetPathUri = new Uri(absoluteAssetPath, UriKind.Absolute);

            string parent;

            Console.Write(string.Format("Building directory list for {0} files...", fileList.Count));
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

            Console.WriteLine("DONE\n");
        }

        /// <summary>
        /// Checks whether metafiles for all files and folders are available.
        /// </summary>
        /// <returns>Every file and folder has its own metafile.</returns>
        private bool BuildMetaFileList() {
            string metafile = "";
            bool success = true;

            Console.WriteLine("Searching for metafiles...");
            foreach (string file in fileList) {
                if (GetMetaFile(file, ref metafile)) {
                    metaFileList.Add(metafile);
                }
                else {
                    Console.WriteLine(string.Format("No metafile was found for '{0}'!", file));
                    success = false;
                }
            }
            foreach (string dir in directoryList) {
                if (GetMetaFile(dir, ref metafile)) {
                    metaFileList.Add(metafile);
                }
                else {
                    Console.WriteLine(string.Format("No metafile was found for '{0}'!", dir));
                    success = false;
                }
            }

            Console.WriteLine("Metafile search finished...");
            return success;
        }

        /// <summary>
        /// Seaches for a metafile given a file or folder.
        /// </summary>
        /// <param name="file">File or folder</param>
        /// <param name="metafile">Metafile</param>
        /// <returns>Metafile was found.</returns>
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
