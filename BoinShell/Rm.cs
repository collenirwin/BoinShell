using System;
using System.IO;

namespace BoinShell {
    public class Rm : Command {
        public Rm() : base(new string[] { "rm" }, "deletes the specified file (ex: rm file.txt)") { }

        public override void run() {
            Program.error("No file path provided.");
        }

        public override void run(string arg) {
            try {
                arg = Path.Combine(Program.pwd.FullName, arg);

                if (File.Exists(arg)) {
                    if (Program.canContinue("Are you sure you want to delete \"" + arg + "\"? [y/n] ")) {
                        File.Delete(arg);
                    }

                } else {
                    Program.error("\"" + arg + "\" doesn't exist.");
                }

            } catch (Exception ex) {
                Program.argException(arg, "be removed", ex);
            }
        }
    }
}
