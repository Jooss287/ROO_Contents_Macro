using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows;

namespace AppPlayer_autoClicker
{
    class ProcessInfobyMousePositionExtension : ProcessInfobyMousePosition
    {
        public RECT handleRect;
        private IntPtr _privhWnd = IntPtr.Zero;
        private Rectangle _drawOutline;
        public void DrawRect()
        {
            int pid;

            if (_privhWnd == _hwnd)
                return;

            GetWindowThreadProcessId(_hwnd, out pid);
            Process proc = Process.GetProcessById(pid);
            IntPtr handle = proc.MainWindowHandle;

            if (handle == IntPtr.Zero)
                return;
            _privhWnd = _hwnd;

            proc.WaitForInputIdle();
            handleRect = new RECT();
            GetWindowRect(handle, ref handleRect);

            IntPtr desktopPtr = GetDC(IntPtr.Zero);
            Graphics g = Graphics.FromHdc(desktopPtr);

            Pen b = new Pen(Color.Red);
            _drawOutline = new Rectangle(handleRect.Left, handleRect.Top, (handleRect.Right - handleRect.Left), (handleRect.Bottom - handleRect.Top));
            g.DrawRectangle(b, _drawOutline);

            g.Dispose();
            ReleaseDC(IntPtr.Zero, desktopPtr);
        }
        IntPtr a;
        public void GetFocusRect()
        {
            POINT pt = new POINT
            {
                x = _mousex,
                y = _mousey
            };
            MapWindowPoints(IntPtr.Zero, _hwnd, ref pt, 1);
            a = ChildWindowFromPoint(_hwnd, pt);
            
            
            return;
            //SetForegroundWindow(_hwnd);
            
            //IntPtr parents = GetParent(_hwnd);
            //while (parents == _hwnd)
            //{
            //    _hwnd = parents;
            //    parents = GetParent(_hwnd);
            //}
        }

        #region dll import
        [DllImport("User32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("User32.dll")]
        public static extern void ReleaseDC(IntPtr hwnd, IntPtr dc);
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);
        [DllImport("user32")]
        public static extern Int32 MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, ref POINT pts, Int32 cPoints);
        [DllImport("user32.dll")]
        public static extern IntPtr ChildWindowFromPoint(IntPtr hWndParent, POINT pt);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner  
            public int Top;         // y position of upper-left corner  
            public int Right;       // x position of lower-right corner  
            public int Bottom;      // y position of lower-right corner  
        }
        public struct POINT
        {
            public Int32 x;
            public Int32 y;
        }
        #endregion
    }
}
