﻿using System;

namespace CommunityPatch
{
    internal class Logger
    {
        private enum LogLevel : byte
        {
            TRACE = 0,
            DEBUG = 1,
            INFO = 2,
            WARN = 3,
            ERROR = 4,
            FATAL = 5,
            OFF = 6
        }

        internal struct TargetConfig
        {
            public string filename;
            public string path;
            public string layout;
            public bool keepOldFiles;

            internal TargetConfig(string filename = null, string path = null, string layout = null, bool keepOldFiles = false)
            {
                this.filename = filename;
                this.path = path;
                this.layout = layout;
                this.keepOldFiles = keepOldFiles;
            }
        }

        public object logger;
        public readonly byte minLoggingLevel;
        public readonly string filename = null;
        public readonly string loggerID;
        public readonly string path = null;
        public readonly string layout = null;
        public readonly bool keepOldFiles = false;

        internal string logPath = "";

        internal Logger(string loggerID, TargetConfig config = default, byte defaultLogLevel = (byte)LogLevel.ERROR)
        {
            this.loggerID = loggerID;
            this.minLoggingLevel = defaultLogLevel;

            // Setup targeting configs
            this.filename = config.filename;
            this.path = config.path;
            this.layout = config.layout;
            this.keepOldFiles = config.keepOldFiles;

            // Read in configured logging level
            string loggingLevelStr = null;
            string[] commandLineArgs = CommandLineReader.GetCommandLineArgs();
            for (int i = 0; i < commandLineArgs.Length; i++)
            {
                if (commandLineArgs[i] == "+log_level" && i < commandLineArgs.Length - 1)
                {
                    if (loggingLevelStr == null)
                    {
                        Console.WriteLine($"[{loggerID}] General log level of {commandLineArgs[i + 1]} read");
                        loggingLevelStr = commandLineArgs[i + 1];
                    }
                }
                else if (commandLineArgs[i] == $"+log_level_{loggerID}" && i < commandLineArgs.Length - 1)
                {
                    string overrideStatement = loggingLevelStr == null ? "" : $", overriding general log level of {loggingLevelStr}";
                    Console.WriteLine($"[{loggerID}] Custom log level of {commandLineArgs[i + 1]} read{overrideStatement}");
                    loggingLevelStr = commandLineArgs[i + 1];
                }
            }

            // Assign the correct level to the logger
            try
            {
                if (loggingLevelStr != null)
                {
                    LogLevel loggingLevel = (LogLevel)Enum.Parse(typeof(LogLevel), loggingLevelStr, true);
                    this.minLoggingLevel = (byte)loggingLevel;
                    Console.WriteLine($"[{loggerID}] Logging {loggingLevel} and up");
                }
                else
                {
                    Console.WriteLine($"[{loggerID}] No log level found. Defaulting to {this.minLoggingLevel}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing log level of {loggingLevelStr}");
                Console.WriteLine(ex.ToString());
                Console.WriteLine($"[{loggerID}] {loggingLevelStr} is unrecognized logging level. Defaulting to {this.minLoggingLevel}");
            }

            // Perform any injected setup
            try
            {
                // Perform any injected setup
                this.Setup();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up loger");
                Console.WriteLine(ex.ToString());
            }
            this.Debug("Logger initialized");
        }

        public void Setup()
        {
            this.Trace("Setup Hook called");
        }

        private void Log(byte level, string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")} | {(LogLevel)level} | {loggerID} | {message}");
        }

        private void LogException(byte level, Exception exception)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")} | {(LogLevel)level} | {loggerID} | {exception.ToString()}");
        }

        private void LogException(byte level, Exception exception, string message)
        {
            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff")} | {(LogLevel)level} | {loggerID} | {message}:\n {exception.ToString()}");
        }

        internal void Trace(string message)
        {
            if (minLoggingLevel <= (byte)LogLevel.TRACE)
            {
                Log((byte)LogLevel.TRACE, message);
            }
        }

        internal void Debug(string message)
        {
            if (minLoggingLevel <= (byte)LogLevel.DEBUG)
            {
                Log((byte)LogLevel.DEBUG, message);
            }
        }

        internal void Info(string message)
        {
            if (minLoggingLevel <= (byte)LogLevel.INFO)
            {
                Log((byte)LogLevel.INFO, message);
            }
        }

        internal void Fatal(string message)
        {
            if (minLoggingLevel <= (byte)LogLevel.FATAL)
            {
                Log((byte)LogLevel.FATAL, message);
            }
        }

        internal void Fatal(Exception exception, string message = null)
        {
            if (minLoggingLevel <= (byte)LogLevel.FATAL)
            {
                if (message != null)
                {
                    LogException((byte)LogLevel.FATAL, exception, message);
                }
                else
                {
                    LogException((byte)LogLevel.FATAL, exception);
                }
            }
        }

        internal void Error(string message)
        {
            if (minLoggingLevel <= (byte)LogLevel.ERROR)
            {
                Log((byte)LogLevel.ERROR, message);
            }
        }

        internal void Error(Exception exception, string message = null)
        {
            if (minLoggingLevel <= (byte)LogLevel.ERROR)
            {
                if (message != null)
                {
                    LogException((byte)LogLevel.ERROR, exception, message);
                }
                else
                {
                    LogException((byte)LogLevel.ERROR, exception);
                }
            }
        }

        internal void Warn(string message)
        {
            if (minLoggingLevel <= (byte)LogLevel.WARN)
            {
                Log((byte)LogLevel.WARN, message);
            }
        }
    }
}
