using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace BankApp
{
    public partial class SplashWindow : Window
    {
        private const double SplashDurationSeconds = 3.0;

        public SplashWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AnimateProgressBar();
            StartCloseTimer();
        }

        /// <summary>
        /// Animates the progress fill bar from 0 to full width over SplashDurationSeconds.
        /// </summary>
        private void AnimateProgressBar()
        {
            // The progress fill Width starts at 0 and needs to animate to the
            // actual container width. We bind to ActualWidth of the parent via
            // a DoubleAnimation targeting the ProgressFill Border.
            double targetWidth = ActualWidth - 80; // 40px margin each side
            if (targetWidth <= 0) targetWidth = 720;

            var animation = new DoubleAnimation
            {
                From           = 0,
                To             = targetWidth,
                Duration       = TimeSpan.FromSeconds(SplashDurationSeconds),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            ProgressFill.BeginAnimation(WidthProperty, animation);
        }

        /// <summary>
        /// Closes this window after SplashDurationSeconds and shows the main window.
        /// </summary>
        private void StartCloseTimer()
        {
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(SplashDurationSeconds)
            };

            timer.Tick += (s, e) =>
            {
                timer.Stop();

                // Fade out before closing
                var fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(300));
                fadeOut.Completed += (_, __) =>
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                };
                BeginAnimation(OpacityProperty, fadeOut);
            };

            timer.Start();
        }
    }
}
