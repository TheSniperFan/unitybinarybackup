using System;
using System.IO;
using System.Text.RegularExpressions;

namespace unitybinarybackup {
    class Program {
        /// <summary>
        /// Do we run in simulation mode?
        /// </summary>
        private static bool simulate = false;
        /// <summary>
        /// Do we want to compress the data after backup up?
        /// </summary>
        private static bool compress = false;
        /// <summary>
        /// The name of the backup
        /// </summary>
        private static string backupName;

        static void Main(string[] args) {
            backupName = DateTime.Now.ToString("dd-MM-yy_HH-mm");

            if (ParseOptions(args)) {
                if (!simulate) {
                    CreateBackupDirectory();
                }

                if (UnityProjectTool.ValidateProject()) {
                    bool result = false;
                    if (simulate) {
                        Console.WriteLine("Running in simulation mode!\n");
                        BackupCreator bc = new BackupCreator();
                        result = bc.Simulate();
                    }
                    else {
                        Console.WriteLine(string.Format("Running in backup mode with the following options:\nCompression: {0}\nBackup name: {1}\n", compress, backupName));
                        BackupCreator bc = new BackupCreator();
                        result = bc.Backup(backupName);
                    }

                    Console.WriteLine("\nFinished!");
                }
                else {
                    Console.WriteLine("Aborting, due to errors during the validation phase!");
                }
            }
            else {
                Console.WriteLine("Invalid command line arguments!\n");
                Console.WriteLine("Mode-switches:");
                Console.WriteLine("-B   -   Backup");
                Console.WriteLine("-S   -   Simulate");
                Console.WriteLine("Backup mode options:");
                Console.WriteLine("n   -   Backup name");
                Console.WriteLine("c   -   Enable compression");
                Console.WriteLine("\nExample:");
                Console.WriteLine("unitybinarybackup.exe -Bnc A_Backup");
            }
            Console.ReadKey();
        }

        /// <summary>
        /// Creates a directory for the backup. Checks whether one already exits and renames the current backup accordingly.
        /// </summary>
        private static void CreateBackupDirectory() {
            if (!Directory.Exists("UBB")) {
                Directory.CreateDirectory("UBB");
            }

            string[] directories = Directory.GetDirectories("UBB", backupName + "*", SearchOption.TopDirectoryOnly);

            if (directories.Length > 0) {
                Console.WriteLine(string.Format("Backup(s) with name '{0}' already exists! New backup will be named '{1}'!\n", backupName, backupName += "_(" + (directories.Length + 1) + ")"));
            }

            Directory.CreateDirectory("UBB\\" + backupName);
        }

        /// <summary>
        /// Parses the command line arguments
        /// </summary>
        /// <param name="args">Passed command line arguments</param>
        /// <returns>Are the options valid</returns>
        private static bool ParseOptions(string[] args) {
            if (args.Length == 0 || args.Length > 2) {
                return false;
            }

            args[0] = args[0].ToUpper();
            if (args.Length == 1) {
                // Simulation mode (-S)
                if (Regex.IsMatch(args[0], @"^-S$", RegexOptions.IgnoreCase)) {
                    simulate = true;
                    return true;
                }
                // Backup mode with possible compression (-B or -Bc)
                else if (Regex.IsMatch(args[0], @"^-Bc?$", RegexOptions.IgnoreCase)) {
                    simulate = false;
                    compress = args[0].Contains("C");
                    return true;
                }
                // Invalid
                else {
                    return false;
                }
            }
            else {
                // We concatenate the first and the second argument (options and backup name). It must start with a -B, contain a n
                // may a c for compression and end with a whitespace followed by a valid filename
                if (Regex.IsMatch(string.Join(" ", args[0], args[1]), @"^-B(c?n|nc?)\s{1}\w+$", RegexOptions.IgnoreCase)) {
                    compress = args[0].Contains("C");
                    backupName = args[1];
                    return true;
                }
                // Invalid
                else {
                    return false;
                }
            }
        }
    }
}
