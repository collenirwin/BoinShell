using System;
using System.Threading;
using System.Threading.Tasks;

namespace BoinShell
{
    /// <summary>
    /// Base class for commands that run on separate threads
    /// </summary>
    /// <remarks>
    /// All inherited classes must call start at the beginning of their run overload and stop at the end.
    /// Also, run must have a check for cancelled which will bring it to a halt.
    /// This is very poor design but I don't have a better way at the moment.
    /// </remarks>
    public abstract class AsyncCommand : Command
    {
        public bool running { get; protected set; }
        public bool cancelled { get; private set; }

        public AsyncCommand(string[] aliases, string helpText)
            : base(aliases, helpText)
        {
            running = false;
            cancelled = false;
        }

        /// <summary>
        /// Use this at the start of the run method
        /// </summary>
        protected void start()
        {
            running = true;
            cancelled = false;
        }

        /// <summary>
        /// Use this at the end of the run method
        /// </summary>
        protected void end()
        {
            running = false;
            cancelled = false;
        }

        /// <summary>
        /// Stops the running thread if possible
        /// </summary>
        public void cancel()
        {
            if (running)
            {
                cancelled = true;
            }
        }
    }
}
