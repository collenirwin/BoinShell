namespace BoinShell
{
    public class Exit : Command
    {
        public Exit() : base(new string[] { "exit" }, "terminates BoinShell or the current running program") { }

        public override void run()
        {
            Program.exiting = true;
        }

        public override void run(string arg)
        {
            Program.error("exit does not take any arguments.");
        }
    }
}
