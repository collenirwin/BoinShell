using System;

namespace BoinShell
{
    public class Pwd : Command
    {
        public Pwd() : base(new string[] { "pwd", "whereami" }, "prints the full path to the present working directory") { }

        public override void run(Action callback = null)
        {
            Program.colorPrintln(Program.pwd.FullName, Program.DIRECTORY_COLOR);
            if (callback != null) callback.Invoke();
        }

        public override void run(string arg, Action callback = null)
        {
            Program.error("pwd does not take any arguments.");
            if (callback != null) callback.Invoke();
        }
    }
}
