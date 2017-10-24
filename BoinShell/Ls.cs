using System;
using System.IO;
using System.Threading.Tasks;

namespace BoinShell
{
    public class Ls : AsyncCommand
    {
        public Ls() : base(new string[] { "ls", "dir" }, "lists files and folders within current directory") { }

        public override void run(Action callback = null)
        {
            run(Program.pwd.FullName, callback);
        }

        public override async void run(string arg, Action callback = null)
        {
            if (Directory.Exists(arg))
            {
                start();
                
                Action action = () =>
                {
                    var helper = new AsyncHelper(this);
                    helper.lsHelper(new DirectoryInfo(arg), 2);
                };

                await Task.Run(() => action());

                end();
            }
            else
            {
                Program.error("Directory \"" + arg + "\" doesn't exist.");
            }

            if (callback != null) callback.Invoke();
        }
    }
}
