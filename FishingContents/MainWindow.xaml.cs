using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Text.RegularExpressions;

using wPoint = System.Drawing.Point;

namespace FishingContents
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
        public bool threadloop = false;
        Macro_AutoClick mac;

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("BCD 라이센스로 Opencv/EmguCV 사용. Made by Blurrr\n" +
                "소스공개: https://github.com/Jooss287/Auto-clicker");
        }
        public MainWindow()
        {
            InitializeComponent();

            mWindow.Title = "Roo 자동 낚시 (Ver.200827.00)";
            InitializeUserInterface();

            img_reference.Source = new BitmapImage(new Uri("Resource/targetImg.png", UriKind.Relative));
            
            Mouse.Capture(this);
            DispatcherTimer mousePosTimer = new DispatcherTimer();
            mousePosTimer.Interval = TimeSpan.FromSeconds(0.05);
            mousePosTimer.Tick += new EventHandler(MousePosCallBack);
            mousePosTimer.Start();
        }

        private void InitializeUserInterface()
        {
            screen_lefttop_x.Text = "392";
            screen_lefttop_y.Text = "179";
            screen_rightbottom_x.Text = "1919";
            screen_rightbottom_y.Text = "1039";
            txt_mouse_click_posX.Text = "1422";
            txt_mouse_click_posY.Text = "694";
        }
        
        

        #region CallBack
        private void MousePosCallBack(object sender, EventArgs e)
        {
            Point pointToWindow = Macro_AutoClick.GetMousePosition();
            txt_mouse_pos.Text = pointToWindow.X.ToString() + ", " + pointToWindow.Y.ToString();
        }
        private void Btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (threadloop == false)
            {
                wPoint mouse_pos = new wPoint(
                    Convert.ToInt32(txt_mouse_click_posX.Text),
                    Convert.ToInt32(txt_mouse_click_posY.Text));
                wPoint lefttop = new wPoint(Convert.ToInt32(screen_lefttop_x.Text), Convert.ToInt32(screen_lefttop_y.Text));
                wPoint rightbottom = new wPoint(Convert.ToInt32(screen_rightbottom_x.Text), Convert.ToInt32(screen_rightbottom_y.Text));

                mac = new Macro_AutoClick(mouse_pos, lefttop, rightbottom) ;
                mac.StartThread();
                threadloop = true;
            }
            else
            {
                mac.EndThread();
                threadloop = false;
            }
            return;
        }
        #endregion

        #region keyboard hook
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x100;

        private LowLevelKeyboardProc _proc = hookProc;

        private static IntPtr hhook = IntPtr.Zero;
        public void SetHook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hInstance, 0);
        }
        public static void UnHook()
        {
            UnhookWindowsHookEx(hhook);
        }

        public void EndThreadCmd()
        {
            threadloop = false;
        }
        private static Action NonStaticDelegate;
        public static IntPtr hookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode.ToString() == "118")
                {
                    MainWindow temp = new MainWindow();
                    MainWindow.NonStaticDelegate = new Action(temp.EndThreadCmd);
                    return (IntPtr)1;
                }
                else
                    return CallNextHookEx(hhook, code, (int)wParam, lParam);
            }
            else
                return CallNextHookEx(hhook, code, (int)wParam, lParam);
        }
        #endregion

        #region UI Setting
        public bool IsNumeric(string source)
        {
            Regex regex = new Regex("[^0-9.-]+");
            return !regex.IsMatch(source);
        }
        private void NurmericCheckFunc(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsNumeric(e.Text);
        }
        private void TxtboxSelectAll(object sender, RoutedEventArgs e)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(
                    delegate
                    {
                        (sender as TextBox).SelectAll();
                    }
                )
            );
        }
        #endregion
    }


}
