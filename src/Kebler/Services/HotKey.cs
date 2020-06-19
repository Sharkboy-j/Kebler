using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace Kebler.Services
{
    public class HotKey : IDisposable
    {
        private static Dictionary<int, HotKey> _dictHotKeyToCalBackProc;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


        private const int WM_HOT_KEY = 0x0312;
        private bool _disposed;
        private readonly Key _key;
        private readonly KeyModifier _modifire;
        private readonly Action _action;
        private readonly IntPtr _hWnd;
        private  int _id;


        public HotKey(Key key, KeyModifier keyModifiers, Action action, IntPtr wnd, bool register = true)
        {
            _hWnd = wnd;
            _key = key;
            _modifire = keyModifiers;
            _action = action;
            if (register)
            {
                Register();
            }
        }

        public bool Register()
        {
            var virtualKeyCode = KeyInterop.VirtualKeyFromKey(_key);
            _id = virtualKeyCode + ((int)_modifire * 0x10000);
            var result = RegisterHotKey(_hWnd, _id, (uint)_modifire, (uint)virtualKeyCode);

            if (_dictHotKeyToCalBackProc == null)
            {
                _dictHotKeyToCalBackProc = new Dictionary<int, HotKey>();
                ComponentDispatcher.ThreadFilterMessage += ComponentDispatcherThreadFilterMessage;
            }

            _dictHotKeyToCalBackProc.Add(_id, this);

            //Debug.Print(result + ", " + _id + ", " + virtualKeyCode);
            return result;
        }

        public void Unregister()
        {
            if (!_dictHotKeyToCalBackProc.TryGetValue(_id, out _)) return;

            UnregisterHotKey(_hWnd, _id);
            _dictHotKeyToCalBackProc.Remove(_id);
        }

        private static void ComponentDispatcherThreadFilterMessage(ref MSG msg, ref bool handled)
        {
            if (handled) return;
            if (msg.message != WM_HOT_KEY) return;

            if (!_dictHotKeyToCalBackProc.TryGetValue((int) msg.wParam, out var hotKey)) return;

            hotKey._action?.Invoke();
            handled = true;
        }

     
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

       
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                Unregister();
            }
            _disposed = true;
        }
    }

    [Flags]
    public enum KeyModifier
    {
        None = 0x0000,
        Alt = 0x0001,
        Ctrl = 0x0002,
        NoRepeat = 0x4000,
        Shift = 0x0004,
        Win = 0x0008
    }
}
