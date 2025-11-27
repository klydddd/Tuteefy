using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TuteefyWPF.Pages
{
    public partial class StudentPage : Page
    {
        public SeriesCollection SeriesCollection { get; set; }
        public List<string> Labels { get; set; }

        private Database db = new Database();
        private string currentTuteeID;

        // Helper class to store Task Data
        public class ToDoItem
        {
            public int TaskID { get; set; }
            public string Content { get; set; }
        }

        public StudentPage(string tuteeID, string name)
        {
            InitializeComponent();

            currentTuteeID = tuteeID;
            txtGreet.Text = "Welcome, " + name + "!";

            SeriesCollection = new SeriesCollection();
            Labels = new List<string>();

            DataContext = this;

            // Load Data
            LoadMyPerformance();
            LoadTasks();
        }

        // --- TO-DO LIST LOGIC ---

        private void LoadTasks()
        {
            ToDoListBox.Items.Clear();
            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();

                    // FIX 1: Added [dbo] schema to force SQL to look in the right place
                    string query = "SELECT TaskID, TaskContent FROM [dbo].[ToDoTable] WHERE TuteeID = @tid ORDER BY TaskID DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tid", currentTuteeID);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ToDoListBox.Items.Add(new ToDoItem
                                {
                                    TaskID = (int)reader["TaskID"],
                                    Content = reader["TaskContent"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // FIX 2: Better error message to help you debug the file copy issue
                MessageBox.Show($"Error loading tasks: {ex.Message}\n\n" +
                    "NOTE: If the table exists in Server Explorer but not here, Visual Studio might be overwriting your DB file in the 'bin/Debug' folder.\n" +
                    "Try changing the DB file property 'Copy to Output Directory' to 'Copy if newer'.");
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string content = TaskInputBox.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();
                    // FIX 1: Added [dbo] schema
                    string query = "INSERT INTO [dbo].[ToDoTable] (TuteeID, TaskContent) VALUES (@tid, @content)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tid", currentTuteeID);
                        cmd.Parameters.AddWithValue("@content", content);
                        cmd.ExecuteNonQuery();
                    }
                }

                TaskInputBox.Text = "";
                LoadTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding task: " + ex.Message);
            }
        }

        private void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            if (ToDoListBox.SelectedItem is ToDoItem selectedItem)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(db.connectionString))
                    {
                        conn.Open();
                        // FIX 1: Added [dbo] schema
                        string query = "DELETE FROM [dbo].[ToDoTable] WHERE TaskID = @id";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", selectedItem.TaskID);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadTasks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error removing task: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a task to remove.");
            }
        }

        // --- NAVIGATION ---

        private void GoToLessons_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new LessonsPage(currentTuteeID, "Tutee"));
        }

        // --- PERFORMANCE CHART ---

        private void LoadMyPerformance()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT Q.Title, R.Score 
                        FROM QuizResultsTable R
                        JOIN QuizzesTable Q ON R.QuizID = Q.QuizID
                        WHERE R.TuteeID = @tid
                        ORDER BY R.DateTaken ASC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tid", currentTuteeID);

                        ChartValues<double> myScores = new ChartValues<double>();
                        Labels.Clear();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string title = reader["Title"].ToString();
                                double score = Convert.ToDouble(reader["Score"]);
                                Labels.Add(title);
                                myScores.Add(score);
                            }
                        }

                        if (myScores.Count == 0)
                        {
                            Labels.Add("No Data");
                            myScores.Add(0);
                        }

                        SeriesCollection.Add(new LineSeries
                        {
                            Title = "My Score",
                            Values = myScores,
                            PointGeometry = DefaultGeometries.Circle,
                            PointGeometrySize = 10,
                            Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9600ff")),
                            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#229600ff"))
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading performance: " + ex.Message);
            }
        }
    }
}