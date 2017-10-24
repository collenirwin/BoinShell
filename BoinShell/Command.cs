using System;
using System.Linq;
using System.Threading;

namespace BoinShell
{
    /// <summary>
    /// Base class for all commands
    /// </summary>
    public abstract class Command : IComparable<Command>
    {
        public string[] aliases { get; protected set; }
        public string helpText { get; protected set; }

        public Command(string[] aliases, string helpText)
        {
            if (aliases == null)
            {
                throw new Exception("[BoinShell.Command Exception] Command.aliases must not be null.");
            }

            this.aliases = aliases;
            for (int x = 0; x < this.aliases.Length; x++)
            {
                // make all aliases lowercase
                this.aliases[x] = this.aliases[x].ToLower();
            }

            this.helpText = helpText;
        }

        public abstract void run(Action callback = null);

        public abstract void run(string arg, Action callback = null);

        //public void runManaged(string arg = null)
        //{
        //    running = true;

        //    ThreadStart thread = null;

        //    if (arg == null)
        //    {
        //        thread = new ThreadStart(run);
        //    }
        //    else
        //    {
                
        //    }

        //    // add a callback
        //    thread += () =>
        //    {

        //    };

        //    cancelled = false;
        //    running = false;
        //}

        //public void cancel()
        //{
        //    cancelled = true;
        //}

        public bool hasAlias(string alias)
        {
            return aliases.Contains(alias.ToLower());
        }

        public int CompareTo(Command other)
        {
            // compare by the first alias
            return this.aliases[0].CompareTo((other as Command).aliases[0]);
        }
    }
}
