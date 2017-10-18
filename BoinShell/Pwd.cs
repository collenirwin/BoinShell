namespace BoinShell
{
    public class Pwd : Command
    {
        public Pwd() : base(new string[] { "pwd", "whereami" }, "prints the full path to the present working directory") { }

        public override void run()
        {
            Program.colorPrintln(Program.pwd.FullName, Program.DIRECTORY_COLOR);
        }

        public override void run(string arg)
        {
            run();
        }
    }
}
