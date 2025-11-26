using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TuteefyWPF.UserControls
{
    public partial class QuizQuestionCard : UserControl
    {
        private Database db = new Database();
        private string quizID = string.Empty;

        // Helper class to store choice info temporarily
        private class ChoiceData
        {
            public string Text { get; set; }
            public bool IsCorrect { get; set; } // This will become 1 or 0 in SQL
        }

        public QuizQuestionCard(string id)
        {
            InitializeComponent();
            quizID = id;
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var parent = FindParent<StackPanel>(this);
            var quizViewPage = FindParent<Page>(this);

            if (parent != null && quizViewPage is TuteefyWPF.Pages.QuizPages.QuizView quizView)
            {
                int questionNumber = (int)this.Tag;
                parent.Children.Remove(this);
                RemoveTrackerItem(questionNumber, quizView);
                RenumberQuestions(parent, quizView);
            }
        }

        public void SaveQuestionToDatabase()
        {
            // 1. Validation
            if (string.IsNullOrWhiteSpace(QuestionTextBox.Text) ||
                string.IsNullOrWhiteSpace(ChoiceATextBox.Text) ||
                string.IsNullOrWhiteSpace(ChoiceBTextBox.Text) ||
                string.IsNullOrWhiteSpace(ChoiceCTextBox.Text) ||
                string.IsNullOrWhiteSpace(ChoiceDTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields before saving.", "Validation Error");
                return;
            }

            // 2. Validate that at least ONE correct answer is selected
            if (CorrectARadioButton.IsChecked != true &&
                CorrectBRadioButton.IsChecked != true &&
                CorrectCRadioButton.IsChecked != true &&
                CorrectDRadioButton.IsChecked != true)
            {
                MessageBox.Show("Please select which answer is the correct one.", "Validation Error");
                return;
            }

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                // --- Generate Question ID ---
                string GetNextQuestionID()
                {
                    string query = "SELECT TOP 1 QuestionID FROM QuestionsTable WHERE QuizID = @qid ORDER BY QuestionID DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@qid", quizID);
                        var lastId = cmd.ExecuteScalar() as string;

                        if (string.IsNullOrEmpty(lastId)) return quizID + "-Q001";

                        try
                        {
                            string numberPart = lastId.Substring(lastId.LastIndexOf("-Q") + 2);
                            int num = int.Parse(numberPart);
                            return quizID + "-Q" + (num + 1).ToString("D3");
                        }
                        catch { return quizID + "-Q" + Guid.NewGuid().ToString().Substring(0, 4); }
                    }
                }

                string questionID = GetNextQuestionID();

                try
                {
                    // --- Insert Question ---
                    string insertQuestion = @"INSERT INTO QuestionsTable (QuestionID, QuizID, QuestionText) 
                                              VALUES (@QuestionID, @QuizID, @QuestionText)";

                    using (SqlCommand cmd = new SqlCommand(insertQuestion, conn))
                    {
                        cmd.Parameters.AddWithValue("@QuestionID", questionID);
                        cmd.Parameters.AddWithValue("@QuizID", quizID);
                        cmd.Parameters.AddWithValue("@QuestionText", QuestionTextBox.Text);
                        cmd.ExecuteNonQuery();
                    }

                    // --- Insert Choices ---
                    // UPDATED: Now checking RadioButtons
                    List<ChoiceData> choices = new List<ChoiceData>
                    {
                        new ChoiceData { Text = ChoiceATextBox.Text, IsCorrect = CorrectARadioButton.IsChecked == true },
                        new ChoiceData { Text = ChoiceBTextBox.Text, IsCorrect = CorrectBRadioButton.IsChecked == true },
                        new ChoiceData { Text = ChoiceCTextBox.Text, IsCorrect = CorrectCRadioButton.IsChecked == true },
                        new ChoiceData { Text = ChoiceDTextBox.Text, IsCorrect = CorrectDRadioButton.IsChecked == true }
                    };

                    string insertChoices = @"INSERT INTO ChoicesTable (ChoiceID, QuestionID, ChoiceText, IsCorrect) 
                                             VALUES (@ChoiceID, @QuestionID, @ChoiceText, @IsCorrect)";

                    for (int i = 0; i < choices.Count; i++)
                    {
                        using (SqlCommand cmd = new SqlCommand(insertChoices, conn))
                        {
                            // Generate Choice ID (e.g., QID-C1, QID-C2...)
                            cmd.Parameters.AddWithValue("@ChoiceID", questionID + "-C" + (i + 1));
                            cmd.Parameters.AddWithValue("@QuestionID", questionID);
                            cmd.Parameters.AddWithValue("@ChoiceText", choices[i].Text);

                            // EXPLICIT LOGIC:
                            // If choices[i].IsCorrect is true, SQL receives 1.
                            // If choices[i].IsCorrect is false, SQL receives 0.
                            cmd.Parameters.AddWithValue("@IsCorrect", choices[i].IsCorrect ? 1 : 0);

                            cmd.ExecuteNonQuery();
                        }
                    }

                    DisableControls();
                    MessageBox.Show("Question Saved!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving question: " + ex.Message);
                }
            }
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            SaveQuestionToDatabase();
        }

        private void DisableControls()
        {
            this.QuestionTextBox.IsEnabled = false;
            this.ChoiceATextBox.IsEnabled = false;
            this.ChoiceBTextBox.IsEnabled = false;
            this.ChoiceCTextBox.IsEnabled = false;
            this.ChoiceDTextBox.IsEnabled = false;

            // Disable RadioButtons
            this.CorrectARadioButton.IsEnabled = false;
            this.CorrectBRadioButton.IsEnabled = false;
            this.CorrectCRadioButton.IsEnabled = false;
            this.CorrectDRadioButton.IsEnabled = false;

            this.DoneButton.IsEnabled = false;
        }

        // Helper Methods
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            T parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        private void RemoveTrackerItem(int questionNumber, TuteefyWPF.Pages.QuizPages.QuizView quizView)
        {
            var trackerPanel = quizView.FindName("QuestionTrackerPanel") as StackPanel;
            if (trackerPanel != null)
            {
                var trackerItem = trackerPanel.Children.OfType<TextBlock>()
                    .FirstOrDefault(tb => tb.Tag is int tag && tag == questionNumber);
                if (trackerItem != null) trackerPanel.Children.Remove(trackerItem);
            }
        }

        private void RenumberQuestions(StackPanel cardContainer, TuteefyWPF.Pages.QuizPages.QuizView quizView)
        {
            var trackerPanel = quizView.FindName("QuestionTrackerPanel") as StackPanel;
            if (trackerPanel == null) return;

            int newNumber = 1;
            foreach (var card in cardContainer.Children.OfType<QuizQuestionCard>())
            {
                card.Tag = newNumber;
                newNumber++;
            }

            newNumber = 1;
            foreach (var tracker in trackerPanel.Children.OfType<TextBlock>())
            {
                tracker.Tag = newNumber;
                tracker.Text = $"Question {newNumber}";
                newNumber++;
            }
        }

        private void QuestionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // You can leave this empty, or add validation logic here later.
        }
    }
}