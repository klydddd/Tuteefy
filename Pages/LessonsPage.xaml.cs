using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TuteefyWPF.UserControls;

namespace TuteefyWPF.Pages
{
    public partial class LessonsPage : Page
    {
        private System.Windows.Threading.DispatcherTimer scrollTimer;

        public LessonsPage()
        {
            InitializeComponent();
            LoadLessons();
            InitializeScrollAnimation();
        }

        private void InitializeScrollAnimation()
        {
            // Timer to restore button opacity after scrolling stops
            scrollTimer = new System.Windows.Threading.DispatcherTimer();
            scrollTimer.Interval = TimeSpan.FromMilliseconds(300);
            scrollTimer.Tick += (s, e) =>
            {
                scrollTimer.Stop();
                AnimateButtonOpacity(1.0);
            };

            // Subscribe to scroll changed event
            MainScrollViewer.ScrollChanged += MainScrollViewer_ScrollChanged;
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                // User is scrolling - reduce opacity
                AnimateButtonOpacity(0.3);

                // Reset timer
                scrollTimer.Stop();
                scrollTimer.Start();
            }
        }

        private void AnimateButtonOpacity(double toOpacity)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                To = toOpacity,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            CreateLessonButton.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void LoadLessons()
        {
            // Add sample lessons and quizzes
            AddLessonCard("Introduction to Algebra", "MATH101");
            AddLessonCard("Basic Chemistry", "CHEM001");
            AddLessonCard("Physics Quiz 1", "PHY-Q01");
            AddLessonCard("World History", "HIST201");
            AddLessonCard("Math Final Exam", "MATH-FE");
            AddLessonCard("English Literature", "ENG301");
            AddLessonCard("Biology Basics", "BIO101");
            AddLessonCard("Computer Science", "CS101");
        }

        private void AddLessonCard(string title, string code)
        {
            var card = new QuizAndLessonCard
            {
                Title = title,
                Code = code
            };

            LessonsPanel.Children.Add(card);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                double scrollAmount = e.Delta > 0 ? -50 : 50;
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
                e.Handled = true;
            }
        }

        private void CreateLessonButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to create lesson page or open a dialog
            MessageBox.Show("Navigate to Create Lesson page", "Create Lesson");

            // Example navigation:
            // NavigationService?.Navigate(new CreateLessonPage());
        }
    }
}