using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using TuteefyWPF.UserControls.QuizControls;

namespace TuteefyWPF.Pages
{
    public partial class QuizPage : Page
    {
        private System.Windows.Threading.DispatcherTimer scrollTimer;
        private string username = string.Empty;

        // FIX 1: Add this variable so the whole class can see the role
        private string userRole = string.Empty;

        private Database db = new Database();

        public QuizPage(string role, string userID)
        {
            InitializeComponent();

            // FIX 2: Store the incoming 'role' into our class variable
            this.userRole = role;
            this.username = userID;

            if (this.userRole == "Tutee")
            {
                CreateQuizButton.Visibility = Visibility.Collapsed;
            }

            LoadQuizzes();
            InitializeScrollAnimation();
        }

        private void InitializeScrollAnimation()
        {
            scrollTimer = new System.Windows.Threading.DispatcherTimer();
            scrollTimer.Interval = TimeSpan.FromMilliseconds(300);
            scrollTimer.Tick += (s, e) =>
            {
                scrollTimer.Stop();
                AnimateButtonOpacity(1.0);
            };

            MainScrollViewer.ScrollChanged += MainScrollViewer_ScrollChanged;
        }

        private void MainScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange != 0)
            {
                AnimateButtonOpacity(0.3);
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
            try
            {
                QuizzesPanel.Children.Clear();

                // I kept your simplified query for testing. 
                // Once it works, remember to switch back to the version with "AND IsAvailable = 1"
                string query = @"SELECT QuizID, Title
                                 FROM QuizzesTable
                                 WHERE TutorID = @userId 
                                    OR TuteeID = @userId";

                using (SqlConnection conn = new SqlConnection(db.connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", username);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            // MessageBox.Show($"No quizzes found for user: {username}");
                            return;
                        }

                        while (reader.Read())
                        {
                            string title = reader["Title"].ToString();
                            string quizID = reader["QuizID"].ToString();

                            AddQuizCard(title, quizID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quizzes: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddQuizCard(string title, string code)
        {
            var card = new UserControls.QuizControls.QuizCard
            {
                Title = title,
                Code = code,
                TutorID = username,
                UserRole = this.userRole,
                CurrentUserID = this.username // Pass the logged-in user's ID here
            };

            QuizzesPanel.Children.Add(card);
        }

        //private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    if (sender is ScrollViewer scrollViewer)
        //    {
        //        double scrollAmount = e.Delta > 0 ? -50 : 50;
        //        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
        //        e.Handled = true;
        //    }
        //}

        private void CreateQuizButton_Click(object sender, RoutedEventArgs e)
        {
            TuteefyWPF.Classes.WindowHelper windowHelper = new TuteefyWPF.Classes.WindowHelper();
            var addWindow = new TuteefyWPF.WindowsFolder.AddQuizWindow(username);
            TuteefyWPF.Classes.WindowHelper.ShowDimmedDialog(Window.GetWindow(this), addWindow);
        }
    }
}