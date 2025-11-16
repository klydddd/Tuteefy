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
using TuteefyWPF; // This gives access to StudentCard

namespace TuteefyWPF
{
    /// <summary>
    /// Interaction logic for StudentCards.xaml
    /// </summary>
    public partial class StudentCards : Page
    {
        private System.Windows.Threading.DispatcherTimer scrollTimer;

        public StudentCards()
        {
            InitializeComponent();
            InitializeScrollAnimation();
            AddStudentCard("Alex Cruz", "Mathematics", "92");
            AddStudentCard("Bianca Santos", "Physics", "88");
            AddStudentCard("Carlos Lim", "English", "95");
            AddStudentCard("Alex Cruz", "Mathematics", "92");
            AddStudentCard("Bianca Santos", "Physics", "88");
            AddStudentCard("Carlos Lim", "English", "95");
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
            CardScrollViewer.ScrollChanged += CardScrollViewer_ScrollChanged;
        }

        private void CardScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
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

            AddStudentButton.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void AddStudentCard(string name, string subject, string grade)
        {
            // Create a new instance of your UserControl
            var card = new StudentCardControl
            {
                StudentName = name,
                Subject = subject,
                TotalGrade = grade
            };
            // Add it to your WrapPanel
            StudentCardsPanel.Children.Add(card);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                double scrollAmount = e.Delta > 0 ? -50 : 50; // adjust scroll speed
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
                e.Handled = true;
            }
        }

        private void RefreshStudentCards()
        {
            // Clear existing cards
            StudentCardsPanel.Children.Clear();

            // Reload cards from the static Students list
            foreach (var student in WindowsFolder.StudentWindows.AddStudentWindow.Students)
            {
                AddStudentCard(student.FullName, "Subject", "Grade");
            }
        }

        private void AddStudentButton_Click(object sender, RoutedEventArgs e)
        {

            var addWindow = new TuteefyWPF.WindowsFolder.StudentWindows.AddStudentWindow();
            bool? result = TuteefyWPF.Classes.WindowHelper.ShowDimmedDialog(Window.GetWindow(this), addWindow);
            

            if (result == true)
            {
                // Add the newly enrolled student to the UI
                var lastStudent = TuteefyWPF.WindowsFolder.StudentWindows.AddStudentWindow.Students[
                    TuteefyWPF.WindowsFolder.StudentWindows.AddStudentWindow.Students.Count - 1
                ];
                AddStudentCard(lastStudent.FullName, addWindow.SubjectTxtBox.Text, "N/A");
            }
        }



    }


}