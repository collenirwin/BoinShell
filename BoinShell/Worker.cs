using System;
using System.Threading.Tasks;

namespace BoinShell
{
    public class Worker
    {
        public event EventHandler done;

        public async void doWork(Action action)
        {
            await Task.Run(() => action());

            if (done != null)
            {
                done(this, new EventArgs());
            }
        }

        public async void doWork<T>(Action<T> action, T arg)
        {
            await Task.Run(() => action(arg));

            if (done != null)
            {
                done(this, new EventArgs());
            }
        }
    }
}
