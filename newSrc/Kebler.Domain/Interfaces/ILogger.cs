using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Kebler.Domain.Interfaces
{
    public interface ILogger
    {
        public void Info(in string message, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "");

        public void Trace(in Stopwatch time, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "");

        public void Trace(in string customMessage = null, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "");

        public void Warn(in string message, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "");

        public void Error(in Exception exception, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "");

        public void Error(in string exception, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "");

        public void Ui(in string button, [CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "");

        public void Ui([CallerLineNumber] in int lineNumber = 0,
            [CallerMemberName] in string caller = "", [CallerFilePath] in string sourceFilePath = "");
    }
}