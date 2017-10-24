using System;
using System.IO;
using System.Threading.Tasks;

namespace BoinShell
{
    public class Tree : AsyncCommand
    {
        public Tree() : base(new string[] { "tree" }, "displays the directory tree starting from the current directory and going down") { }

        public override void run(Action callback = null)
        {
            run(Program.pwd.FullName, callback);
        }

        public override async void run(string arg, Action callback = null)
        {
            arg = Program.combinePathPwd(arg);

            if (Directory.Exists(arg))
            {
                start();

                Action action = () =>
                {
                    var helper = new AsyncHelper(this);
                    helper.lsHelper(new DirectoryInfo(arg), 0, true);
                };

                //task = new Task(action);
                //task.Start();

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
