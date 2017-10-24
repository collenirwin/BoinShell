using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BoinShell
{
    public static class CancelOps
    {
        public static bool cancelled { get; private set; }

        /// <summary>
        /// Cancels any running method
        /// </summary>
        public static void cancel()
        {
            cancelled = true;
        }

        /// <summary>
        /// Prints all files and directories in the specified DirectoryInfo
        /// </summary>
        /// <param name="dirinfo">Directory to look in</param>
        /// <param name="spaces">Number of spaces to prepend before we print the file/dir names</param>
        /// <param name="recursive">Go through all directories within this one?</param>
        public static void lsHelper(DirectoryInfo dirinfo, int spaces = 0, bool recursive = false)
        {
            if (cancelled)
            {
                opCancelled();
                return;
            }

            try
            {
                foreach (var dir in dirinfo.GetDirectories())
                {
                    Program.printSpaces(spaces);

                    try
                    {
                        Program.colorPrintln(dir.Name + "\\ ", Program.DIRECTORY_COLOR);

                        if (recursive)
                        {
                            lsHelper(dir, spaces + 1, recursive);
                        }

                    }
                    catch
                    {
                        Program.error("[Directory] Access Denied");
                    }
                }

                foreach (var file in dirinfo.GetFiles())
                {
                    Program.printSpaces(spaces);

                    try
                    {
                        if (file.Name.EndsWith(".exe"))
                        {
                            Program.colorPrintln(file.Name, Program.EXECUTABLE_COLOR);
                        }
                        else
                        {
                            Program.colorPrintln(file.Name, Program.FILE_COLOR);
                        }

                    }
                    catch
                    {
                        Program.error("[File] Access Denied");
                    }
                }

            }
            catch
            {
                Program.error("Access Denied");
            }
        }

        private static void opCancelled()
        {
            cancelled = false;
        }
    }
}
