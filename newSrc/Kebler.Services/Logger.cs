using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Kebler.Domain;
using Kebler.Domain.Interfaces;

namespace Kebler.Services
{
    /// <summary>
    /// I DON'T WANT TO USE FUCKING NLOG OR SIMILAR SHIT. THAT IS PEACE OF SHITY WIZZARD
    /// </summary>
    public class Logger : ILogger
    {
        private readonly IConfigService _configService = ConfigService.Instance;
        private static readonly string FileName = $"{nameof(Kebler)}_{DateTime.Now:dd-MM-yyyy}.log";
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstStrings.LogFolder, FileName);
        private static readonly object WriteLock = new();
        private static ILogger _logger;
        private enum LogType { Info, Warning, Error, Ui, Trace }

        //200MB
        private const long MaxLogFileSizeBytes = 209_715_200;

        public static ILogger Instance => _logger ??= new Logger();

        public static readonly FileInfo LogFileInfo = new(FilePath);

        public Logger()
        {
            Task.Run(ClearOldLogs);
        }

        public void Info(in string message, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "")
        {
            var data = Format(message, LogType.Info, lineNumber, caller, GetClassName(sourceFilePath));

            Task.Run(() =>
            {
                WriteToFile(data);
            });
        }

        public void Trace(in Stopwatch time, in int lineNumber = 0, in string caller = "", in string sourceFilePath = "")
        {
            if (ConfigService.DefaultSettingsInstanse.TraceEnabled)
            {
                var data = Format($"Elapsed time {time.Elapsed}", LogType.Trace, lineNumber, caller, GetClassName(sourceFilePath));

                Task.Run(() =>
                {
                    WriteToFile(data);
                });
            }
        }

        public void Trace(in string customMessage = null, in int lineNumber = 0, in string caller = "", in string sourceFilePath = "")
        {
            if (!_configService.IsInited || ConfigService.DefaultSettingsInstanse.TraceEnabled)
            {
                var data = Format(customMessage, LogType.Trace, lineNumber, caller, GetClassName(sourceFilePath));

                Task.Run(() =>
                {
                    WriteToFile(data);
                });
            }
        }

        public void Warn(in string message, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "")
        {
            var data = Format(message, LogType.Warning, lineNumber, caller, GetClassName(sourceFilePath));
            Task.Run(() =>
            {
                WriteToFile(data);
            });
        }

        public void Error(in Exception exception, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "")
        {
            var data = Format(FormatException(exception), LogType.Error, lineNumber, caller, GetClassName(sourceFilePath));

            Task.Run(() =>
            {
                WriteToFile(data);
            });
#if RELEASE
                //Microsoft.AppCenter.Crashes.Crashes.TrackError(exception);
#endif
        }

        public void Error(in string exception, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "")
        {
            var data = Format(exception, LogType.Error, lineNumber, caller, GetClassName(sourceFilePath));

            Task.Run(() =>
            {
                WriteToFile(data);
            });
#if RELEASE
            //Microsoft.AppCenter.Crashes.Crashes.TrackError(new Exception { Source = exception });
#endif
        }

        public void Ui(in string button, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "")
        {
            var message = button == null ? string.Empty : $" '{button}'";

            var data = Format($"User clicked{message}", LogType.Ui, lineNumber, caller, GetClassName(sourceFilePath));

            Task.Run(() =>
            {
                WriteToFile(data);
            });
        }

        public void Ui([CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "")
        {
            Ui(null, lineNumber, caller, sourceFilePath);
        }

        private static string Format(in string message, in LogType type, in int lineNumber = 0, in string caller = "", in string className = "")
        {
            var thread = Thread.CurrentThread.ManagedThreadId == 1 ? "Main" : $"{Thread.CurrentThread.ManagedThreadId}";
            return $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss.fff}] [{thread}] [{type.ToString().ToLower()}] [{className}:{lineNumber}->({caller})] {message}{Environment.NewLine}";
        }

        private static string FormatException(in Exception ex)
        {
            return $"{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{ex.InnerException}";
        }

        private static string GetClassName(in string path)
        {
            return new FileInfo(path).Name;
        }

        private static void WriteToFile(in string data)
        {
            lock (WriteLock)
            {
                CheckFileLength();
                try
                {
                    if (LogFileInfo?.DirectoryName != null)
                        Directory.CreateDirectory(LogFileInfo.DirectoryName);

                    File.AppendAllTextAsync(FilePath, data);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static void CheckFileLength()
        {
            try
            {
                if (LogFileInfo.Length >= MaxLogFileSizeBytes)
                {
                    LogFileInfo.Delete();
                }
            }
            catch (Exception)
            {
                //File.AppendAllTextAsync(FilePath, $"Logger file size more than 200mb, but error occured when deleting it.{Environment.NewLine}{ex.Message}");
            }
        }

        private static void ClearOldLogs()
        {
            var globalLogStopwatch = new Stopwatch();
            globalLogStopwatch.Start();

            DirectoryInfo logsDir;
            FileInfo[] files;
            try
            {
                logsDir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstStrings.LogFolder));
            }
            catch (Exception ex)
            {
                Instance.Error($"Couldn't access logs folder due to error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return;
            }

            try
            {
                files = logsDir.GetFiles();
            }
            catch (Exception ex)
            {
                Instance.Error($"Couldn't get log files from log folder due to error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return;
            }

            foreach (var logFile in files)
            {
                if ((DateTime.Now - logFile.LastWriteTime).Days >= 2)
                {

                    try
                    {
                        logFile.Delete();
                        Instance.Info($"Logger file '{logFile.Name}' removed");
                    }
                    catch (Exception ex)
                    {
                        Instance.Error($"Couldn't delete log file due to error: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    }
                }
            }
            globalLogStopwatch.Stop();

            Instance.Trace($"Global timer for {nameof(ClearOldLogs)}: {globalLogStopwatch.Elapsed}");
        }
    }
}