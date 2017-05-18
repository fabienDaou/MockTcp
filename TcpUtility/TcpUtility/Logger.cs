using System;

namespace TcpUtility
{
    public class Logger
    {
        public static void Log(string message, LogLevel logLevel)
        {
            Console.WriteLine($"|{logLevel}|{message}");
        }
    }
}
