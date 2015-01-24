using System;

namespace unitybinarybackup {
    class Program {
        static void Main(string[] args) {
            if (UnityProjectTool.ValidateProject()) {
                Console.WriteLine("\nFinished!");
            }
            else {
                Console.WriteLine("\nAborting, due to errors during the validation phase!");
            }

            Console.ReadKey();
        }
    }
}
