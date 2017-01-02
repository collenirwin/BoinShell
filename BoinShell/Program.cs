using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BoinShell {
    class Program {

        #region Vars

        static bool startup = true;
        static bool exiting = false;
        static bool canRunHere = true;

        static string prompt = "> ";

        static Dictionary<string, Action> cmds = new Dictionary<string, Action>();
        static Dictionary<string, Action<string>> cmdsWithArgs = new Dictionary<string, Action<string>>();
        static Dictionary<string, string> cmdHelp = new Dictionary<string, string>();

        static DirectoryInfo pwd = new DirectoryInfo(Directory.GetCurrentDirectory());

        #region Colors

        const ConsoleColor defaultColor     = ConsoleColor.Gray;
        const ConsoleColor defaultBackColor = ConsoleColor.Black;
        const ConsoleColor directoryColor   = ConsoleColor.DarkGray;
        const ConsoleColor fileColor        = ConsoleColor.Gray;
        const ConsoleColor executableColor  = ConsoleColor.Cyan;
        const ConsoleColor errorColor       = ConsoleColor.Red;

        #endregion

        #endregion

        #region Methods

        static void Main(string[] args) {
            if (startup) {
                loadCmds();

                canRunHere = Properties.Settings.Default.runhere;

                // if we're given a valid path in args, switch pwd to that path
                if (args.Length != 0 && Directory.Exists(args[0])) {
                    pwd = new DirectoryInfo(args[0]);
                }

                updateTitle();
                clear();

                Console.Write("Welcome to ");
                colorPrint("BoinShell", ConsoleColor.Green);
                Console.WriteLine(". Type h for a list of commands.");

                startup = false;
            }

            printPrompt();

            string cmd = TabComplete.readLine(listFilesDirs(pwd), Console.CursorLeft).Trim().ToLower();

            // if we have a valid command
            if (cmd.Length != 0) {

                // check if there are args
                if (cmd.Contains(" ")) {
                    var cmdArgs = cmd.Split(' ');
                    if (cmdArgs.Length > 1 && cmdsWithArgs.ContainsKey(cmdArgs[0])) {

                        string restOfArgs = "";

                        // append the rest of the args given into one big arg, so we can pass it to the cmd to handle it (or them?)
                        for (int x = 1; x < cmdArgs.Length; x++) {
                            restOfArgs += " " + cmdArgs[x];
                        }

                        // run command with argument(s)
                        cmdsWithArgs[cmdArgs[0]].Invoke(restOfArgs.Trim());
                    } else {
                        run(cmd);
                    }

                // no args
                } else if (cmds.ContainsKey(cmd)) {
                    cmds[cmd].Invoke();

                // just try to run it as a program/file
                } else {
                    run(cmd);
                }
            }        

            if (!exiting) {
                Main(args);
            }
        }

        static void loadCmds() {

            // commands without args
            cmds.Add("ls",  ls);
            cmds.Add("dir", ls);
            cmds.Add("clear", clear);
            cmds.Add("cls",   clear);
            cmds.Add("pwd",  pwdCmd);
            cmds.Add("exit", exit);
            cmds.Add("help", help);
            cmds.Add("h",    help);
            cmds.Add("tree", tree);

            // commands with a single argument
            cmdsWithArgs.Add("cd",      cd);
            cmdsWithArgs.Add("run",     run);
            cmdsWithArgs.Add("runhere", runhere);
            cmdsWithArgs.Add("cat",     cat);
            cmdsWithArgs.Add("help",    help);
            cmdsWithArgs.Add("h",       help);
            cmdsWithArgs.Add("mk",      mk);
            cmdsWithArgs.Add("mkdir",   mkdir);
            cmdsWithArgs.Add("rm",      rm);
            cmdsWithArgs.Add("rmdir",   rmdir);
            cmdsWithArgs.Add("append",  append);

            // commands paired with their help text
            cmdHelp.Add("ls",      "ls, dir - list files and folders within current directory");
            cmdHelp.Add("dir",     "ls, dir - list files and folders within current directory");
            cmdHelp.Add("clear",   "clear, cls - clear the terminal window");
            cmdHelp.Add("cls",     "clear, cls - clear the terminal window");
            cmdHelp.Add("pwd",     "pwd - print the full path to the present working directory");
            cmdHelp.Add("exit",    "exit - terminates BoinShell or the current running program");
            cmdHelp.Add("help",    "help, h - lists all commands with their help messages, or displays the help message for a given command (ex: h ls)");
            cmdHelp.Add("h",       "help, h - lists all commands with their help messages, or displays the help message for a given command (ex: h ls)");
            cmdHelp.Add("cd",      "cd - changes the current working directory to a given directory (ex: cd \\folder)");
            cmdHelp.Add("run",     "run - executes the specified program with the specified arguments (ex: run program.exe arg.txt)");
            cmdHelp.Add("runhere", "runhere - determines whether programs should be run within BoinShell or using the Windows shell (ex: runhere on)");
            cmdHelp.Add("cat",     "cat - displays the contents of a specified file (ex: cat file.txt)");
            cmdHelp.Add("mk",      "mk - creates a file with the specified name, and writes the specified text to it (ex: mk file.txt Hello World!)");
            cmdHelp.Add("mkdir",   "mkdir - creates a directory with the specified name (ex: mkdir newdirectory)");
            cmdHelp.Add("rm",      "rm - deletes the specified file (ex: rm file.txt)");
            cmdHelp.Add("rmdir",   "rmdir - deletes the specified directory (ex: rmdir newdirectory)");
            cmdHelp.Add("append",  "append - appends text to the specified file (ex: file.txt new text)");
            cmdHelp.Add("tree",    "tree - displays the directory tree starting from the current directory and going down");
        }

        #region Helpers

        static List<string> listFilesDirs(DirectoryInfo dir) {
            var list = new List<string>();

            foreach (var subdir in dir.GetDirectories()) {
                list.Add(subdir.Name);
            }

            foreach (var file in dir.GetFiles()) {
                list.Add(file.Name);
            }

            list.Sort();
            return list;
        }

        static void updateTitle() {
            Console.Title = "BoinShell - " + pwd.FullName;
        }

        static void printPrompt() {
            colorPrint(pwd.Name, directoryColor);
            colorPrint(prompt, defaultColor);
        }

        static void error(string message, bool newline = true) {
            if (newline) {
                colorPrintln(message, errorColor);
            } else {
                colorPrint(message, errorColor);
            }
        }

        /// <summary>
        /// output:
        /// "ARG" failed to ACTION with the following exception:
        /// EX.Message
        /// </summary>
        static void argException(string arg, string action, Exception ex) {
            error(
                "\"" + arg + "\" failed to " + action + " with the following exception:" +
                Environment.NewLine +
                ex.Message
            );
        }

        static string getPathDir(string path) {
            string dir = pwd.FullName;

            if (Directory.Exists(path)) {
                dir = path;
            } else {
                string pathCombined = Path.Combine(pwd.FullName, path);

                if (Directory.Exists(pathCombined)) {
                    dir = pathCombined;
                }
            }

            return dir;
        }

        static string getPathFile(string path) {
            string pathCombined = Path.Combine(pwd.FullName, path);

            if (File.Exists(pathCombined)) {
                return pathCombined;
            }

            return path;
        }

        static string[] splitOptionalArgs(string text) {

            // default to the original command and an empty string for the optional arg
            string[] args = { text, "" };
            int firstSpace = text.IndexOf(' ');

            // we've got an argument!
            if (firstSpace != -1) {
                args[0] = text.Substring(0, firstSpace);  // first word
                args[1] = text.Substring(firstSpace + 1); // the rest
            }

            return args;
        }

        static bool canContinue(string prompt) {
            Console.Write(prompt);
            return (Console.ReadLine().Trim().ToLower() == "y");
        }

        static void printHelpText(string text) {
            int endOfNames = text.IndexOf(" -");

            // we have a " -"
            if (endOfNames != -1) {

                // highlight the cmd names
                colorPrint(text.Substring(0, endOfNames), ConsoleColor.Yellow);

                // print the rest
                Console.WriteLine(text.Substring(endOfNames));
            } else {
                Console.WriteLine(text);
            }
        }

        static void printSpaces(int count) {
            for (int x = 0; x < count; x++) {
                Console.Write(' ');
            }
        }

        static void lsHelper(DirectoryInfo dirinfo, int spaces = 0, bool recursive = false) {
            foreach (var dir in dirinfo.GetDirectories()) {
                printSpaces(spaces);
                colorPrintln(dir.Name + "\\ ", directoryColor);

                if (recursive) {
                    lsHelper(dir, spaces + 1, recursive);
                }
            }

            foreach (var file in dirinfo.GetFiles()) {
                printSpaces(spaces);

                if (file.Name.EndsWith(".exe")) {
                    colorPrintln(file.Name, executableColor);
                } else {
                    colorPrintln(file.Name, fileColor);
                }
            }
        }

        #region Colors

        static void colorPrint(string text, ConsoleColor color, ConsoleColor backColor = ConsoleColor.Black) {
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = color;

            Console.Write(text);

            Console.BackgroundColor = defaultBackColor;
            Console.ForegroundColor = defaultColor;
        }

        static void colorPrintln(string text, ConsoleColor color, ConsoleColor backColor = ConsoleColor.Black) {
            colorPrint(text, color, backColor);
            Console.WriteLine();
        }

        #endregion

        #endregion

        #region Commands

        #region Without Arguments

        private static void help() {
            var lstDesc = new List<string>();

            // all help messages
            foreach (var desc in cmdHelp.Values) {

                // if we haven't got it already (no duplicates)
                if (!lstDesc.Contains(desc)) {
                    lstDesc.Add(desc);
                }
            }

            lstDesc.Sort();

            // print 'em all
            foreach (var desc in lstDesc) {
                printHelpText(desc);
            }

            lstDesc.Clear();
        }

        private static void help(string cmd) {
            if (cmdHelp.ContainsKey(cmd)) {
                printHelpText(cmdHelp[cmd]);
            } else {
                error("\"" + cmd + "\" not recognized as a command. Type h for a list of available commands.");
            }
        }

        static void ls() {
            lsHelper(pwd, 2);
        }

        static void clear() {
            try { // TODO: clean this up
                Console.Clear();
                Console.ForegroundColor = defaultColor;
                Console.BackgroundColor = defaultBackColor;
            } catch { }
        }

        static void pwdCmd() {
            colorPrintln(pwd.FullName, directoryColor);
        }

        static void exit() {
            exiting = true;
        }

        static void tree() {
            lsHelper(pwd, 0, true);
        }

        #endregion

        #region With Arguments

        static void cd(string path) {
            path = path.Trim();

            if (path == "..") {
                if (pwd.Parent != null) {
                    pwd = pwd.Parent;
                }
                
            } else {
                pwd = new DirectoryInfo(getPathDir(path));
            }

            updateTitle();
        }

        #region Run

        static void runhere(string toggle) {
            toggle = toggle.Trim().ToLower();

            if (toggle == "on" || toggle == "true") {
                Properties.Settings.Default.runhere = true;
                canRunHere = true;
            } else if (toggle == "off" || toggle == "false") {
                Properties.Settings.Default.runhere = false;
                canRunHere = false;
            } else {
                error("runhere only accepts the following arguments: on, off, true, or false");
            }

            Properties.Settings.Default.Save();
        }

        static void run(string path) {
            Process process = null;

            try {

                // args[0] -> file to run
                // args[1] -> arguments to pass
                var args = splitOptionalArgs(path);
                string filePath = getPathFile(args[0].Trim());
                string fileArgs = args[1].Trim();

                // we're just gonna let Windows handle this, skip our process logic
                if (!canRunHere) {
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
                argException(path, "run", ex);
            }

            if (process != null) {
                process.Close();
                process.Dispose();
            }
        }

        private static void process_OnOutputDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                Console.WriteLine(e.Data);
            }
        }

        private static void process_OnErrorDataReceived(object sender, DataReceivedEventArgs e) {
            if (e.Data != null) {
                error(e.Data);
            }
        }

        #endregion

        static void cat(string path) {
            try {
                using (var reader = File.OpenText(getPathFile(path))) {
                    colorPrint(reader.ReadToEnd(), defaultColor);
                }

            } catch (Exception ex) {
                argException(path, "open", ex);
            }
        }

        private static void append(string path) {
            try {
                var args = splitOptionalArgs(path);
                args[0] = Path.Combine(pwd.FullName, args[0].Trim());

                // append to the file
                using (var writer = File.AppendText(args[0])) {
                    writer.WriteLine(args[1]);
                }

            } catch (Exception ex) {
                argException(path, "be appended to", ex);
            }
        }

        private static void mk(string path) {
            try {
                var args = splitOptionalArgs(path);
                args[0]  = Path.Combine(pwd.FullName, args[0].Trim());

                // if it's not already there or they've given the go ahead to overwrite
                if (!File.Exists(args[0]) || canContinue("\"" + args[0] + "\" already exists. Do you want to overwrite it? [y/n] ")) {

                    // create the file
                    using (var writer = File.CreateText(args[0])) {
                        writer.WriteLine(args[1]); // write to it
                    }
                }

            } catch (Exception ex) {
                argException(path, "be created", ex);
            }
        }

        private static void mkdir(string path) {
            try {
                path = Path.Combine(pwd.FullName, path);

                // if it's not already there or they've given the go ahead to overwrite
                if (!Directory.Exists(path) || canContinue("\"" + path + "\" already exists. Do you want to overwrite it? [y/n] ")) {
                    Directory.CreateDirectory(path);
                }

            } catch (Exception ex) {
                argException(path, "be created", ex);
            }
        }

        private static void rm(string path) {
            try {
                path = Path.Combine(pwd.FullName, path);

                if (File.Exists(path)) {
                    if (canContinue("Are you sure you want to delete \"" + path + "\"? [y/n] ")) {
                        File.Delete(path);
                    }

                } else {
                    error("\"" + path + "\" doesn't exist.");
                }

            } catch (Exception ex) {
                argException(path, "be removed", ex);
            }
        }

        private static void rmdir(string path) {
            try {
                path = Path.Combine(pwd.FullName, path);

                if (Directory.Exists(path)) {
                    if (canContinue("Are you sure you want to delete \"" + path + "\"? [y/n] ")) {
                        Directory.Delete(path);
                    }

                } else {
                    error("\"" + path + "\" doesn't exist.");
                }

            } catch (Exception ex) {
                argException(path, "be removed", ex);
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
