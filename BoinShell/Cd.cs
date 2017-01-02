using System;
using System.IO;

namespace BoinShell {
    public class Cd : Command {
        public Cd() : base(new string[] { "cd" }, "changes the current working directory to a given directory (ex: cd \\folder)") { }

        public override void run() {

            // change to home directory
            run(Environment.ExpandEnvironmentVariables("%USERPROFILE%"));
        }

        public override void run(string arg) {
            arg = arg.Trim();

            if (arg == "..") {
                if (Program.pwd.Parent != null && Program.pwd.Parent.Exists) {
                    updateDirInfo(Program.pwd.Parent);
                }

            } else {
                string newDir = Program.getPathDir(arg);

                if (Directory.Exists(newDir)) {
                    updateDirInfo(new DirectoryInfo(newDir));
                } else {
                    Program.error("Couldn't find a directory with the path \"" + newDir + "\"");
                }
            }

            
        }

        private void updateDirInfo(DirectoryInfo newDir) {

            // change dir
            Program.pwd = newDir;

            // update backtrace
            Program.backTrace.Add(Program.pwd.FullName);

            // update title bar text
            Program.updateTitle();
        }
    }
}
