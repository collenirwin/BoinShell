using System;
using System.IO;

namespace BoinShell
{
    public class Rm : Command
    {
        public Rm() : base(new string[] { "rm" }, "deletes the specified file (ex: rm file.txt)") { }

        public override void run(Action callback = null)
        {
            Program.error("No file path provided.");
            if (callback != null) callback.Invoke();
        }

        public override void run(string arg, Action callback = null)
        {
            try
            {
                arg = Program.combinePathPwd(arg);

                if (File.Exists(arg))
                {
                    if (Program.canContinue("Are you sure you want to delete \"" + arg + "\"? [y/n] "))
                    {
                        File.Delete(arg);
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
