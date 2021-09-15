using System.Windows;
using System.Windows.Threading;

namespace LaserScan.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = new System.TimeSpan(0, 0, 0, 0, 10);
            _timer.Tick += TimerTick;
        }

        #region Methods
        private void TimerTick(object sender, System.EventArgs e)
        {
            if ((bool)menu.IsChecked)
            {
                var oldWidth = sideBarColumn.Width.Value;
                sideBarColumn.Width = new GridLength(oldWidth + 5);
                if (sideBarColumn.Width.Value >= 200)
                {
                    _timer.Stop();
                }

            }
            else
            {
                var oldWidth = sideBarColumn.Width.Value;
                sideBarColumn.Width = new GridLength(oldWidth - 5);
                if (sideBarColumn.Width.Value < 1)
                {
                    _timer.Stop();
                }
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void corner_Click(object sender, RoutedEventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    this.WindowState = WindowState.Maximized;
                    break;
                case WindowState.Maximized:
                    this.WindowState = WindowState.Normal;
                    break;
                default:
                    break;
            }
        }

        private void DockPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void minimize_Click(object sender, RoutedEventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    this.WindowState = WindowState.Minimized;
                    break;
                case WindowState.Maximized:
                    this.WindowState = WindowState.Minimized;
                    break;
                default:
                    break;
            }
        }

        private void menu_Click(object sender, RoutedEventArgs e)
        {
            _timer.Start();
        }
        #endregion
    }
}
