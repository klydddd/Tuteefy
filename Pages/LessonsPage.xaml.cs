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

        // FIX: Removed 'static'. Now it is a normal instance variable.
        public string CurrentUserID;

        private string UserRole;

        public LessonsPage(string userID, string role)
        {
            InitializeComponent();

            CurrentUserID = userID;
            UserRole = role;
            db = new Database();

            // Hide Create Button for Tutees
            if (UserRole == "Tutee")
            {
                CreateLessonButton.Visibility = Visibility.Collapsed;
            }

            LoadLessons();
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

            CreateLessonButton.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public void LoadLessons()
        {
            LessonsPanel.Children.Clear();

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "";

                    // --- LOGIC CHANGE ---
                    // We now use different queries depending on the role.

                    if (UserRole == "Tutor")
                    {
                        // Tutor: Show lessons they CREATED
                        query = "SELECT LessonID, Title, Content, Code, FileName FROM LessonsTable WHERE TutorID = @userId";
                    }
                    else
                    {
                        // Tutee: Show lessons ASSIGNED to them (via the join table)
                        query = @"SELECT L.LessonID, L.Title, L.Content, L.Code, L.FileName 
                                  FROM LessonsTable L
                                  INNER JOIN LessonAssignmentsTable A ON L.LessonID = A.LessonID
                                  WHERE A.TuteeID = @userId";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@userId", CurrentUserID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        // Debug check (Optional)
                        if (!reader.HasRows && UserRole == "Tutee")
                        {
                            // MessageBox.Show("No lessons found for this student.");
                        }

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
                                FileName = fileName,
                                UserRole = this.UserRole,
                                CurrentUserID = this.CurrentUserID
                            };

                            LessonsPanel.Children.Add(card);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading lessons: " + ex.Message);
                }
            }
        }
        private void CreateLessonButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new TuteefyWPF.WindowsFolder.LessonsWindows.AddLessonsWindow(CurrentUserID);

            addWindow.LessonCreated += (s, args) =>
            {
                LoadLessons();
            };

            TuteefyWPF.Classes.WindowHelper.ShowDimmedDialog(Window.GetWindow(this), addWindow);
        }
    }
}