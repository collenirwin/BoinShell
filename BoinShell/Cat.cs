using System;
using System.IO;

namespace BoinShell
{
    public class Cat : Command
    {
        public Cat() : base(new string[] { "cat", "show" }, "displays the contents of a specified file (ex: cat file.txt)") { }

        public override void run(Action callback = null)
        {
            Program.error("No file path provided.");
            if (callback != null) callback.Invoke();
        }

        public override void run(string arg, Action callback = null)
        {
            try
            {
                using (var reader = File.OpenText(Program.getFilePathIfExists(arg)))
                {
                    Console.Write(reader.ReadToEnd());
                }

            }
            catch (Exception ex)
            {
                Program.argException(arg, "open", ex);
            }

            if (callback != null) callback.Invoke();
        }
    }
}
