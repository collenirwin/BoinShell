using System.IO;

namespace BoinShell
{
    public class Ls : Command
    {
        public Ls() : base(new string[] { "ls", "dir" }, "lists files and folders within current directory") { }

        public override void run()
        {
            run(Program.pwd.FullName);
        }

        public override void run(string arg)
        {
            if (Directory.Exists(arg))
            {
                Program.lsHelper(new DirectoryInfo(arg), 2);
            }
            else
            {
                Program.error("Directory \"" + arg + "\" doesn't exist.");
            }
        }
    }
}
