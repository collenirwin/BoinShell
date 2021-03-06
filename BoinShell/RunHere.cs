﻿using System;

namespace BoinShell
{
    public class RunHere : Command
    {
        public RunHere() : base(new string[] { "runhere" }, "determines whether the run command should launch programs within BoinShell (ex: runhere off)") { }

        public override void run()
        {
            Console.WriteLine("Running here: " + Program.canRunHere.ToString());
        }

        public override void run(string arg)
        {
            arg = arg.Trim().ToLower();

            if (arg == "on" || arg == "true")
            {
                Properties.Settings.Default.runhere = true;
                Program.canRunHere = true;
            }
            else if (arg == "off" || arg == "false")
            {
                Properties.Settings.Default.runhere = false;
                Program.canRunHere = false;
            }
            else
            {
                Program.error("runhere only accepts the following arguments: on, off, true, or false");
            }

            Properties.Settings.Default.Save();
        }
    }
}
