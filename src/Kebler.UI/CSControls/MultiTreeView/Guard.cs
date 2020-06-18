using System;
using System.Diagnostics;
using System.Threading;

namespace Kebler.UI.CSControls.MuliTreeView
{
    public static class Guard
    {
        [DebuggerStepThrough]
        public static bool NotNull<T>(out T result, T obj) where T : class
        {
            result = obj;
            return result == null;
        }

        [DebuggerStepThrough]
        public static bool NotNull<T>(T obj, out T result) where T : class
        {
            result = obj;
            return result == null;
        }

        [DebuggerStepThrough]
        public static bool NotNull<T>(T obj) where T : class
        {
            return obj == null;
        }

        [DebuggerStepThrough]
        public static bool NotNull(object obj1, object obj2)
        {
            return obj1 == null || obj2 == null;
        }

        [DebuggerStepThrough]
        public static bool NotNull(object obj1, object obj2, object obj3)
        {
            return obj1 == null || obj2 == null || obj3 == null;
        }

        [DebuggerStepThrough]
        public static bool NotNullOrEmpty(string obj)
        {
            return string.IsNullOrEmpty(obj);
        }

        [DebuggerStepThrough]
        public static bool IsNull<T>(T obj) where T : class
        {
            return obj != null;
        }

        [DebuggerStepThrough]
        public static bool True(bool condition)
        {
            return !condition;
        }

        [DebuggerStepThrough]
        public static bool False(bool condition)
        {
            return condition;
        }

       
        [DebuggerStepThrough]
        [Conditional("DEBUG")]
        public static void AssertIsMainThread()
        {
            if (Thread.CurrentThread.IsBackground)
                throw new InvalidOperationException("Invalid thread");
        }

        [DebuggerStepThrough]
        [Conditional("DEBUG")]
        public static void AssertIsBackgroundThread()
        {
            if (!Thread.CurrentThread.IsBackground)
                throw new InvalidOperationException("Invalid thread");
        }
    }
}
