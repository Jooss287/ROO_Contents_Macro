using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows;
using System.Drawing;

namespace AppPlayer_autoClicker
{
    class ProcessInfobyMousePosition
    {
        #region public 멤버
        public int _mousex, _mousey;
        public IntPtr _hwnd;
        public string _processname;
        public string _windowtext;
        public string _classname;
        #endregion

        #region public method
        public void SetPositon(int x, int y)
        {
            if (x >= 0 & y >= 0)
            {
                _mousex = x; _mousey = y;
                _hwnd = GetHandle();
                _processname = GetProcessName(_hwnd);
                _windowtext = GetWindowText(_hwnd);
                _classname = GetClassName(_hwnd);
            }
        }
        #endregion

        #region private method
        private string GetProcessName(IntPtr hWnd)
        {
            int pid;
            GetWindowThreadProcessId(hWnd, out pid);
            Process proc = Process.GetProcessById(pid);
            return proc.MainModule.ModuleName;
        }
        private IntPtr GetHandle()
        {
            IntPtr hWnd = WindowFromPoint(_mousex, _mousey);
            if (hWnd == IntPtr.Zero)
                return hWnd;
            else
                return hWnd;
        }
        private string GetWindowText(IntPtr hWnd)
        {
            StringBuilder text = new StringBuilder(256);
            if (GetWindowText(hWnd, text, text.Capacity) > 0)
            {
                return text.ToString();
            }
            return String.Empty;
        }
        private string GetClassName(IntPtr hWnd)
        {
            StringBuilder className = new StringBuilder(100);
            if (GetClassName(hWnd, className, className.Capacity) > 0)
            {
                return className.ToString();
            }
            return String.Empty;
        }
        #endregion

        #region dll import
        [DllImport("user32.dll")]
        protected static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32.dll")]
        protected static extern IntPtr WindowFromPoint(int x, int y);
        [DllImport("user32.dll")]
        protected static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        protected static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    }
    #endregion
}
