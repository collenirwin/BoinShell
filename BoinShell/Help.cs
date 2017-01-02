using System.Collections.Generic;

namespace BoinShell {
    public class Help : Command {
        public Help() : base(new string[] { "help", "h" }, "lists all commands with their help messages, or displays the help message for a given command (ex: h ls)") { }

        public override void run() {
            var lstCmds = new List<Command>();

            // all help messages
            foreach (var cmd in Program.cmds.Values) {

                // if we haven't got it already (no duplicates)
                if (!lstCmds.Contains(cmd)) {
                    lstCmds.Add(cmd);
                }
            }

            lstCmds.Sort();

            // print 'em all
            foreach (var cmd in lstCmds) {
                Program.printHelpText(cmd);
            }

            lstCmds.Clear();
        }

        public override void run(string arg) {
            arg = arg.ToLower();

            if (Program.cmds.ContainsKey(arg)) {
                Program.printHelpText(Program.cmds[arg]);
            } else {
                Program.error("\"" + arg + "\" not recognized as a command. Type h for a list of available commands.");
            }
        }
    }
}
