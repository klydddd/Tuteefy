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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.SqlClient;
using TuteefyWPF.Pages;

namespace TuteefyWPF.WindowsFolder.LessonsWindows
{
    /// <summary>
    /// Interaction logic for AddLessonsWindow.xaml
    /// </summary>
    public partial class AddLessonsWindow : Window
    {
        private string lesson;
        private string code;
        private string content;
        private Database db;

        // Event to notify when a lesson is created
        public event EventHandler LessonCreated;

        public AddLessonsWindow()
        {
            InitializeComponent();
            db = new Database();
        }

        private void CreateLessonButton_Click(object sender, RoutedEventArgs e)
        {
            lesson = LessonTitleTextBox.Text.Trim();
            code = LessonCodeTextBox.Text.Trim();
            content = ContentTextBox.Text.Trim();

            // Validate inputs
            if (string.IsNullOrWhiteSpace(lesson))
            {
                MessageBox.Show("Please enter a lesson title.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                MessageBox.Show("Please enter a lesson code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Please enter lesson content.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO LessonsTable (TutorID, Title, Code, Content) VALUES (@TutorID, @Title, @Code, @Content)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@TutorID", Pages.LessonsPage.CurrentTutorID);
                    cmd.Parameters.AddWithValue("@Title", lesson);
                    cmd.Parameters.AddWithValue("@Code", code);
                    cmd.Parameters.AddWithValue("@Content", content);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        // Trigger the event to notify the parent page
                        LessonCreated?.Invoke(this, EventArgs.Empty);

                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to create lesson. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}