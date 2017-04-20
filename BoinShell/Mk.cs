using System;
using System.IO;

namespace BoinShell {
    public class Mk : Command {
        public Mk() : base(new string[] { "mk" }, "creates a file with the specified name, and writes the specified text to it (ex: mk file.txt Hello World!)") { }

        public override void run() {
            Program.error("No file path provided.");
        }

        public override void run(string arg) {
            try {
                var args = Program.splitOptionalArgs(arg);
                args[0]  = Program.combinePathPwd(args[0].Trim());

                // if it's not already there or they've given the go ahead to overwrite
                if (!File.Exists(args[0]) || Program.canContinue("\"" + args[0] + "\" already exists. Do you want to overwrite it? [y/n] ")) {

                    // create the file
                    using (var writer = File.CreateText(args[0])) {
                        writer.WriteLine(args[1]); // write to it
                    }
                }

            } catch (Exception ex) {
                Program.argException(arg, "be created", ex);
            }
        }
    }
}
