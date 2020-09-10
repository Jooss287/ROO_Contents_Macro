using System;
using System.Runtime.InteropServices;
using Emgu.CV;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

using wPoint = System.Drawing.Point;

namespace FishingContents
{
    
    class Macro_AutoClick
    {
        const int CLICK_THRESHOLD = 1000000;
        readonly wPoint appPlayerLT;
        readonly wPoint appPlayerRB;

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
        readonly wPoint mouse_pos;
        private wPoint capLT;
        private wPoint capRB;
        private Thread macroThread;
        private bool ThreadLoop;
        public Macro_AutoClick(wPoint param_mouse_pos, wPoint param_screenLT, wPoint param_screenRB)
        {
            mouse_pos = param_mouse_pos;
            appPlayerLT = param_screenLT;
            appPlayerRB = param_screenRB;
            CalcTargetSize(param_screenLT, param_screenRB);
            ThreadLoop = false;
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
        public static System.Windows.Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new System.Windows.Point(w32Mouse.X, w32Mouse.Y);
        }
        private void MouseClick()
        {
            SetCursorPos(mouse_pos.X, mouse_pos.Y);
            mouse_event((int)MouseFlag.ME_LEFTDOWN, mouse_pos.X, mouse_pos.Y, 0, 0);
            mouse_event((int)MouseFlag.ME_LEFTUP, 0, 0, 0, 0);
        }
        private void CalcTargetSize(wPoint param_screenLT, wPoint param_screenRB)
        {
            const int template_width = 156;
            const int template_height = 156;
            const int template_screen_width = 1280;
            const int template_screen_height = 720;
            const int template_left_width = 78;
            const int template_right_width = 78;
            const int template_bottom_height = 25;
            const int template_top_height = 131;

            const double templateWidthRatio = (double)template_left_width / (double)template_screen_width;
            const double templateTopRatio = (double)template_top_height / (double)template_screen_height;
            const double templateBottomratio = (double)template_bottom_height / (double)template_screen_height;

            int screen_width = param_screenRB.X - param_screenLT.X;
            int screen_height = param_screenRB.Y - param_screenLT.Y;

            double target_cap_width = templateWidthRatio * screen_width;
            double target_cap_top = templateTopRatio * screen_height;
            double target_cap_bottom = templateBottomratio * screen_height;

            capLT = new wPoint(
                mouse_pos.X - Convert.ToInt32(Math.Round(target_cap_width)), mouse_pos.Y - Convert.ToInt32(Math.Round(target_cap_top)));
            capRB = new wPoint(
                mouse_pos.X + Convert.ToInt32(Math.Round(target_cap_width)), mouse_pos.Y + Convert.ToInt32(Math.Round(target_cap_bottom)));
        }
        private Mat CaptureMatWindow()
        {
            BitmapImage bmpImage = CaptureWindow(capLT, capRB);
            Mat matCapture = BitmapSourceExtension.ToMat(bmpImage);
            matCapture.ConvertTo(matCapture, Emgu.CV.CvEnum.DepthType.Cv32F);

            return matCapture;
        }

        public void StartThread()
        {
            if (ThreadLoop)
                return ;
            macroThread = new Thread(new ThreadStart(AutoFishing));
            ThreadLoop = true;
            macroThread.Start();
        }
        public void EndThread()
        {
            if (ThreadLoop)
                ThreadLoop = false;
        }
        void AutoFishing()
        {
            while (ThreadLoop)
            {
                Thread.Sleep(200);
                Mat refImg = CaptureMatWindow();
                MouseClick();
                
                double similarity;
                int timecheck = 0;
                do
                {
                    if (timecheck >= 140)
                    {
                        MouseClick();
                        timecheck = 0;
                    }
                        
                    if (!ThreadLoop)
                    {
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            ((MainWindow)Application.Current.MainWindow).btn_start.Content = "매크로 시작";
                        }));
                        return;
                    }
                    similarity = CheckSimilarity(ref refImg);
                    Thread.Sleep(50);
                    timecheck++;
                } while (similarity < CLICK_THRESHOLD);

                MouseClick();
                Thread.Sleep(2000);

                ScreenCapture();
                MouseClick();
                Thread.Sleep(200);
            }
            return;
        }
        private double CheckSimilarity(ref Mat refStateImg)
        {
            Mat tempImg = CaptureMatWindow();
            
            double compareRatio = CvInvoke.CompareHist(refStateImg, tempImg, Emgu.CV.CvEnum.HistogramCompMethod.Chisqr);

            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                ((MainWindow)Application.Current.MainWindow).img_reference.Source = BitmapSourceExtension.ToBitmapSource(tempImg);
                ((MainWindow)Application.Current.MainWindow).btn_start.Content = Convert.ToString(Convert.ToInt32(compareRatio));
            }));

            return compareRatio;
        }

        public void ScreenCapture()
        {
            BitmapImage bmpImage = CaptureWindow(appPlayerLT, appPlayerRB);
            Mat matCapture = BitmapSourceExtension.ToMat(bmpImage);
            //matCapture.ConvertTo(matCapture, Emgu.CV.CvEnum.DepthType.Cv32F);

            string savePath = "SaveImage";
            string saveName = "ScreenShot";
            string saveExt = "jpg";

            string fileName = SaveFileName(savePath, saveName, saveExt);
            CheckFolder(savePath);
            matCapture.Save(fileName);
        }

        #region file save
        private string SaveFileName(string saveFolder, string baseFileName, string fileExt)
        {
            if (saveFolder == "") saveFolder = Environment.CurrentDirectory;   // 실행파일위치로 변경
            string today = String.Format(DateTime.Now.ToString("yyyyMMdd"));   // 오늘날짜 입력 21080101
            baseFileName = today + baseFileName;

            FileInfo fi = new FileInfo(saveFolder + "\\" + baseFileName + "_001" + "." + fileExt);   // 실행파일위치 + 오늘날짜 + 순번(001)
            string tmpPath = "";
            int i = 1;   // 순번 증가
            while (fi.Exists)   // 파일이 있으면 계속 반복 (순번 증가)
            {
                tmpPath = saveFolder + "\\" + baseFileName + (++i).ToString("_000") + "." + fileExt;   // ex) basefileName_001.fileExt
                fi = new FileInfo(tmpPath);
            }
            tmpPath = saveFolder + "\\" + baseFileName + i.ToString("_000") + "." + fileExt;   // ex) basefileName_001.fileExt
            return tmpPath;
        }
        
        private void CheckFolder(string savePath)
        {
            DirectoryInfo di = new DirectoryInfo(savePath);
            if (!di.Exists) di.Create();
        }
        #endregion
    }
}
