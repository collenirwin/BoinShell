using System;
using System.IO;

namespace BoinShell
{
    public class Append : Command
    {
        public Append() : base(new string[] { "append" }, "appends text to the specified file (ex: file.txt new text)") { }

        public override void run()
        {
            Program.error("No file path provided.");
        }

        public override void run(string arg)
        {
            try
            {
                var args = Program.splitOptionalArgs(arg);
                args[0] = Program.combinePathPwd(args[0].Trim());

                // append to the file
                using (var writer = File.AppendText(args[0]))
                {
                    writer.WriteLine(args[1]);
                }

            }
            catch (Exception ex)
            {
                Program.argException(arg, "be appended to", ex);
            }
        }
    }
}
