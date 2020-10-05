using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Kebler.UI.CSControls.Window
{
    public static class WindowLocationStateExtensions
    {
        internal const int WM_GETMINMAXINFO = 36;
        internal const int WM_WINDOWPOSCHANGED = 71;
        private const int MONITOR_DEFAULTTONEAREST = 2;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect(
            [In] ref RECT lprc,
            uint dwFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetMonitorInfo(
            IntPtr hMonitor,
            ref MONITORINFO lpmi);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(
            IntPtr hWnd,
            [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(
            IntPtr hWnd,
            out WINDOWPLACEMENT lpwndpl);

        [DllImport("shell32.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern uint SHAppBarMessage(
            int dwMessage,
            ref APPBARDATA pData);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr handle, int flags);

        public static void SetWindowLocationState(this System.Windows.Window window, WindowLocationState state)
        {
            var windowPlacement = ToWindowPlacement(state);
            var hMonitor = MonitorFromRect(ref windowPlacement.normalPosition, 2U);
            var monitorinfo = new MONITORINFO {cbSize = (uint) Marshal.SizeOf(typeof(MONITORINFO))};
            ref var local = ref monitorinfo;
            if (GetMonitorInfo(hMonitor, ref local) &&
                !RectanglesIntersect(windowPlacement.normalPosition, monitorinfo.rcMonitor))
                windowPlacement.normalPosition = PlaceOnScreen(monitorinfo.rcMonitor, windowPlacement.normalPosition);
            SetWindowPlacement(new WindowInteropHelper(window).Handle, ref windowPlacement);
        }

        public static WindowLocationState GetWindowLocationState(this System.Windows.Window window)
        {
            var placement = GetPlacement(new WindowInteropHelper(window).Handle);
            var left = placement.normalPosition.Left;
            var top = placement.normalPosition.Top;
            var num1 = placement.normalPosition.Bottom - placement.normalPosition.Top;
            var num2 = placement.normalPosition.Right - placement.normalPosition.Left;
            return new WindowLocationState(left, top, num2, num1,
                window.WindowState);
        }

        public static WindowLocationState GetWindowLocationStateX(this System.Windows.Window window)
        {
            var placement = GetPlacement(new WindowInteropHelper(window).Handle);
            var unitX = 0;
            var unitY = 0;
            if (window.WindowState != WindowState.Maximized)
                TransformFromPixels(window, placement.normalPosition.Left,
                    placement.normalPosition.Top, out unitX, out unitY);
            var actualWidth = window.ActualWidth;
            var actualHeight = window.ActualHeight;
            return new WindowLocationState(unitX, unitY, actualWidth, actualHeight,
                window.WindowState);
        }

        public static void GetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
        {
            var structure = (MINMAXINFO) Marshal.PtrToStructure(lParam, typeof(MINMAXINFO));
            var hMonitor = MonitorFromWindow(hwnd, 2);
            if (structure != null && AutoHideEnabled())
            {
                var lpmi = new MONITORINFO {cbSize = (uint) Marshal.SizeOf(typeof(MONITORINFO))};
                GetMonitorInfo(hMonitor, ref lpmi);
                var rcWork = lpmi.rcWork;
                var rcMonitor = lpmi.rcMonitor;
                structure.ptMaxPosition.X = Math.Abs(rcWork.Left - rcMonitor.Left);
                structure.ptMaxPosition.Y = Math.Abs(rcWork.Top - rcMonitor.Top);
                structure.ptMaxSize.X = Math.Abs(rcWork.Right - rcWork.Left);
                structure.ptMaxSize.Y = Math.Abs(rcWork.Bottom - rcWork.Top - 1);
            }

            Marshal.StructureToPtr(structure, lParam, true);
        }

        public static bool AutoHideEnabled()
        {
            var pData = new APPBARDATA();
            return SHAppBarMessage(4, ref pData) > 0U;
        }

        public static void TransformFromPixels(
            Visual visual,
            double pixelX,
            double pixelY,
            out int unitX,
            out int unitY)
        {
            var presentationSource = PresentationSource.FromVisual(visual);
            Matrix transformToDevice;
            if (presentationSource != null)
                transformToDevice = presentationSource.CompositionTarget.TransformToDevice;
            else
                using (var hwndSource = new HwndSource(new HwndSourceParameters()))
                {
                    transformToDevice = hwndSource.CompositionTarget.TransformToDevice;
                }

            unitX = (int) (pixelX / transformToDevice.M11);
            unitY = (int) (pixelY / transformToDevice.M22);
        }

        public static void TransformToPixels(
            Visual visual,
            double unitX,
            double unitY,
            out int pixelX,
            out int pixelY)
        {
            var presentationSource = PresentationSource.FromVisual(visual);
            Matrix transformToDevice;
            if (presentationSource != null)
                transformToDevice = presentationSource.CompositionTarget.TransformToDevice;
            else
                using (var hwndSource = new HwndSource(new HwndSourceParameters()))
                {
                    transformToDevice = hwndSource.CompositionTarget.TransformToDevice;
                }

            pixelX = (int) (transformToDevice.M11 * unitX);
            pixelY = (int) (transformToDevice.M22 * unitY);
        }

        private static WINDOWPLACEMENT ToWindowPlacement(
            WindowLocationState state)
        {
            return new WINDOWPLACEMENT
            {
                minPosition = new POINT(-1, -1),
                maxPosition = new POINT(-1, -1),
                normalPosition = new RECT((int) state.Left, (int) state.Top, (int) (state.Left + state.Width),
                    (int) (state.Top + state.Height)),
                length = Marshal.SizeOf(typeof(WINDOWPLACEMENT)),
                flags = 0,
                showCmd = ToShowCmd(state.WindowState)
            };
        }

        private static int ToShowCmd(WindowState windowState)
        {
            if (windowState == WindowState.Minimized)
                return 2;
            return windowState == WindowState.Maximized ? 3 : 1;
        }

        private static bool RectanglesIntersect(
            RECT a,
            RECT b)
        {
            return a.Left <= b.Right && a.Right >= b.Left && a.Top <= b.Bottom && a.Bottom >= b.Top;
        }

        private static RECT PlaceOnScreen(
            RECT monitorRect,
            RECT windowRect)
        {
            var num1 = monitorRect.Right - monitorRect.Left;
            var num2 = monitorRect.Bottom - monitorRect.Top;
            if (windowRect.Right < monitorRect.Left)
            {
                var num3 = windowRect.Right - windowRect.Left;
                if (num3 > num1)
                    num3 = num1;
                windowRect.Left = monitorRect.Left;
                windowRect.Right = windowRect.Left + num3;
            }
            else if (windowRect.Left > monitorRect.Right)
            {
                var num3 = windowRect.Right - windowRect.Left;
                if (num3 > num1)
                    num3 = num1;
                windowRect.Right = monitorRect.Right;
                windowRect.Left = windowRect.Right - num3;
            }

            if (windowRect.Bottom < monitorRect.Top)
            {
                var num3 = windowRect.Bottom - windowRect.Top;
                if (num3 > num2)
                    num3 = num2;
                windowRect.Top = monitorRect.Top;
                windowRect.Bottom = windowRect.Top + num3;
            }
            else if (windowRect.Top > monitorRect.Bottom)
            {
                var num3 = windowRect.Bottom - windowRect.Top;
                if (num3 > num2)
                    num3 = num2;
                windowRect.Bottom = monitorRect.Bottom;
                windowRect.Top = windowRect.Bottom - num3;
            }

            return windowRect;
        }

        private static WINDOWPLACEMENT GetPlacement(
            IntPtr windowHandle)
        {
            var lpwndpl = new WINDOWPLACEMENT();
            GetWindowPlacement(windowHandle, out lpwndpl);
            return lpwndpl;
        }

        [Serializable]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        [Serializable]
        private struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT minPosition;
            public POINT maxPosition;
            public RECT normalPosition;
        }

        [Serializable]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }
        }

        private struct MONITORINFO
        {
            public uint cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class MINMAXINFO
        {
            public POINT ptMaxPosition;
            public POINT ptMaxSize;
            public POINT ptMaxTrackSize;
            public POINT ptMinTrackSize;
            public POINT ptReserved;
        }

        private struct APPBARDATA
        {
            public int cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public uint uEdge;
            public RECT rc;
            public int lParam;
        }
    }
}