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
using System.Data.SqlClient;
namespace TuteefyWPF.Pages
{
    public partial class LessonsPage : Page
    {
        private System.Windows.Threading.DispatcherTimer scrollTimer;
        private Database db;
        public static string CurrentTutorID;
        

        public LessonsPage(string tutorID)
        {
            CurrentTutorID = tutorID;

            InitializeComponent();
            db = new Database();
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

        public void LoadLessons()
        {
            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT LessonID, Title, Content, Code, FileName FROM LessonsTable WHERE TutorID = @CurrentTutorID";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@CurrentTutorID", CurrentTutorID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int lessonId = reader["LessonID"] == DBNull.Value ? 0 : Convert.ToInt32(reader["LessonID"]);
                            string title = reader["Title"].ToString();
                            string content = reader["Content"].ToString();
                            string code = reader["Code"].ToString();
                            string fileName = reader["FileName"] == DBNull.Value ? string.Empty : reader["FileName"].ToString();

                            QuizAndLessonCard card = new QuizAndLessonCard
                            {
                                Title = title,
                                Code = code,
                                LessonContent = content,
                                LessonId = lessonId,
                                FileName = fileName
                            };

                            LessonsPanel.Children.Add(card);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        // Old way of adding LessonCard
        private void AddLessonCard(string title, string code)
        {
            var card = new QuizAndLessonCard
            {
                Title = title,
                Code = code
            };

            LessonsPanel.Children.Add(card);
        }

        private void CreateLessonButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new TuteefyWPF.WindowsFolder.LessonsWindows.AddLessonsWindow(CurrentTutorID);

            // Subscribe to the LessonCreated event
            addWindow.LessonCreated += (s, args) =>
            {
                // Clear existing lessons and reload
                LessonsPanel.Children.Clear();
                LoadLessons();
            };

            TuteefyWPF.Classes.WindowHelper.ShowDimmedDialog(Window.GetWindow(this), addWindow);
        }
    }
}