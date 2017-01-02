using System;
using System.IO;

namespace BoinShell {
    public class RmDir : Command {
        public RmDir() : base(new string[] { "rmdir" }, "deletes the specified directory (ex: rmdir newdirectory)") { }

        public override void run() {
            Program.error("No directory path provided.");
        }

        public override void run(string arg) {
            try {
                arg = Path.Combine(Program.pwd.FullName, arg);

                if (Directory.Exists(arg)) {
                    if (Program.canContinue("Are you sure you want to delete \"" + arg + "\"? [y/n] ")) {
                        Directory.Delete(arg);
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
