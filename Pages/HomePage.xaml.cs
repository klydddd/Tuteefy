using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TuteefyWPF
{
    public partial class HomePage : Page
    {
        // CHANGED: Use SeriesCollection for multiple lines (students)
        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; } // This will hold Quiz Titles
        public DateTime SelectedDate { get; set; }

        private Database db = new Database();
        private string currentUserId;

        public HomePage()
        {
            InitializeComponent();
            InitializeCharts();
        }

        public HomePage(string userId)
        {
            InitializeComponent();
            currentUserId = userId;
            InitializeCharts();

            if (!string.IsNullOrEmpty(currentUserId))
            {
                LoadStudentScores();
            }
        }

        private void InitializeCharts()
        {
            SeriesCollection = new SeriesCollection();
            Labels = new List<string>();
            SelectedDate = DateTime.Today;
            DataContext = this;
        }

        private void LoadStudentScores()
        {
            SeriesCollection.Clear();
            Labels.Clear();

            // Data structure to hold results: 
            // Dictionary<StudentName, Dictionary<QuizTitle, Score>>
            var dataMap = new Dictionary<string, Dictionary<string, double>>();
            var allQuizTitles = new HashSet<string>(); // To keep unique list of quizzes

            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT T.FullName, Q.Title, R.Score
                        FROM QuizResultsTable R
                        JOIN QuizzesTable Q ON R.QuizID = Q.QuizID
                        JOIN TuteeTable T ON R.TuteeID = T.TuteeID
                        WHERE T.TutorID = @tutorId
                        ORDER BY Q.Title"; // Order helps keep X-axis consistent

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tutorId", currentUserId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string studentName = reader["FullName"].ToString();
                                string quizTitle = reader["Title"].ToString();
                                double score = Convert.ToDouble(reader["Score"]);

                                // Add to master list of quizzes
                                allQuizTitles.Add(quizTitle);

                                // Organize data by student
                                if (!dataMap.ContainsKey(studentName))
                                {
                                    dataMap[studentName] = new Dictionary<string, double>();
                                }
                                dataMap[studentName][quizTitle] = score;
                            }
                        }
                    }
                }

                // 1. Set up the X-Axis (Labels)
                Labels.AddRange(allQuizTitles);

                if (Labels.Count == 0)
                {
                    Labels.Add("No Data");
                    return;
                }

                // 2. Create a LineSeries for EACH student
                foreach (var studentEntry in dataMap)
                {
                    string studentName = studentEntry.Key;
                    var scores = studentEntry.Value;

                    ChartValues<double> studentValues = new ChartValues<double>();

                    // IMPORTANT: We must align scores to the Labels list.
                    // If a student missed "Quiz 1", we put Double.NaN (gap in line) or 0.
                    foreach (var quizTitle in Labels)
                    {
                        if (scores.ContainsKey(quizTitle))
                        {
                            studentValues.Add(scores[quizTitle]);
                        }
                        else
                        {
                            // Use double.NaN to break the line, or 0 to show a fail
                            studentValues.Add(double.NaN);
                        }
                    }

                    // Add the line to the chart
                    SeriesCollection.Add(new LineSeries
                    {
                        Title = studentName, // This shows in the Legend
                        Values = studentValues,
                        PointGeometry = DefaultGeometries.Circle,
                        PointGeometrySize = 10,
                        LineSmoothness = 0 // 0 = Straight lines, 1 = Curvy
                        // Fill = Brushes.Transparent // Uncomment if you don't want area shading
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading chart: " + ex.Message);
            }
        }

        private void Calendar_Loaded(object sender, RoutedEventArgs e) { }
    }
}