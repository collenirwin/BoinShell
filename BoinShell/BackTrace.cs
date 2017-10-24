using System;

namespace BoinShell
{
    public class BackTrace : Command
    {
        public BackTrace() : base(new string[] { "backtrace" }, "shows all the places you've been this session") { }

        public override void run(Action callback = null)
        {
            foreach (var place in Program.backTrace)
            {
                Program.colorPrintln(place, Program.DIRECTORY_COLOR);
            }

            if (callback != null) callback.Invoke();
        }

        public override void run(string arg, Action callback = null)
        {
            Program.error("backtrace does not take any arguments.");
            if (callback != null) callback.Invoke();
        }
    }
}
