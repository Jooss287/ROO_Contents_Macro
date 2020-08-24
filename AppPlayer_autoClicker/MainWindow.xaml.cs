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

using Emgu.CV;
using System.IO;
using System.Drawing.Imaging;
using Emgu.CV.Dnn;
using System.Runtime.CompilerServices;

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
        readonly BitmapImage targetImage;
        readonly BitmapImage targetImage1;
        ProcessInfobyMousePositionExtension ext = new ProcessInfobyMousePositionExtension();
        const int template_width = 156;
        const int template_height = 156;
        const int template_screen_width = 1280;
        const int template_screen_height = 720;
        const int template_left_width = 78;
        const int template_right_width = 78;
        const int template_bottom_height = 25;
        const int template_top_height = 131;

        public bool threadloop = false;

        System.Drawing.Point mouse_pos;
        System.Drawing.Point lefttop;
        System.Drawing.Point rightbottom;

        public MainWindow()
        {
            InitializeComponent();

            mWindow.Title = "Roo 자동 낚시 (Ver.200825.00)";
            InitializeUserInterface();

            string filename = Environment.CurrentDirectory + "\\Resource\\targetImg.png";
            targetImage = new BitmapImage(new Uri(filename, UriKind.Relative));
            targetImage1 = new BitmapImage(new Uri("Resource/targetImg.png", UriKind.Relative));
            img_reference.Source = targetImage1;
            
            Mouse.Capture(this);
            DispatcherTimer mousePosTimer = new DispatcherTimer();
            mousePosTimer.Interval = TimeSpan.FromSeconds(0.05);
            mousePosTimer.Tick += new EventHandler(MousePosCallBack);
            mousePosTimer.Start();
        }

        private void InitializeUserInterface()
        {
            screen_lefttop_x.Text = "428";
            screen_lefttop_y.Text = "100";
            screen_rightbottom_x.Text = "1784";
            screen_rightbottom_y.Text = "864";
            txt_mouse_click_posX.Text = "1342";
            txt_mouse_click_posY.Text = "559";
        }
        
        public static System.Windows.Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new System.Windows.Point(w32Mouse.X, w32Mouse.Y);
        }

        private void MousePosCallBack(object sender, EventArgs e)
        {
            System.Windows.Point pointToWindow = GetMousePosition();
            txt_mouse_pos.Text = pointToWindow.X.ToString() + ", " + pointToWindow.Y.ToString();
        }
        private void CallBack_SelProcess(object  sender, EventArgs e)
        {
            System.Windows.Point pointToWindow = GetMousePosition();
            ext.SetPositon(Convert.ToInt32(pointToWindow.X), Convert.ToInt32(pointToWindow.Y));
            ext.GetFocusRect();
            //btn_sel_process.Content = Convert.ToString(ext.handleRect.Left) +','+ Convert.ToString(ext.handleRect.Top) ;
        }
        
        private void Btn_start_Click(object sender, RoutedEventArgs e)
        {
            if (threadloop == false)
            {
                mouse_pos.X = System.Convert.ToInt32(txt_mouse_click_posX.Text);
                mouse_pos.Y = System.Convert.ToInt32(txt_mouse_click_posY.Text);
                lefttop = new System.Drawing.Point(Convert.ToInt32(screen_lefttop_x.Text), Convert.ToInt32(screen_lefttop_y.Text));
                rightbottom = new System.Drawing.Point(Convert.ToInt32(screen_rightbottom_x.Text), Convert.ToInt32(screen_rightbottom_y.Text));

                Thread t1 = new Thread(new ThreadStart(AutoFishing));
                threadloop = true;
                SetHook();
                t1.Start();
            }
            else
            { 
                threadloop = false;
            }

            return;
        }

        void AutoFishing()
        {
            while(threadloop)
            { 
            SetCursorPos(mouse_pos.X, mouse_pos.Y);
            mouse_event((int)MouseFlag.ME_LEFTDOWN, mouse_pos.X, mouse_pos.Y, 0, 0);
            mouse_event((int)MouseFlag.ME_LEFTUP, 0, 0, 0, 0);

            double similarity;
            do
            {
                if (!threadloop)
                {
                    UnHook();
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        btn_start.Content = "매크로 종료";
                    }));
                    return;
                }
                similarity = CheckSimilarity();
                Thread.Sleep(100);
            } while (similarity > 2000000);

            SetCursorPos(mouse_pos.X, mouse_pos.Y);
            mouse_event((int)MouseFlag.ME_LEFTDOWN, mouse_pos.X, mouse_pos.Y, 0, 0);
            mouse_event((int)MouseFlag.ME_LEFTUP, 0, 0, 0, 0);

            Thread.Sleep(2000);

            SetCursorPos(mouse_pos.X, mouse_pos.Y);
            mouse_event((int)MouseFlag.ME_LEFTDOWN, mouse_pos.X, mouse_pos.Y, 0, 0);
            mouse_event((int)MouseFlag.ME_LEFTUP, 0, 0, 0, 0);

            Thread.Sleep(2000);
            }

            UnHook();
            return;
        }

        private BitmapImage CaptureWindow(System.Drawing.Point lefttop, System.Drawing.Point rightbottom)
        {
            //int width = (int)SystemParameters.PrimaryScreenWidth;
            //int height = (int)SystemParameters.PrimaryScreenHeight;
            int width = rightbottom.X - lefttop.X;
            int height = rightbottom.Y - lefttop.Y;

            using (Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.CopyFromScreen(lefttop.X, lefttop.Y, 0, 0, bmp.Size);
                }

                using (MemoryStream memory = new MemoryStream())
                {
                    bmp.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapimage = new BitmapImage();
                    bitmapimage.BeginInit();
                    bitmapimage.StreamSource = memory;
                    bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapimage.EndInit();

                    return bitmapimage;
                }
            }
        }

        private double CheckSimilarity()
        {
            
            int screen_width = rightbottom.X - lefttop.X;
            int screen_height = rightbottom.Y - lefttop.Y;

            double templateWidthRatio = ((double)template_left_width) / (double)template_screen_width * (double)screen_width;
            double templateTopRatio = ((double)template_top_height) / (double)template_screen_height * (double)screen_height;
            double templateBottomratio = ((double)template_bottom_height) / (double)template_screen_height * (double)screen_height;

            
            System.Drawing.Point template_lefttop = new System.Drawing.Point(
                mouse_pos.X - Convert.ToInt32(Math.Round(templateWidthRatio)), mouse_pos.Y - Convert.ToInt32(Math.Round(templateTopRatio)));
            System.Drawing.Point template_rightbottom = new System.Drawing.Point(
                mouse_pos.X + Convert.ToInt32(Math.Round(templateWidthRatio)), mouse_pos.Y + Convert.ToInt32(Math.Round(templateBottomratio)));

            BitmapImage bmpImage = CaptureWindow(template_lefttop, template_rightbottom);
            Mat matCapture = BitmapSourceExtension.ToMat(bmpImage);

            //BitmapImage temp = targetImage;
            Mat targetResize = new Mat();
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                targetResize = BitmapSourceExtension.ToMat(targetImage);
            }));
             
            CvInvoke.Resize(targetResize, targetResize, new System.Drawing.Size(template_rightbottom.X - template_lefttop.X, template_rightbottom.Y - template_lefttop.Y), 0, 0, Emgu.CV.CvEnum.Inter.Linear);

            matCapture.ConvertTo(matCapture, Emgu.CV.CvEnum.DepthType.Cv32F);
            targetResize.ConvertTo(targetResize, Emgu.CV.CvEnum.DepthType.Cv32F);

            double compareRatio = CvInvoke.CompareHist(targetResize, matCapture, Emgu.CV.CvEnum.HistogramCompMethod.Chisqr);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                img_reference.Source = BitmapSourceExtension.ToBitmapSource(matCapture);
                btn_start.Content = Convert.ToString(compareRatio);
            }));

            return compareRatio;
            //CompareHist
        }

        public static void EndThread11()
        {
            
        }

        #region dll import
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
        #endregion

        private void btn_sel_process_Click(object sender, RoutedEventArgs e)
        {
            Mouse.Capture(this);
            DispatcherTimer mousePosTimer = new DispatcherTimer();
            mousePosTimer.Interval = TimeSpan.FromSeconds(0.05);
            mousePosTimer.Tick += new EventHandler(CallBack_SelProcess);
            mousePosTimer.Start();
        }

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
                }
                return (IntPtr)1;
            }
            else
                return CallNextHookEx(hhook, code, (int)wParam, lParam);
        }
        #endregion

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("BCD 라이센스로 Opencv/EmguCV 사용. Made by Blurrr\n" +
                "소스공개: https://github.com/Jooss287/Auto-clicker");
        }
    }


}
