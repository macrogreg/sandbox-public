using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ActivityListeningTest01
{
    class Program
    {
        static void Main(string[] args)
        {
            // (new FireAndReceiveActivities01()).Exec();
            (new ActivityContextCreation()).Exec();
        }
    }
}
