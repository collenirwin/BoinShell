namespace BoinShell
{
    public class BackTrace : Command
    {
        public BackTrace() : base(new string[] { "backtrace" }, "shows all the places you've been this session") { }

        public override void run()
        {
            foreach (var place in Program.backTrace)
            {
                Program.colorPrintln(place, Program.directoryColor);
            }
        }

        public override void run(string arg)
        {
            Program.error("backtrace does not take any arguments.");
        }
    }
}
