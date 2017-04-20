using System.IO;

namespace BoinShell {
    public class Tree : Command {
        public Tree() : base(new string[] { "tree" }, "displays the directory tree starting from the current directory and going down") { }

        public override void run() {
            run(Program.pwd.FullName);
        }

        public override void run(string arg) {
            arg = Program.combinePathPwd(arg);

            if (Directory.Exists(arg)) {
                Program.lsHelper(new DirectoryInfo(arg), 0, true);
            } else {
                Program.error("Directory \"" + arg + "\" doesn't exist.");
            }
        }
    }
}
