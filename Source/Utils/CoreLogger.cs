using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Merthsoft.DesignatorShapes.Utils
{
    public static class CoreLogger
    {
        public static void Log(string message)
        {
            Verse.Log.Message($"{Timestamp} Designator shapes 1.4 : {message}");
        }

        public static void LogStartAndEnd(Action action, string actionName)
        {
            Log(actionName + " Start");
            action();
            Log(actionName + " End");
        }
        
        public static T LogStartAndEnd<T>(Func<T> action, string actionName)
        {
            Log(actionName + " Start");
            var output = action();
            Log(actionName + " End");
            return output;
        }

        public static string Timestamp
        {
            get
            {
                return $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}]";
            }
        }

        static bool logLow = false;
        internal static void LogStartAndEndLow(Action p, string v)
        {
            if (logLow)
                LogStartAndEnd(p, v);
            else
                p();
        }
        internal static T LogStartAndEndLow<T>(Func<T> p, string v)
        {
            if (logLow)
                return LogStartAndEnd(p, v);

            return p();
        }
    }
}
