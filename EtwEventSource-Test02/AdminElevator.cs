using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace EtwEventSource_Test02
{
    public static class AdminElevator
    {
        private static bool s_doNotEnforceAdmin = false;
        private static readonly int s_thisProcessId = Process.GetCurrentProcess().Id;

        public static class Settings
        {
            public static bool SkipAdminEnforcement
            {
                get { return s_doNotEnforceAdmin; }
                set { s_doNotEnforceAdmin = value; } 
            }
        }

        public static int ThisProcessId
        {
            get { return s_thisProcessId; }
        }

        public static bool GetCurrentProcessIsRunAsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);

            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool EnsureAdminOrForkProcess()
        {
            bool isAdmin = GetCurrentProcessIsRunAsAdmin();
            if (isAdmin)
            {
                Console.WriteLine($"This application (process #{ThisProcessId}) is running with with administrator priviledges.");
                return true;
            }

            Console.WriteLine($"This application (process #{ThisProcessId}) is NOT running with with administrator priviledges.");

            if (Settings.SkipAdminEnforcement)
            {
                Console.WriteLine("Administrator priviledges are not required. Continuing.");
                return true;
            }

            Console.WriteLine("This program must be run as an administrator!");
            Console.WriteLine("Relaunching...");
            Console.WriteLine();

            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            process.StartInfo.FileName = Path.ChangeExtension(Assembly.GetEntryAssembly().CodeBase, "exe");
            process.StartInfo.Verb = "runas";

            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }
    }
}
