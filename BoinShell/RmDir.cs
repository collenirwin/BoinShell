using System;
using System.IO;

namespace BoinShell
{
    public class RmDir : Command
    {
        public RmDir() : base(new string[] { "rmdir" }, "deletes the specified directory and all files and directories within it (ex: rmdir mydirectory)") { }

        public override void run(Action callback = null)
        {
            Program.error("No directory path provided.");
            if (callback != null) callback.Invoke();
        }

        public override void run(string arg, Action callback = null)
        {
            try
            {
                arg = Program.combinePathPwd(arg);

                if (Directory.Exists(arg))
                {
                    if (Program.canContinue(
                        "Are you sure you want to delete \"" + arg + "\" and all files and directories witin it? [y/n] "))
                    {
                        // recursively delete the specified directory
                        Directory.Delete(arg, true);
                    }
                }
                else
                {
                    Program.error("\"" + arg + "\" doesn't exist.");
                }
            }
            catch (Exception ex)
            {
                Program.argException(arg, "be removed", ex);
            }

            if (callback != null) callback.Invoke();
        }
    }
}
