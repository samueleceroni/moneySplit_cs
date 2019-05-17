using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Logger
{
    public class SingletonLogger : TraceSource
    {
        private static SingletonLogger istance;
        public static readonly string LoggerFilePath = @"log.txt";
        public static readonly string LoggerName = "TelegramBot.Logger";

        public SingletonLogger(string name) : base(name)
        {
            this.Listeners.Clear();
            TextWriterTraceListener txt = new TextWriterTraceListener(LoggerFilePath);
            this.Listeners.Add(txt);
            this.Switch = new SourceSwitch("switch");
            this.Switch.Level = SourceLevels.All;
            Trace.AutoFlush = true;
        }

        public static SingletonLogger Istance
        {
            get => istance == null ? istance = new SingletonLogger(LoggerName) : istance;
        }
    }
}
