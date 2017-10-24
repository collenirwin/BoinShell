using System.IO;

namespace BoinShell
{
    public class AsyncHelper
    {
        public AsyncCommand cmd { get; private set; }

        public AsyncHelper(AsyncCommand cmd)
        {
            this.cmd = cmd;
        }

        /// <summary>
        /// Prints all files and directories in the specified DirectoryInfo,
        /// stops if cmd.cancelled.
        /// </summary>
        /// <param name="dirinfo">Directory to look in</param>
        /// <param name="spaces">Number of spaces to prepend before we print the file/dir names</param>
        /// <param name="recursive">Go through all directories within this one?</param>
        public void lsHelper(DirectoryInfo dirinfo, int spaces = 0, bool recursive = false)
        {
            // stop if cmd has been cancelled
            if (cmd.cancelled)
            {
                return;
            }

            try
            {
                foreach (var dir in dirinfo.GetDirectories())
                {
                    // stop if cmd has been cancelled
                    if (cmd.cancelled)
                    {
                        return;
                    }

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
                    // stop if cmd has been cancelled
                    if (cmd.cancelled)
                    {
                        return;
                    }
                    
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
    }
}
