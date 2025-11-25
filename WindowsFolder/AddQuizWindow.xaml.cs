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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TuteefyWPF.WindowsFolder
{
    /// <summary>
    /// Interaction logic for AddQuizWindow.xaml
    /// </summary>
    public partial class AddQuizWindow : Window
    {
        private Database db = new Database();
        private string username = string.Empty;

        public AddQuizWindow(string tutorID)
        {
            InitializeComponent();
            username = tutorID;
        }

        private void CreateQuizButton_Click(object sender, RoutedEventArgs e)
        {
            TuteefyWPF.Classes.WindowHelper.UndimDialog(this.Owner);

            // Navigate to QuizView in the main window
            if (this.Owner is TuteefyMain mainWindow)
            {
                string GetNextQuizID(SqlConnection conn)
                {
                    string query = "SELECT TOP 1 QuizID FROM QuizzesTable " +
                                    "WHERE TutorID = '" + username + "' " +
                                     "ORDER BY QuizID DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    var lastId = cmd.ExecuteScalar() as string;

                    if (string.IsNullOrEmpty(lastId))
                        return username + "-Q001";

                    int num = int.Parse(lastId.Substring(7));
                    return username + "-Q" + (num + 1).ToString("D3");
                }

                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();

                    string quizID = GetNextQuizID(conn);

                    try
                    {
                        string insertQuiz = @"INSERT INTO QuizzesTable (QuizID, TutorID, Title, Description)
                                    VALUES (@QuizID, @TutorID, @Title, @Description)";

                        using (SqlCommand cmd = new SqlCommand(insertQuiz, conn))
                        {
                            cmd.Parameters.AddWithValue("@QuizID", quizID);
                            cmd.Parameters.AddWithValue("@TutorID", username);
                            cmd.Parameters.AddWithValue("@Title", QuizTitleTextBox.Text);
                            cmd.Parameters.AddWithValue("@Description", QuizDesc.Text);
                            cmd.ExecuteNonQuery();
                        }

                        // Create the QuizView page with the data
                        TuteefyWPF.Pages.QuizPages.QuizView quizViewPage = new TuteefyWPF.Pages.QuizPages.QuizView(quizID, username);
                        quizViewPage.QuizTitle.Content = QuizTitleTextBox.Text;
                        quizViewPage.QuizDesc.Content = QuizDesc.Text;

                        // Navigate to the created page instance
                        mainWindow.MainFrame.Navigate(quizViewPage);

                        // Close the window
                        this.DialogResult = true;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }

            }

        }
    }
}
