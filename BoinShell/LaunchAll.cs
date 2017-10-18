using System.Diagnostics;
using System.IO;

namespace BoinShell
{
    public class LaunchAll : Command
    {
        public LaunchAll() : base(new string[] { "launchall", "lall" }, "runs all programs in the current or specified directory via the OS") { }

        public override void run()
        {
            run(Program.pwd.FullName);
        }

        public override void run(string arg)
        {
            try
            {
                arg = Program.combinePathPwd(arg);
                var dir = new DirectoryInfo(arg);

                foreach (var file in dir.GetFiles())
                {
                    try
                    {

                        // launching "filename" ...
                        Program.colorPrint("launching \"", Program.DEFAULT_COLOR);
                        Program.colorPrint(file.Name, Program.EXECUTABLE_COLOR);
                        Program.colorPrintln("\" ...", Program.DEFAULT_COLOR);

                        // run the file
                        Process.Start(file.FullName);

                        // "filename" launched successfully
                        Program.colorPrint(" \"", Program.DEFAULT_COLOR);
                        Program.colorPrint(file.Name, Program.EXECUTABLE_COLOR);
                        Program.colorPrintln("\" launched successfully\n", Program.DEFAULT_COLOR);

                    }
                    catch
                    {
                        Program.error(" \"" + file.Name + "\" failed to launch\n");
                    }
                }
            }
            catch
            {
                Program.error("launchall failed to run in \"" + arg + "\"");
            }
        }
    }
}
