using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Text.RegularExpressions;

using wPoint = System.Drawing.Point;
using FishingContents.VersionCheck;
using System.Threading;

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

        public MainWindow()
        {
            InitializeComponent();
            VersionCheck();

            mWindow.Title = "Roo Fishing Macro (" + ProgramVersion.Ver + ")";
            InitializeUserInterface();

            img_reference.Source = new BitmapImage(new Uri("Resource/targetImg.png", UriKind.Relative));

            SetMousePosTimer();
        }

        #region Initialize
        private void InitializeUserInterface()
        {
            screen_lefttop_x.Text = "0";
            screen_lefttop_y.Text = "0";
            screen_rightbottom_x.Text = "0";
            screen_rightbottom_y.Text = "0";
            txt_mouse_click_posX.Text = "0";
            txt_mouse_click_posY.Text = "0";

#if DEBUG
            screen_lefttop_x.Text = "392";
            screen_lefttop_y.Text = "179";
            screen_rightbottom_x.Text = "1919";
            screen_rightbottom_y.Text = "1039";
            txt_mouse_click_posX.Text = "1422";
            txt_mouse_click_posY.Text = "694";
#endif
        }

        private void SetMousePosTimer()
        {
            Mouse.Capture(this);
            DispatcherTimer mousePosTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(0.05),
            };
            mousePosTimer.Tick += new EventHandler(MousePosCallBack);
            mousePosTimer.Start();
        }

        private void VersionCheck()
        {
            if (ProgramVersion.IsLastestVer())
                return;

            MessageBoxResult MsgRes = MessageBox.Show("최신 버전이 아닙니다.최신 버전을 다운받으시겠습니까?", "VersionCheck", MessageBoxButton.YesNo);
            if (MsgRes == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(ProgramVersion.GetLeastURL());
                this.Close();
            }
            //Thread t1 = new Thread(delegate ()
            //{
                
            //    return;
            //});
            //t1.Start();
            
        }
        #endregion



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
        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(ProgramVersion.CopyRight());
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
