using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace BoinShell {
    class Program {

        #region Vars

        static bool startup = true;
        static bool exiting = false;

        static string prompt = "> ";

        static Dictionary<string, Action> cmds = new Dictionary<string, Action>();
        static Dictionary<string, Action<string>> cmdsWithArgs = new Dictionary<string, Action<string>>();

        static DirectoryInfo pwd = new DirectoryInfo(Directory.GetCurrentDirectory());
        static Uri pwdURI        = new Uri(pwd.FullName + Path.DirectorySeparatorChar);

        #region Colors

        const ConsoleColor defaultColor     = ConsoleColor.Gray;
        const ConsoleColor defaultBackColor = ConsoleColor.Black;
        const ConsoleColor directoryColor   = ConsoleColor.DarkGray;
        const ConsoleColor fileColor        = ConsoleColor.Gray;
        const ConsoleColor executableColor  = ConsoleColor.Cyan;
        const ConsoleColor errorColor       = ConsoleColor.Red;

        #endregion

        #endregion

        static void Main(string[] args) {
            if (startup) {
                loadCmds();

                // if we're given a valid path in args, switch pwd to that path
                if (args.Length != 0 && Directory.Exists(args[0])) {
                    pwd = new DirectoryInfo(args[0]);
                    pwdURI = new Uri(pwd.FullName + Path.DirectorySeparatorChar);
                }

                clear();

                Console.Write("Welcome to ");
                colorPrint("BoinShell", ConsoleColor.Green);
                Console.WriteLine(", your current directory is");
                colorPrintln(" " + pwd.FullName, ConsoleColor.DarkGray);

                startup = false;
            }

            printPrompt();

            string cmd = Console.ReadLine().Trim().ToLower();

            // if we have a valid command
            if (cmd.Length != 0) {

                // check if there are args
                if (cmd.Contains(" ")) {
                    var cmdArgs = cmd.Split(' ');
                    if (cmdArgs.Length > 1 && cmdsWithArgs.ContainsKey(cmdArgs[0])) {

                        // run command with argument
                        cmdsWithArgs[cmdArgs[0]].Invoke(cmdArgs[1]);
                    } else {
                        colorPrintln("\"" + cmd + "\" could not be recognized as a command", ConsoleColor.Red);
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

        static void printPrompt() {
            colorPrint(pwd.Name, directoryColor);
            colorPrint(prompt, defaultColor);
        }

        static void loadCmds() {
            cmds.Add("ls",  ls);
            cmds.Add("dir", ls);
            cmds.Add("clear", clear);
            cmds.Add("cls",   clear);
            cmds.Add("exit", exit);

            cmdsWithArgs.Add("cd", cd);
            cmdsWithArgs.Add("run", run);
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

        #region Commands

        static void ls() {
            foreach (var dir in pwd.GetDirectories()) {
                Console.Write(" ");
                colorPrintln(" " + dir.Name + "\\ ", directoryColor);
            }

            foreach (var file in pwd.GetFiles()) {
                if (file.Name.EndsWith(".exe")) {
                    colorPrintln("  " + file.Name, executableColor);
                } else {
                    colorPrintln("  " + file.Name, fileColor);
                }
            }
        }

        static void clear() {
            Console.Clear();
            Console.ForegroundColor = defaultColor;
            Console.BackgroundColor = defaultBackColor;
        }

        static void exit() {
            exiting = true;
        }

        static void cd(string path) {
            path = path.Trim();

            if (path == "..") {
                if (pwd.Parent != null) {
                    pwd = pwd.Parent;
                }
                
            } else {
                pwd = new DirectoryInfo(getPathDir(path));
            }
        }

        static void run(string path) {
            try {
                Process.Start(getPathFile(path.Trim()));
            } catch (Exception ex) {
                colorPrintln("\"" + path + "\" failed to run with the following exception:" +
                    Environment.NewLine +
                    ex.Message,
                    errorColor
                );
            }
        }

        #endregion
    }
}
