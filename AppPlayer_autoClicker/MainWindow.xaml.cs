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
        ProcessInfobyMousePositionExtension ext = new ProcessInfobyMousePositionExtension();

        public MainWindow()
        {
            InitializeComponent();

            targetImage = new BitmapImage(new Uri("Resource/targetImg.png", UriKind.Relative));
            img_reference.Source = targetImage;
            
            Mouse.Capture(this);
            DispatcherTimer mousePosTimer = new DispatcherTimer();
            mousePosTimer.Interval = TimeSpan.FromSeconds(0.05);
            mousePosTimer.Tick += new EventHandler(MousePosCallBack);
            mousePosTimer.Start();

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
            //ext.DrawRect();
            ext.GetFocusRect();
            btn_sel_process.Content = Convert.ToString(ext.handleRect.Left) +','+ Convert.ToString(ext.handleRect.Top) ;
        }
        
        private void Btn_start_Click(object sender, RoutedEventArgs e)
        {
            CheckSimilarity();
            return;
            int mouseX = System.Convert.ToInt32(txt_mouse_click_posX.Text);
            int mouseY = System.Convert.ToInt32(txt_mouse_click_posY.Text);
            SetCursorPos(mouseX, mouseY);
            mouse_event((int)MouseFlag.ME_LEFTDOWN, mouseX, mouseY, 0, 0);
            mouse_event((int)MouseFlag.ME_LEFTUP, 0, 0, 0, 0);
            return;
        }

        private BitmapImage CaptureWindow()
        {
            //int width = (int)SystemParameters.PrimaryScreenWidth;
            //int height = (int)SystemParameters.PrimaryScreenHeight;
            int width = 200;
            int height = 200;

            using (Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.CopyFromScreen(0, 0, 0, 0, bmp.Size);
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

        private void CheckSimilarity()
        {
            BitmapImage bmpImage = CaptureWindow();
            Mat matCapture = BitmapSourceExtension.ToMat(bmpImage);

            Mat matTarget = BitmapSourceExtension.ToMat(targetImage);
            double compareRatio = CvInvoke.CompareHist(matCapture, matTarget, Emgu.CV.CvEnum.HistogramCompMethod.Correl);
            btn_start.Content = Convert.ToString(compareRatio);
            return;
            //CompareHist
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
    }


}
