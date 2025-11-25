using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using TuteefyWPF.UserControls.QuizControls;

namespace TuteefyWPF.Pages
{
    public partial class QuizPage : Page
    {
        private System.Windows.Threading.DispatcherTimer scrollTimer;
        private string username = string.Empty;
        private Database db = new Database();

        public QuizPage(string userRole, string tutorID)
        {
            InitializeComponent();
            if (userRole == "Tutee")
            {
                CreateQuizButton.Visibility = Visibility.Collapsed;
            }
            username = tutorID;
            LoadQuizzes();
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

            CreateQuizButton.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void LoadQuizzes()
        {
            QuizzesPanel.Children.Clear();

            string query = @"SELECT QuizID, Title
                         FROM QuizzesTable
                         WHERE TutorID = @tutorId";

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tutorId", username);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string title = reader["Title"].ToString();
                        string quizID = reader["QuizID"].ToString();

                        AddQuizCard(title, quizID);
                    }
                }
            }
        }

        private void AddQuizCard(string title, string code)
        {
            var card = new UserControls.QuizControls.QuizCard
            {
                Title = title,
                Code = code,
                TutorID = username
            };

            QuizzesPanel.Children.Add(card);
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

        private void CreateQuizButton_Click(object sender, RoutedEventArgs e)
        {
            TuteefyWPF.Classes.WindowHelper windowHelper = new TuteefyWPF.Classes.WindowHelper();
            var addWindow = new TuteefyWPF.WindowsFolder.AddQuizWindow(username);
            TuteefyWPF.Classes.WindowHelper.ShowDimmedDialog(Window.GetWindow(this), addWindow);
            // Example navigation:
            // NavigationService?.Navigate(new CreateQuizPage());
        }
    }
}