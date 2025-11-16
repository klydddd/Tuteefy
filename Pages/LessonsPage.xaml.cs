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
        private string CurrentTutorID;
        

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

        private void LoadLessons()
        {

            //MessageBox.Show(CurrentTutorID);
            

            using(SqlConnection conn = new SqlConnection(db.connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT Title, Content FROM LessonsTable WHERE TutorID = @CurrentTutorID";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@CurrentTutorID", CurrentTutorID);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read()) 
                        {
                            
                            string Title = reader["Title"].ToString();

                            QuizAndLessonCard card = new QuizAndLessonCard();

                            card.Title = Title;

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
            // Get the parent window (TuteefyMain)
            Window mainWindow = Window.GetWindow(this);

            if (mainWindow != null)
            {
                // Create overlay to dim the main window
                Grid overlay = new Grid
                {
                    Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), // Semi-transparent black
                    Visibility = Visibility.Visible
                };

                // Add overlay to the main window's root grid
                if (mainWindow.Content is Grid mainGrid)
                {
                    mainGrid.Children.Add(overlay);
                }

                // Alternatively, just reduce the window opacity
                double originalOpacity = mainWindow.Opacity;
                mainWindow.Opacity = 0.7; // Dim the main window

                // Create and show the add window
                TuteefyWPF.WindowsFolder.AddWindow addWindow = new TuteefyWPF.WindowsFolder.AddWindow();
                addWindow.Owner = mainWindow; // Set owner so it stays on top
                addWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // When add window closes, restore main window
                addWindow.Closed += (s, args) =>
                {
                    mainWindow.Opacity = originalOpacity; // Restore opacity

                    // Remove overlay if it was added
                    if (mainWindow.Content is Grid grid)
                    {
                        grid.Children.Remove(overlay);
                    }
                };

                addWindow.ShowDialog(); // Use ShowDialog to make it modal
            }
        }
    }
}