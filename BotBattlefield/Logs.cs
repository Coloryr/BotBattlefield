using System;
using System.IO;

namespace ColoryrSDK
{
    public class Logs
    {
        public string log = "logs.log";
        private object obj = new object();
        public string Dir { get; init; }

        public void LogWrite(string a)
        {
            try
            {
                lock (obj)
                {
                    DateTime date = DateTime.Now;
                    string year = date.ToShortDateString().ToString();
                    string time = date.ToLongTimeString().ToString();
                    string write = "[" + year + "]" + "[" + time + "]" + a;
                    File.AppendAllText(Dir + log, write + Environment.NewLine);
                    Console.WriteLine(write);
                }
            }
            catch
            {

            }
        }

        public void LogError(Exception e)
        {
            LogWrite("[ERROR]" + e.Message + "\n" + e.StackTrace);
        }

        public void LogError(string e)
        {
            LogWrite("[ERROR]" + e);
        }
    }
}
