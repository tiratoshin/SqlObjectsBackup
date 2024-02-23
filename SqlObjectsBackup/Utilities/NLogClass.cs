using System;
using System.IO;
using NLog;
using NLog.Config;

namespace SqlObjectsBackup
{
    public class NLogClass
    {
        public static void ConfigureNLog(string logDirPath)
        {
            // Generate the log filename using the current date and time
            string logFileName = $"app_{DateTime.Now:yyyyMMdd_HHmmss}.log";
            string fullLogFilePath = Path.Combine(logDirPath, logFileName);

            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = fullLogFilePath };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }
    }
}