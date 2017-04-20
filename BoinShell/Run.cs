using System;
using System.Diagnostics;

namespace BoinShell {
    public class Run : Command {
        public Run() : base(new string[] { "run" }, "executes the specified program with the specified arguments (ex: run program.exe arg.txt)") { }

        public override void run() {
            Program.error("No file path provided.");
        }

        public override void run(string arg) {
            Process process = null;

            try {

                // args[0] -> file to run
                // args[1] -> arguments to pass
                var args = Program.splitOptionalArgs(arg);
                string filePath = Program.getFilePathIfExists(args[0].Trim());
                string fileArgs = args[1].Trim();

                // we're just gonna let Windows handle this, skip our process logic
                if (!Program.canRunHere) {
                    Process.Start(filePath, fileArgs);
                    return;
                }

                process = new Process();

                process.StartInfo = new ProcessStartInfo {
                    FileName  = filePath,
                    Arguments = fileArgs,
                    RedirectStandardInput  = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute = false
                };

                process.OutputDataReceived += new DataReceivedEventHandler(process_OnOutputDataReceived);
                process.ErrorDataReceived  += new DataReceivedEventHandler(process_OnErrorDataReceived);

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.HasExited) {
                    string cmd = Console.ReadLine();

                    // exit has to be our 'ctrl+c'
                    if (cmd == "exit") {
                        break;
                    } else {

                        // talk to the process
                        process.StandardInput.WriteLine(cmd);
                    }
                }

            } catch (Exception ex) {
                Program.argException(arg, "run", ex);
            }

            if (process != null) {
                process.Close();
                process.Dispose();
            }
        }

        private void process_OnOutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                Console.WriteLine(e.Data);
            }
        }

        private void process_OnErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                Program.error(e.Data);
            }
        }
    }
}
