using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;


namespace AppPlayer_autoClicker
{
    [Flags]
    public enum MouseFlag
    {
        ME_MOVE = 1, ME_LEFTDOWN = 2, ME_LEFTUP = 4, ME_RIGHTDOWN = 8,
        ME_RIGHTUP = 0x10, ME_MIDDLEDOWN = 0x20, ME_MIDDLEUP = 0x40,
        ME_WHEEL = 0x800, ME_ABSOULUTE = 8000
    }

    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);
        [DllImport("user32.dll")]
        static extern void mouse_event(int flag, int dx, int dy, int buttons, int extra);
        [DllImport("user32.dll")]
        static extern int SetCursorPos(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        Point autoMouseClickPos;
        Button button;

        private void MousePosCallBack(object sender, EventArgs e)
        {
            Point pointToWindow = GetMousePosition();
            txt_mouse_pos.Text = pointToWindow.X.ToString() + ", " + pointToWindow.Y.ToString();
        }
        public MainWindow()
        {
            InitializeComponent();

            Mouse.Capture(this);
            DispatcherTimer mousePosTimer = new DispatcherTimer();
            mousePosTimer.Interval = TimeSpan.FromSeconds(0.05);
            mousePosTimer.Tick += new EventHandler(MousePosCallBack);
            mousePosTimer.Start();
            
        }
    private void Btn_start_Click(object sender, RoutedEventArgs e)
        {
            int mouseX = System.Convert.ToInt32(txt_mouse_click_posX.Text);
            int mouseY = System.Convert.ToInt32(txt_mouse_click_posY.Text);
            SetCursorPos(mouseX, mouseY);
            mouse_event((int)MouseFlag.ME_LEFTDOWN, mouseX, mouseY, 0, 0);
            mouse_event((int)MouseFlag.ME_LEFTUP, 0, 0, 0, 0);
        }


    }

    
}
