using System;
using System.Diagnostics;

namespace BoinShell
{
    public class Run : Command
    {
        public Process process { get; private set; }

        public Run() : base(new string[] { "run" }, "executes the specified program with the specified arguments (ex: run program.exe arg.txt)") { }

        public override void run(Action callback = null)
        {
            Program.error("No file path provided.");
            if (callback != null) callback.Invoke();
        }

        public override void run(string arg, Action callback = null)
        {
            process = null;

            try
            {
                var args = Program.splitOptionalArgs(arg);

                // args[0] -> file to run
                // args[1] -> arguments to pass
                string filePath = Program.getFilePathIfExists(args[0].Trim());
                string fileArgs = args[1].Trim();

                // if the runhere setting is turned off
                if (!Program.canRunHere)
                {
                    // we're just gonna let Windows handle this, skip our process logic
                    Process.Start(filePath, fileArgs);
                    return;
                }

                process = new Process();

                process.StartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    Arguments = fileArgs,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Console.WriteLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        Program.error(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.HasExited)
                {
                    string cmd = Console.ReadLine();

                    // if Ctrl+C was pressed
                    if (cmd == null)
                    {
                        break;
                    }
                    else
                    {
                        // talk to the process
                        process.StandardInput.WriteLine(cmd);
                    }
                }
            }
            catch (Exception ex)
            {
                Program.argException(arg, "run", ex);
            }

            close();
            if (callback != null) callback.Invoke();
        }

        private void dispose()
        {
            if (process != null)
            {
                process.Dispose();
                process = null;
            }
        }

        public void close()
        {
            if (process != null)
            {
                process.Close();
                dispose();
            }
        }

        public void kill()
        {
            if (process != null)
            {
                process.Kill();
                dispose();
            }
        }
    }
}
