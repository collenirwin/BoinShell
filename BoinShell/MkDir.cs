using System;
using System.IO;

namespace BoinShell
{
    public class MkDir : Command
    {
        public MkDir() : base(new string[] { "mkdir" }, "creates a directory with the specified name (ex: mkdir newdirectory)") { }

        public override void run()
        {
            Program.error("No directory path provided.");
        }

        public override void run(string arg)
        {
            try
            {
                arg = Program.combinePathPwd(arg);

                // if it's not already there or they've given the go ahead to overwrite
                if (!Directory.Exists(arg) || Program.canContinue("\"" + arg + "\" already exists. Do you want to overwrite it? [y/n] "))
                {
                    Directory.CreateDirectory(arg);
                }

            }
            catch (Exception ex)
            {
                Program.argException(arg, "be created", ex);
            }
        }
    }
}
