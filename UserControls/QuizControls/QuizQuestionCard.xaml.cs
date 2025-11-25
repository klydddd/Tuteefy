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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TuteefyWPF.UserControls
{
    /// <summary>
    /// Interaction logic for QuizQuestionCard.xaml
    /// </summary>
    public partial class QuizQuestionCard : UserControl
    {
        private Database db = new Database();
        private string quizID = string.Empty; 
        public QuizQuestionCard(string id)
        {
            InitializeComponent();
            quizID = id;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the parent StackPanel (QuizQuestionsContainer)
            var parent = FindParent<StackPanel>(this);
            // Find the parent Page to access the tracker panel
            var quizViewPage = FindParent<Page>(this);

            if (parent != null && quizViewPage is TuteefyWPF.Pages.QuizPages.QuizView quizView)
            {
                // Get the question number from Tag
                int questionNumber = (int)this.Tag;

                // Remove this card from the parent
                parent.Children.Remove(this);

                // Find and remove the corresponding tracker item
                RemoveTrackerItem(questionNumber, quizView);

                // --- NEW CODE ---
                // After removing, re-number all remaining cards and trackers
                RenumberQuestions(parent, quizView);
            }
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate that all fields are filled
            if (string.IsNullOrWhiteSpace(QuestionTextBox.Text))
            {
                MessageBox.Show("Please enter a question.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ChoiceATextBox.Text) ||
                string.IsNullOrWhiteSpace(ChoiceBTextBox.Text) ||
                string.IsNullOrWhiteSpace(ChoiceCTextBox.Text) ||
                string.IsNullOrWhiteSpace(ChoiceDTextBox.Text))
            {
                MessageBox.Show("Please fill in all answer choices.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- BUG FIX ---
            // The "Done" button should only confirm the current card is complete.
            // ALL code that was here to add a new question has been REMOVED.

            string GetNextQuestionID(SqlConnection conn)
            {
                string query = "SELECT TOP 1 QuestionID FROM QuestionsTable " +
                                "WHERE QuizID = '" + quizID + "' " +
                                 "ORDER BY QuestionID DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                var lastId = cmd.ExecuteScalar() as string;

                if (string.IsNullOrEmpty(lastId))
                    return quizID + "-Q001";

                int num = int.Parse(lastId.Substring(12));
                return quizID + "-Q" + (num + 1).ToString("D3");
            }

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                string questionID = GetNextQuestionID(conn);

                try
                {
                    string insertQuestion = @"INSERT INTO QuestionsTable (QuestionID, QuizID, QuestionText)
                                    VALUES (@QuestionID, @QuizID, @QuestionText)";

                    using (SqlCommand cmd = new SqlCommand(insertQuestion, conn))
                    {
                        cmd.Parameters.AddWithValue("@QuestionID", questionID);
                        cmd.Parameters.AddWithValue("@QuizID", quizID);
                        cmd.Parameters.AddWithValue("@QuestionText", QuestionTextBox.Text);
                        cmd.ExecuteNonQuery();
                    }

                    string GetNextChoiceID(SqlConnection conn2)
                    {
                        string query = "SELECT TOP 1 ChoiceID FROM ChoicesTable " +
                                        "WHERE QuestionID = '" + questionID + "' " +
                                         "ORDER BY ChoiceID DESC";
                        SqlCommand cmd = new SqlCommand(query, conn2);
                        var lastId = cmd.ExecuteScalar() as string;

                        if (string.IsNullOrEmpty(lastId))
                            return questionID + "-C1";

                        int num = int.Parse(lastId.Substring(17));
                        return questionID + "-C" + (num + 1).ToString();
                    }

                    string insertChoices = @"INSERT INTO ChoicesTable (ChoiceID, QuestionID, ChoiceText)
                                    VALUES (@ChoiceID, @QuestionID, @ChoiceText)";

                    string[] choiceTexts = new string[]
                    {
                        ChoiceATextBox.Text,
                        ChoiceBTextBox.Text,
                        ChoiceCTextBox.Text,
                        ChoiceDTextBox.Text
                    };

                    foreach (string text in choiceTexts)
                    {
                        string choiceID = GetNextChoiceID(conn);

                        using (SqlCommand cmd = new SqlCommand(insertChoices, conn))
                        {
                            cmd.Parameters.AddWithValue("@ChoiceID", choiceID);
                            cmd.Parameters.AddWithValue("@QuestionID", questionID);
                            cmd.Parameters.AddWithValue("@ChoiceText", text);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    this.QuestionTextBox.IsEnabled = false;
                    this.ChoiceATextBox.IsEnabled = false;
                    this.ChoiceBTextBox.IsEnabled = false;
                    this.ChoiceCTextBox.IsEnabled = false;
                    this.ChoiceDTextBox.IsEnabled = false;
                    this.DoneButton.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }

            }
        }
                

        // Helper method to find parent control of a specific type
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
                return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindParent<T>(parentObject);
        }

        private void RemoveTrackerItem(int questionNumber, TuteefyWPF.Pages.QuizPages.QuizView quizView)
        {
            // Access the QuestionTrackerPanel
            var trackerPanel = quizView.FindName("QuestionTrackerPanel") as StackPanel;

            if (trackerPanel != null)
            {
                // Find and remove the tracker item with matching Tag
                var trackerItem = trackerPanel.Children
                    .OfType<TextBlock>()
                    .FirstOrDefault(tb => tb.Tag is int tag && tag == questionNumber);

                if (trackerItem != null)
                {
                    trackerPanel.Children.Remove(trackerItem);
                }
            }
        }

        // --- NEW METHOD ---
        // This method re-syncs all question numbers after one is deleted.
        private void RenumberQuestions(StackPanel cardContainer, TuteefyWPF.Pages.QuizPages.QuizView quizView)
        {
            var trackerPanel = quizView.FindName("QuestionTrackerPanel") as StackPanel;
            if (trackerPanel == null) return;

            int newNumber = 1;

            // Renumber all the question cards in the main list
            foreach (var card in cardContainer.Children.OfType<QuizQuestionCard>())
            {
                card.Tag = newNumber;
                newNumber++;
            }

            newNumber = 1;
            // Renumber all the trackers in the side panel
            foreach (var tracker in trackerPanel.Children.OfType<TextBlock>())
            {
                tracker.Tag = newNumber;
                tracker.Text = $"Question {newNumber}"; // Update text
                newNumber++;
            }

            // Optional: Update the main counter on the QuizView page itself
            // This part is more complex and requires passing data back up,
            // but for the UI, this will fix the numbering.
        }

        // This method is no longer needed because the buggy DoneButton logic is gone.
        // private void AddQuestionTracker(int questionNumber) { ... }


        private void QuestionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // You can add logic here if you want
        }

        private void CorrectBCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}