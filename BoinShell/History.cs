using System.Linq;

namespace BoinShell {
    public class History : Command {
        public History() : base(new string[] { "history" }, "shows the commands you've used this session, or sets the number it keeps track of (ex: history 50)") { }

        public override void run() {
            var history = TabComplete.history.ToList();
            history.Reverse(); // reversing order so it shows last used at the bottom

            foreach (var cmd in history) {
                Program.colorPrintln(cmd, Program.commandColor);
            }
        }

        public override void run(string arg) {
            int newSize = 0;
            if (int.TryParse(arg, out newSize)) {
                TabComplete.historySize = newSize; // set a new history size
            } else {
                Program.error("Expected an integer, recieved \"" + arg + "\"");
            }
        }
    }
}
