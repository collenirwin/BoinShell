using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BoinShell
{
    class Program
    {

        #region Vars

        static ManualResetEvent mre = new ManualResetEvent(false);

        public static bool exiting = false;
        public static bool canRunHere = true;

        static string prompt = "> ";

        public static Dictionary<string, Command> cmds = new Dictionary<string, Command>();
        public static DirectoryInfo pwd = new DirectoryInfo(Directory.GetCurrentDirectory());

        public static List<string> backTrace = new List<string>();

        #region Colors

        public const ConsoleColor DEFAULT_COLOR = ConsoleColor.Gray;
        public const ConsoleColor DEFAULT_BACK_COLOR = ConsoleColor.Black;
        public const ConsoleColor DIRECTORY_COLOR = ConsoleColor.DarkGray;
        public const ConsoleColor FILE_COLOR = ConsoleColor.Gray;
        public const ConsoleColor EXECUTABLE_COLOR = ConsoleColor.Cyan;
        public const ConsoleColor ERROR_COLOR = ConsoleColor.Red;
        public const ConsoleColor COMMAND_COLOR = ConsoleColor.Yellow;

        #endregion

        #endregion

        #region Methods

        static void Main(string[] args)
        {
            // starting up

            loadCmds();

            canRunHere = Properties.Settings.Default.runhere;

            // if we're given a valid path in args, switch pwd to that path
            if (args.Length != 0 && Directory.Exists(args[0]))
            {
                cmds["cd"].run(args[0]);
            }
            else
            {
                cmds["cd"].run(pwd.FullName);
            }

            // register Ctrl+C-pressed event
            Console.CancelKeyPress += (sender, e) =>
            {
                // don't exit
                e.Cancel = true;

                //mre.Set();

                // kill current running process if one exists
                (cmds["run"] as Run).kill();

                var runningCmds = new List<AsyncCommand>();

                // cancel all AsyncCommands
                foreach (var cmd in cmds.Values)
                {
                    if (cmd is AsyncCommand)
                    {
                        (cmd as AsyncCommand).cancel();
                    }
                }

                colorPrintln("^C", ERROR_COLOR);

                if (Console.CursorLeft == 0)
                {
                    printPrompt();
                } 
                //else if (Console.CursorLeft != prompt.Length)
                //{
                //    Console.WriteLine();
                //    printPrompt();
                //}
            };

            clear();

            Console.Write("Welcome to ");
            colorPrint("BoinShell", ConsoleColor.Green);
            Console.WriteLine(". Type h for a list of commands.");

            // done starting up

            takeInput();
        }

        public static void takeInput()
        {
            printPrompt();

            string cmd = TabComplete.readLine(getTabCompleteList(pwd), Console.CursorLeft).Trim().ToLower();

            // if we have a valid command
            if (cmd.Length != 0)
            {
                // check if there are args
                if (cmd.Contains(" "))
                {
                    var cmdArgs = cmd.Split(' ');
                    if (cmdArgs.Length > 1 && cmds.ContainsKey(cmdArgs[0]))
                    {
                        string restOfArgs = "";

                        // append the rest of the args given into one big arg, 
                        // so we can pass it to the cmd to handle it (or them?)
                        for (int x = 1; x < cmdArgs.Length; x++)
                        {
                            restOfArgs += " " + cmdArgs[x];
                        }

                        // run command with argument(s)
                        runCmd(cmdArgs[0], restOfArgs.Trim(), takeInput);
                    }
                    else // cmd doesn't exist, try to run the input
                    {
                        runCmd("run", cmd, takeInput);
                    }
                }
                else if (cmds.ContainsKey(cmd)) // no args
                {
                    runCmd(cmd, null, takeInput);
                }
                else // just try to run it as a program/file
                {
                    runCmd("run", cmd, takeInput);
                }
            }
            else // invalid command, prompt again
            {
                takeInput();
            }
        }

        static void runCmd(string cmd, string arg = null, Action callback = null)
        {
            if (arg == null)
            {
                cmds[cmd].run(callback);
            }
            else
            {
                cmds[cmd].run(arg, callback);
            }

            if (!exiting)
            {
                // wait for thread to finish
                mre.WaitOne();
            }
        }

        static void loadCmds()
        {
            var lstCmd = new List<Command>()
            {
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

            foreach (var cmd in lstCmd)
            {
                foreach (var alias in cmd.aliases)
                {
                    cmds.Add(alias, cmd);
                }
            }

            lstCmd.Clear();
        }

        #region Helpers

        /// <summary>
        /// Combines the pwd path with the specified path and returns it
        /// </summary>
        public static string combinePathPwd(string path)
        {
            return Path.Combine(pwd.FullName, path);
        }

        /// <summary>
        /// gets a list of all file and directory names in pwd, will not break for failures
        /// </summary>
        public static List<string> getTabCompleteList(DirectoryInfo dir)
        {
            var list = new List<string>();

            try
            {
                foreach (var subdir in dir.GetDirectories())
                {
                    list.Add(subdir.Name);
                }
            }
            catch { }

            try
            {
                foreach (var file in dir.GetFiles())
                {
                    list.Add(file.Name);
                }
            }
            catch { }

            foreach (var key in cmds.Keys)
            {
                list.Add(key);
            }

            list.Sort();
            return list;
        }

        /// <summary>
        /// Sets the titlebar (format: BoinShell - pwd)
        /// </summary>
        public static void updateTitle()
        {
            Console.Title = "BoinShell - " + pwd.FullName;
        }

        /// <summary>
        /// Prints BoinShell's default prompt (pwdname>) in the correct colors
        /// </summary>
        public static void printPrompt()
        {
            colorPrint(pwd.Name, DIRECTORY_COLOR);
            colorPrint(prompt, DEFAULT_COLOR);
        }

        /// <summary>
        /// Prints the specified error message in the correct error color (optionally with newline appended)
        /// </summary>
        public static void error(string message, bool newline = true)
        {
            if (newline)
            {
                colorPrintln(message, ERROR_COLOR);
            }
            else
            {
                colorPrint(message, ERROR_COLOR);
            }
        }

        /// <summary>
        /// output:
        /// "ARG" failed to ACTION with the following exception:
        /// EX.Message
        /// </summary>
        public static void argException(string arg, string action, Exception ex)
        {
            error(
                "\"" + arg + "\" failed to " + action + " with the following exception:" +
                Environment.NewLine +
                ex.Message
            );
        }

        /// <summary>
        /// If the path does not exist, it returns the path argument
        /// </summary>
        public static string getDirPathIfExists(string path)
        {
            string dir = pwd.FullName;

            if (Directory.Exists(path))
            {
                dir = path;
            }
            else
            {
                string pathCombined = Program.combinePathPwd(path);

                if (Directory.Exists(pathCombined))
                {
                    dir = pathCombined;
                }
            }

            return dir;
        }

        /// <summary>
        /// If the path does not exist, it returns the path argument
        /// </summary>
        public static string getFilePathIfExists(string path)
        {
            string pathCombined = Program.combinePathPwd(path);

            if (File.Exists(pathCombined))
            {
                return pathCombined;
            }

            return path;
        }

        /// <summary>
        /// Splits the specified string into 2 strings, separated by the first space, 
        /// returns the array of string
        /// </summary>
        public static string[] splitOptionalArgs(string text)
        {
            // default to the original command and an empty string for the optional arg
            string[] args = { text, "" };
            int firstSpace = text.IndexOf(' ');

            // we've got an argument!
            if (firstSpace != -1)
            {
                args[0] = text.Substring(0, firstSpace);  // first word
                args[1] = text.Substring(firstSpace + 1); // the rest
            }

            return args;
        }

        /// <summary>
        /// Prints the prompt and returns true if the user typed "y"
        /// </summary>
        public static bool canContinue(string prompt)
        {
            Console.Write(prompt);
            return (Console.ReadLine().Trim().ToLower() == "y");
        }

        /// <summary>
        /// Prints the help message for the specified command
        /// </summary>
        public static void printHelpText(Command cmd)
        {
            // print all but the last alias of cmd, separated by commas
            for (int x = 0; x < cmd.aliases.Length - 1; x++)
            {
                colorPrint(cmd.aliases[x], COMMAND_COLOR);
                Console.Write(", ");
            }

            // print the last alias seperately so it doesn't have a "," after it
            colorPrint(cmd.aliases[cmd.aliases.Length - 1], COMMAND_COLOR);
            Console.WriteLine(" - " + cmd.helpText);
        }

        /// <summary>
        /// Prints the specified number of spaces
        /// </summary>
        public static void printSpaces(int count)
        {
            for (int x = 0; x < count; x++)
            {
                Console.Write(' ');
            }
        }

        /// <summary>
        /// Clears the console window
        /// </summary>
        public static void clear()
        {
            try
            { // TODO: clean this up
                Console.Clear();
                Console.ForegroundColor = DEFAULT_COLOR;
                Console.BackgroundColor = DEFAULT_BACK_COLOR;
            }
            catch { }
        }

        #region Colors

        /// <summary>
        /// Prints the specified text with the specified colors
        /// </summary>
        /// <param name="text">String to print</param>
        /// <param name="color">Forecolor</param>
        /// <param name="backColor">Background color</param>
        public static void colorPrint(string text, ConsoleColor color, ConsoleColor backColor = DEFAULT_BACK_COLOR)
        {
            // change to passed colors
            Console.BackgroundColor = backColor;
            Console.ForegroundColor = color;

            // write the text
            Console.Write(text);

            // change back to the default colors
            Console.BackgroundColor = DEFAULT_BACK_COLOR;
            Console.ForegroundColor = DEFAULT_COLOR;
        }

        /// <summary>
        /// Prints the specified text with the specified colors with a newline appended
        /// </summary>
        /// <param name="text">String to print</param>
        /// <param name="color">Forecolor</param>
        /// <param name="backColor">Background color</param>
        public static void colorPrintln(string text, ConsoleColor color, ConsoleColor backColor = DEFAULT_BACK_COLOR)
        {
            colorPrint(text, color, backColor);
            Console.WriteLine();
        }

        #endregion

        #endregion

        #endregion
    }
}
