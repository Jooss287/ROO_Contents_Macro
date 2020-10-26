using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Text.RegularExpressions;

using wPoint = System.Drawing.Point;
using FishingContents.VersionCheck;
using System.Collections.Generic;

namespace FishingContents
{
    [Flags]
    public enum MouseFlag
    {
        ME_MOVE = 1, ME_LEFTDOWN = 2, ME_LEFTUP = 4, ME_RIGHTDOWN = 8,
        ME_RIGHTUP = 0x10, ME_MIDDLEDOWN = 0x20, ME_MIDDLEUP = 0x40,
        ME_WHEEL = 0x800, ME_ABSOULUTE = 8000
    }
    public enum PointList : int
    {
        LEFTTOP,
        RIGHTBOTTOM,
        CLICKPOS
    }

    public partial class MainWindow : Window
    {
        public bool threadloop = false;
        Macro_AutoClick mac;
        int SavePoint { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            VersionCheck();

            mWindow.Title = "Roo Fishing Macro (" + ProgramVersion.Ver + ")";
            
            InitSavePoint();
            LoadPoints(Properties.Settings.Default.set_point[0]);

            img_reference.Source = new BitmapImage(new Uri("Resource/targetImg.png", UriKind.Relative));

            SetMousePosTimer();

        }

        #region Initialize
        void InitSavePoint()
        {
            if ( Properties.Settings.Default.set_point == null)
            {
                List<List<Point>> save_point = new List<List<Point>>();
                for ( int i = 0;i < 4; i++)
                {
                    Point left_top = new Point(0, 0);
                    Point right_bottom = new Point(0, 0);
                    Point click_pos = new Point(0, 0);
                    List<Point> points = new List<Point>();
                    points.Add(left_top);
                    points.Add(right_bottom);
                    points.Add(click_pos);
                    save_point.Add(points);
                }
                Properties.Settings.Default.set_point = save_point;
            }
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

                mac = new Macro_AutoClick(mouse_pos, lefttop, rightbottom);
                mac.IsScreenShotEnabled = Convert.ToBoolean(isScreenShot.IsChecked);
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

        void LoadPoints(List<Point> points)
        {
            if (points == null)
            {
                Point left_top = new Point(0, 0);
                Point right_bottom = new Point(0, 0);
                Point click_pos = new Point(0, 0);
                points.Add(left_top);
                points.Add(right_bottom);
                points.Add(click_pos);
            }
            screen_lefttop_x.Text = Convert.ToString(points[(int)PointList.LEFTTOP].X);
            screen_lefttop_y.Text = Convert.ToString(points[(int)PointList.LEFTTOP].Y);
            screen_rightbottom_x.Text = Convert.ToString(points[(int)PointList.RIGHTBOTTOM].X);
            screen_rightbottom_y.Text = Convert.ToString(points[(int)PointList.RIGHTBOTTOM].Y);
            txt_mouse_click_posX.Text = Convert.ToString(points[(int)PointList.CLICKPOS].X);
            txt_mouse_click_posY.Text = Convert.ToString(points[(int)PointList.CLICKPOS].Y);
        }
        void ChangePoints()
        {
            if (!CheckEquals(Properties.Settings.Default.set_point[SavePoint], GetNowPoints()))
            {
                MessageBoxResult MsgRes = MessageBox.Show("변경 사항이 있습니다 저장하시겠습니까?", "Save Point", MessageBoxButton.YesNo);
                if (MsgRes == MessageBoxResult.Yes)
                {
                    Properties.Settings.Default.set_point[SavePoint] = GetNowPoints();
                    Properties.Settings.Default.Save();
                }
            }
        }
        bool CheckEquals(List<Point> temp1, List<Point> temp2)
        {
            foreach(PointList list in Enum.GetValues(typeof(PointList)))
            {
                if (!temp1[(int)list].Equals(temp2[(int)list]))
                    return false;
            }
            return true;
        }
        
        List<Point> GetNowPoints()
        {
            List<Point> points = new List<Point>();
            Point left_top = new Point(Convert.ToInt32(screen_lefttop_x.Text), Convert.ToInt32(screen_lefttop_y.Text));
            Point right_bottom = new Point(Convert.ToInt32(screen_rightbottom_x.Text), Convert.ToInt32(screen_rightbottom_y.Text));
            Point click_pos = new Point(Convert.ToInt32(txt_mouse_click_posX.Text), Convert.ToInt32(txt_mouse_click_posY.Text));

            points.Add(left_top);
            points.Add(right_bottom);
            points.Add(click_pos);

            return points;
        }

        private void mWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ChangePoints();
            Properties.Settings.Default.save_point_num = SavePoint;
            Properties.Settings.Default.Save();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePoints();
            SavePoint = Convert.ToInt32((sender as RadioButton).Tag);
            LoadPoints(Properties.Settings.Default.set_point[SavePoint]);
            screen_lefttop_x.Focus();
        }
    }


}
