using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BoinShell {
    class Program {

        #region Vars

        public static bool exiting    = false;
        public static bool canRunHere = true;

        static string prompt = "> ";

        public static Dictionary<string, Command> cmds = new Dictionary<string, Command>();
        public static DirectoryInfo pwd = new DirectoryInfo(Directory.GetCurrentDirectory());

        public static List<string> backTrace = new List<string>();

        #region Colors

        public const ConsoleColor defaultColor     = ConsoleColor.Gray;
        public const ConsoleColor defaultBackColor = ConsoleColor.Black;
        public const ConsoleColor directoryColor   = ConsoleColor.DarkGray;
        public const ConsoleColor fileColor        = ConsoleColor.Gray;
        public const ConsoleColor executableColor  = ConsoleColor.Cyan;
        public const ConsoleColor errorColor       = ConsoleColor.Red;
        public const ConsoleColor commandColor     = ConsoleColor.Yellow;

        #endregion

        #endregion

        #region Methods

        static void Main(string[] args) {

            // starting up

            loadCmds();

            canRunHere = Properties.Settings.Default.runhere;

            // if we're given a valid path in args, switch pwd to that path
            if (args.Length != 0 && Directory.Exists(args[0])) {
                cmds["cd"].run(args[0]);
            } else {
                cmds["cd"].run(pwd.FullName);
            }

            clear();

            Console.Write("Welcome to ");
            colorPrint("BoinShell", ConsoleColor.Green);
            Console.WriteLine(". Type h for a list of commands.");

            // done starting up

            while (true) {
                printPrompt();

                string cmd = TabComplete.readLine(getTabCompleteList(pwd), Console.CursorLeft).Trim().ToLower();

                // if we have a valid command
                if (cmd.Length != 0) {

                    // check if there are args
                    if (cmd.Contains(" ")) {
                        var cmdArgs = cmd.Split(' ');
                        if (cmdArgs.Length > 1 && cmds.ContainsKey(cmdArgs[0])) {

                            string restOfArgs = "";

                            // append the rest of the args given into one big arg, so we can pass it to the cmd to handle it (or them?)
                            for (int x = 1; x < cmdArgs.Length; x++) {
                                restOfArgs += " " + cmdArgs[x];
                            }

                            // run command with argument(s)
                            cmds[cmdArgs[0]].run(restOfArgs.Trim());
                        } else {
                            cmds["run"].run(cmd);
                        }

                    // no args
                    } else if (cmds.ContainsKey(cmd)) {
                        cmds[cmd].run();

                    // just try to run it as a program/file
                    } else {
                        cmds["run"].run(cmd);
                    }
                }

                if (exiting) {
                    break;
                }
            }
        }

        static void loadCmds() {
            
            var lstCmd = new List<Command>() {
                new Append(),
                new Cat(),
                new Cd(),
                new Clear(),
                new Help(),
                new Ls(),
                new LaunchAll(),
                new Pwd(),
                new Exit(),
                new Run(),
                new RunHere(),
                new Tree(),
                new Mk(),
                new MkDir(),
                new Rm(),
                new RmDir(),
                new BackTrace(),
                new History()
            };

            foreach (var cmd in lstCmd) {
                foreach (var alias in cmd.aliases) {
                    cmds.Add(alias, cmd);
                }
            }

            lstCmd.Clear();
        }

        #region Helpers

        /// <summary>
        /// Combines the pwd path with the specified path and returns it
        /// </summary>
        public static string combinePathPwd(string path) {
            return Path.Combine(pwd.FullName, path);
        }

        /// <summary>
        /// gets a list of all file and directory names in pwd, will not break for failures
        /// </summary>
        public static List<string> getTabCompleteList(DirectoryInfo dir) {
            var list = new List<string>();

            try {
                foreach (var subdir in dir.GetDirectories()) {
                    list.Add(subdir.Name);
                }
            } catch { }

            try {
                foreach (var file in dir.GetFiles()) {
                    list.Add(file.Name);
                }
            } catch { }

            foreach (var key in cmds.Keys) {
                list.Add(key);
            }

            list.Sort();
            return list;
        }

        /// <summary>
        /// Sets the titlebar (format: BoinShell - pwd)
        /// </summary>
        public static void updateTitle() {
            Console.Title = "BoinShell - " + pwd.FullName;
        }

        /// <summary>
        /// Prints BoinShell's default prompt (pwdname>) in the correct colors
        /// </summary>
        public static void printPrompt() {
            colorPrint(pwd.Name, directoryColor);
            colorPrint(prompt, defaultColor);
        }

        /// <summary>
        /// Prints the specified error message in the correct error color (optionally with newline appended)
        /// </summary>
        public static void error(string message, bool newline = true) {
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
        public static void argException(string arg, string action, Exception ex) {
            error(
                "\"" + arg + "\" failed to " + action + " with the following exception:" +
                Environment.NewLine +
                ex.Message
            );
        }

        /// <summary>
        /// If the path does not exist, it returns the path argument
        /// </summary>
        public static string getDirPathIfExists(string path) {
            string dir = pwd.FullName;

            if (Directory.Exists(path)) {
                dir = path;
            } else {
                string pathCombined = Program.combinePathPwd(path);

                if (Directory.Exists(pathCombined)) {
                    dir = pathCombined;
                }
            }

            return dir;
        }

        /// <summary>
        /// If the path does not exist, it returns the path argument
        /// </summary>
        public static string getFilePathIfExists(string path) {
            string pathCombined = Program.combinePathPwd(path);

            if (File.Exists(pathCombined)) {
                return pathCombined;
            }

            return path;
        }

        /// <summary>
        /// Splits the specified string into 2 strings, separated by the first space, 
        /// returns the array of string
        /// </summary>
        public static string[] splitOptionalArgs(string text) {

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

        /// <summary>
        /// Prints the prompt and returns true if the user typed "y"
        /// </summary>
        public static bool canContinue(string prompt) {
            Console.Write(prompt);
            return (Console.ReadLine().Trim().ToLower() == "y");
        }

        /// <summary>
        /// Prints the help message for the specified command
        /// </summary>
        public static void printHelpText(Command cmd) {

            // print all but the last alias of cmd, separated by commas
            for (int x = 0; x < cmd.aliases.Length - 1; x++) {
                colorPrint(cmd.aliases[x], commandColor);
                Console.Write(", ");
            }

            // print the last alias seperately so it doesn't have a "," after it
            colorPrint(cmd.aliases[cmd.aliases.Length - 1], commandColor);
            Console.WriteLine(" - " + cmd.helpText);
        }

        /// <summary>
        /// Prints the specified number of spaces
        /// </summary>
        public static void printSpaces(int count) {
            for (int x = 0; x < count; x++) {
                Console.Write(' ');
            }
        }

        /// <summary>
        /// Prints all files and directories in the specified DirectoryInfo
        /// </summary>
        /// <param name="dirinfo">Directory to look in</param>
        /// <param name="spaces">Number of spaces to prepend before we print the file/dir names</param>
        /// <param name="recursive">Go through all directories within this one?</param>
        public static void lsHelper(DirectoryInfo dirinfo, int spaces = 0, bool recursive = false) {
            try {
                foreach (var dir in dirinfo.GetDirectories()) {
                    printSpaces(spaces);

                    try {
                        colorPrintln(dir.Name + "\\ ", directoryColor);

                        if (recursive) {
                            lsHelper(dir, spaces + 1, recursive);
                        }

                    } catch {
                        error("[Directory] Access Denied");
                    }
                }

                foreach (var file in dirinfo.GetFiles()) {
                    printSpaces(spaces);

                    try {
                        if (file.Name.EndsWith(".exe")) {
                            colorPrintln(file.Name, executableColor);
                        } else {
                            colorPrintln(file.Name, fileColor);
                        }

                    } catch {
                        error("[File] Access Denied");
                    }
                }

            } catch {
                error("Access Denied");
            }
        }

        /// <summary>
        /// Clears the console window
        /// </summary>
        public static void clear() {
            try { // TODO: clean this up
                Console.Clear();
                Console.ForegroundColor = defaultColor;
                Console.BackgroundColor = defaultBackColor;
            } catch { }
        }

        #region Colors

        /// <summary>
        /// Prints the specified text with the specified colors
        /// </summary>
        /// <param name="text">String to print</param>
        /// <param name="color">Forecolor</param>
        /// <param name="backColor">Background color</param>
        public static void colorPrint(string text, ConsoleColor color, ConsoleColor backColor = ConsoleColor.Black) {
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = color;

            Console.Write(text);

            Console.BackgroundColor = defaultBackColor;
            Console.ForegroundColor = defaultColor;
        }

        /// <summary>
        /// Prints the specified text with the specified colors with a newline appended
        /// </summary>
        /// <param name="text">String to print</param>
        /// <param name="color">Forecolor</param>
        /// <param name="backColor">Background color</param>
        public static void colorPrintln(string text, ConsoleColor color, ConsoleColor backColor = ConsoleColor.Black) {
            colorPrint(text, color, backColor);
            Console.WriteLine();
        }

        #endregion

        #endregion

        #endregion
    }
}
