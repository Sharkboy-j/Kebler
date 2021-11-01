using System;
using System.Runtime.CompilerServices;

namespace Kebler.Services.Interfaces
{
    public interface ILog
    {
        public void Info(string message, [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = "", [CallerFilePath] string sourceFilePath = "");

        public void Trace(TimeSpan time, [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = "", [CallerFilePath] string sourceFilePath = "");

        public void Warn(string message, [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = "", [CallerFilePath] string sourceFilePath = "");

        public void Error(Exception exception, [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = "", [CallerFilePath] string sourceFilePath = "");

        public void Error(string exception, [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = "", [CallerFilePath] string sourceFilePath = "");

        public void Ui(string button, [CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = "", [CallerFilePath] string sourceFilePath = "");

        public void Ui([CallerLineNumber] int lineNumber = 0,
            [CallerMemberName] string caller = "", [CallerFilePath] string sourceFilePath = "");
    }
}