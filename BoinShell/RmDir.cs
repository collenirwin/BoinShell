﻿using System;
using System.IO;

namespace BoinShell
{
    public class RmDir : Command
    {
        public RmDir() : base(new string[] { "rmdir" }, "deletes the specified directory and all files and directories within it (ex: rmdir mydirectory)") { }

        public override void run()
        {
            Program.error("No directory path provided.");
        }

        public override void run(string arg)
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
        }
    }
}
