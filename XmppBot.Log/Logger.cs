using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmppBot.Log
{
    public class Logger
    {
        public static void Log(string message)
        {
            using (var writer = new StreamWriter($@"{Properties.Log.Default.LogPath}",true))
            {
                writer.WriteLine($"{message}. Logged at {DateTime.Now}");
            }
        }
    }
}
