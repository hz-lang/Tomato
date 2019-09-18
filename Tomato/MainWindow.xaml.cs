using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Tomato
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _running;
        private bool _start;
        private Stopwatch _watch;

        public MainWindow()
        {
            InitializeComponent();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var status = Status.Work;
                var count = 0;

                _running = true;
                _start = false;
                _watch = new Stopwatch();

                while (_running)
                {
                    if (!_start)
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    if (!_watch.IsRunning)
                    {
                        _watch.Start();
                    }

                    Thread.Sleep(200);

                    _watch.Stop();

                    var period = GetPeriod(status) - _watch.Elapsed;
                    Dispatcher.Invoke(() =>
                    {
                        _Title.Text = GetDescription(status);
                        _Count.Text = count.ToString();
                        _Timer.Text = period.ToString("mm\\:ss");
                    });

                    if (period <= TimeSpan.Zero)
                    {
                        _watch.Reset();

                        if (status != Status.Work)
                        {
                            status = Status.Work;

                            Dispatcher.Invoke(() =>
                            {
                                Foreground = Brushes.Black;
                                Background = Brushes.White;
                                WindowState = WindowState.Normal;
                            });
                        }
                        else
                        {
                            count++;

                            if (count % 4 == 0)
                            {
                                status = Status.LongBreak;
                            }
                            else
                            {
                                status = Status.ShortBreak;
                            }

                            Dispatcher.Invoke(() =>
                            {
                                Foreground = Brushes.White;
                                Background = Brushes.Green;
                                WindowState = WindowState.Maximized;
                            });
                        }
                    }
                }
            });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _running = false;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Root_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _start = !_start;
        }

        private string GetDescription(Status status)
        {
            if (status == Status.Work)
                return "工作";
            else
                return "休息";
        }

        private TimeSpan GetPeriod(Status status)
        {
            switch (status)
            {
                case Status.Work:
                    return TimeSpan.FromMinutes(25);
                    //return TimeSpan.FromSeconds(3);
                case Status.ShortBreak:
                    return TimeSpan.FromMinutes(5);
                    //return TimeSpan.FromSeconds(5);
                case Status.LongBreak:
                    return TimeSpan.FromMinutes(15);
                    //return TimeSpan.FromSeconds(10);
                default:
                    throw new NotSupportedException("不支持的状态：" + status);
            }
        }

        private enum Status
        {
            Work, ShortBreak, LongBreak
        }
    }
}
