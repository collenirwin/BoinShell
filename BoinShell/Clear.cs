using System;

namespace BoinShell
{
    public class Clear : Command
    {
        public Clear() : base(new string[] { "clear", "cls" }, "clears the terminal window") { }

        public override void run(Action callback = null)
        {
            Program.clear();
            if (callback != null) callback.Invoke();
        }

        public override void run(string arg, Action callback = null)
        {
            Program.error("clear does not take any arguments.");
            if (callback != null) callback.Invoke();
        }
    }
}
