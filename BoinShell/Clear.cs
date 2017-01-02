namespace BoinShell {
    public class Clear : Command {
        public Clear() : base(new string[] { "clear", "cls" }, "clears the terminal window") { }

        public override void run() {
            Program.clear();
        }

        public override void run(string arg) {
            Program.error("clear does not take any arguments.");
        }
    }
}
