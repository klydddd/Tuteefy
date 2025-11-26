using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TuteefyWPF.UserControls;
using TuteefyWPF.UserControls.QuizControls;

namespace TuteefyWPF.Pages.QuizPages
{
    /// <summary>
    /// Interaction logic for QuizView.xaml
    /// </summary>
    public partial class QuizView : Page
    {
        private int questionCounter = 1;
        private Database db = new Database();
        private string quizID = string.Empty;
        private string tutorID = string.Empty;
        private string userRole = string.Empty;
        private string currentUserId = string.Empty;
        private string assignedTuteeId = null;

        // Stores [ChoiceID, IsCorrect] so we can grade without asking the database again
        private Dictionary<string, bool> _gradingKey = new Dictionary<string, bool>();

        public QuizView(string id, string tutorId, string role, string userId)
        {
            InitializeComponent();
            quizID = id;
            tutorID = tutorId;
            userRole = role;
            currentUserId = userId;

            if (userRole == "Tutee")
            {
                // Hide Tutor controls
                AddQuestionButton.Visibility = Visibility.Collapsed;
                AssignStudentComboBox.Visibility = Visibility.Collapsed;
                if (AssignText != null) AssignText.Visibility = Visibility.Collapsed;

                // SHOW the Finish button, but change text to "Submit"
                FinishButton.Visibility = Visibility.Visible;
                FinishButton.Content = "Submit Quiz";
            }

            LoadQuizData();
            CheckIfAlreadyTaken();
        }

        private void CheckIfAlreadyTaken()
        {
            if (userRole != "Tutee") return;

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM QuizResultsTable WHERE QuizID = @qid AND TuteeID = @uid";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@qid", quizID);
                    cmd.Parameters.AddWithValue("@uid", currentUserId);
                    int count = (int)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        LockQuizUI();
                        FinishButton.Content = "Already Completed";
                    }
                }
            }
        }

        private void LoadQuizData()
        {
            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                // 1. Load Header Info
                using (SqlCommand cmd = new SqlCommand("SELECT Title, Description, TuteeID FROM QuizzesTable WHERE QuizID = @qid", conn))
                {
                    cmd.Parameters.AddWithValue("@qid", quizID);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            QuizTitle.Content = r["Title"]?.ToString() ?? "";
                            QuizDesc.Content = r["Description"]?.ToString() ?? "";
                            assignedTuteeId = r["TuteeID"] != DBNull.Value ? r["TuteeID"].ToString() : null;
                        }
                    }
                }

                // 2. Load Student Dropdown (Only if Tutor)
                if (userRole != "Tutee")
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT TuteeID, FullName FROM TuteeTable WHERE TutorID = @tutor", conn))
                    {
                        cmd.Parameters.AddWithValue("@tutor", tutorID);
                        using (var r = cmd.ExecuteReader())
                        {
                            AssignStudentComboBox.Items.Clear();
                            while (r.Read())
                            {
                                var id = r["TuteeID"].ToString();
                                var name = r["FullName"].ToString();
                                var item = new ComboBoxItem { Tag = id, Content = name };
                                AssignStudentComboBox.Items.Add(item);
                                if (assignedTuteeId != null && id == assignedTuteeId)
                                    AssignStudentComboBox.SelectedItem = item;
                            }
                        }
                    }
                }

                // 3. Load Questions + Choices
                const string sql = @"
            SELECT q.QuestionID, q.QuestionText, c.ChoiceID, c.ChoiceText, c.IsCorrect
            FROM QuestionsTable q
            LEFT JOIN ChoicesTable c ON q.QuestionID = c.QuestionID
            WHERE q.QuizID = @qid
            ORDER BY q.QuestionID";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@qid", quizID);
                    using (var r = cmd.ExecuteReader())
                    {
                        QuizQuestionsContainer.Children.Clear();
                        QuestionTrackerPanel.Children.Clear();
                        _gradingKey.Clear();

                        // Reset local counter for loading
                        int localCounter = 1;

                        string currentQuestionId = null;
                        Border currentBorder = null;
                        StackPanel currentStack = null;
                        StackPanel currentChoicesPanel = null;

                        while (r.Read())
                        {
                            var qid = r["QuestionID"]?.ToString();
                            var qtext = r["QuestionText"]?.ToString();

                            // --- NEW QUESTION BLOCK ---
                            if (currentQuestionId == null || !string.Equals(currentQuestionId, qid, StringComparison.Ordinal))
                            {
                                if (currentBorder != null)
                                {
                                    currentBorder.Child = currentStack;
                                    QuizQuestionsContainer.Children.Add(currentBorder);
                                }

                                currentQuestionId = qid;

                                // Card Design
                                currentBorder = new Border
                                {
                                    CornerRadius = new CornerRadius(15),
                                    Background = Brushes.White,
                                    BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                                    BorderThickness = new Thickness(1),
                                    Padding = new Thickness(25),
                                    Margin = new Thickness(0, 0, 0, 20),
                                    Tag = localCounter // Tag for scrolling
                                };

                                currentBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
                                {
                                    Color = (Color)ColorConverter.ConvertFromString("#AEBAF8"),
                                    BlurRadius = 15,
                                    ShadowDepth = 3,
                                    Opacity = 0.2
                                };

                                // Create Tracker in sidebar
                                CreateTrackerItem(localCounter);
                                localCounter++;

                                currentStack = new StackPanel();

                                var qtb = new TextBlock
                                {
                                    Text = qtext ?? "",
                                    FontSize = 16,
                                    FontWeight = FontWeights.Bold,
                                    Foreground = new SolidColorBrush(Color.FromRgb(150, 0, 255)),
                                    TextWrapping = TextWrapping.Wrap,
                                    Margin = new Thickness(0, 0, 0, 15)
                                };
                                currentStack.Children.Add(qtb);

                                var choiceHeader = new TextBlock
                                {
                                    Text = "Answer Choices",
                                    FontSize = 14,
                                    FontWeight = FontWeights.SemiBold,
                                    Foreground = Brushes.Gray,
                                    Margin = new Thickness(0, 0, 0, 10)
                                };
                                currentStack.Children.Add(choiceHeader);

                                currentChoicesPanel = new StackPanel();
                                currentStack.Children.Add(currentChoicesPanel);
                            }

                            // --- ADD CHOICES ---
                            if (r["ChoiceText"] != DBNull.Value)
                            {
                                var choiceText = r["ChoiceText"].ToString();
                                var choiceId = r["ChoiceID"].ToString();
                                var isCorrectObj = r["IsCorrect"];
                                var isCorrect = (isCorrectObj != DBNull.Value) && (bool)isCorrectObj;

                                if (!_gradingKey.ContainsKey(choiceId))
                                {
                                    _gradingKey.Add(choiceId, isCorrect);
                                }

                                var cb = new RadioButton
                                {
                                    IsEnabled = (userRole == "Tutee"),
                                    Content = choiceText,
                                    IsChecked = (userRole == "Tutee") ? false : isCorrect,
                                    GroupName = qid,
                                    Tag = choiceId,
                                    Style = (Style)this.FindResource("BoxRadioButtonStyle")
                                };
                                currentChoicesPanel.Children.Add(cb);
                            }
                        }

                        if (currentBorder != null)
                        {
                            currentBorder.Child = currentStack;
                            QuizQuestionsContainer.Children.Add(currentBorder);
                        }

                        // Sync the main counter so adding a new question starts at the right number
                        this.questionCounter = localCounter;
                    }
                }
            }
        }

        private void AssignStudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (userRole == "Tutee") return;

            if (AssignStudentComboBox.SelectedItem is ComboBoxItem item)
            {
                string selectedId = item.Tag?.ToString();
                if (string.Equals(selectedId, assignedTuteeId)) return;

                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("UPDATE QuizzesTable SET TuteeID = @tutee WHERE QuizID = @qid", conn))
                    {
                        cmd.Parameters.AddWithValue("@tutee", (object)selectedId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@qid", quizID);
                        cmd.ExecuteNonQuery();
                        assignedTuteeId = selectedId;
                    }
                }
            }
        }

        // --- UPDATED ADD BUTTON LOGIC ---
        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            // 1. Create a new visual Question Card
            QuizQuestionCard newCard = new QuizQuestionCard(quizID);

            // 2. Assign the current number to the Tag (for scrolling/tracking)
            newCard.Tag = questionCounter;

            // 3. Add to the visual container
            QuizQuestionsContainer.Children.Add(newCard);

            // 4. Add the text to the sidebar tracker
            CreateTrackerItem(questionCounter);

            // 5. Increment the counter for the next click
            questionCounter++;

            // Optional: Scroll to the new card
            newCard.BringIntoView();
        }

        private void CreateTrackerItem(int number)
        {
            TextBlock trackerText = new TextBlock
            {
                Text = $"Question {number}",
                Margin = new Thickness(0, 5, 0, 5),
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 100, 100)), // Gray
                Cursor = Cursors.Hand,
                Tag = number // Store the ID here too
            };

            trackerText.MouseLeftButtonUp += TrackerItem_Click;
            QuestionTrackerPanel.Children.Add(trackerText);
        }

        // I removed 'RemoveQuestionTracker' because it seems you handle removal inside the card itself now.
        // If you need to handle removal from THIS page, you would need a public method here.
        public void RemoveQuestionTracker(int questionNumber)
        {
            var trackerItem = QuestionTrackerPanel.Children
               .OfType<TextBlock>()
               .FirstOrDefault(tb => tb.Tag is int tag && tag == questionNumber);

            if (trackerItem != null)
            {
                QuestionTrackerPanel.Children.Remove(trackerItem);
            }
        }

        private void TrackerItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock clickedItem && clickedItem.Tag is int targetNumber)
            {
                // Highlight selection
                foreach (var child in QuestionTrackerPanel.Children.OfType<TextBlock>())
                {
                    child.Foreground = Brushes.Gray;
                    child.FontWeight = FontWeights.Normal;
                }
                clickedItem.Foreground = new SolidColorBrush(Color.FromRgb(150, 0, 255));
                clickedItem.FontWeight = FontWeights.Bold;

                // Scroll to card
                foreach (FrameworkElement card in QuizQuestionsContainer.Children)
                {
                    if (card.Tag is int cardNumber && cardNumber == targetNumber)
                    {
                        card.BringIntoView();
                        break;
                    }
                }
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            // --- SCENARIO 1: TUTOR SAVING ---
            if (userRole != "Tutee")
            {
                // Loop through all cards and trigger their Save function
                foreach (var child in QuizQuestionsContainer.Children)
                {
                    if (child is QuizQuestionCard card)
                    {
                        card.SaveQuestionToDatabase();
                    }
                }
                MessageBox.Show("Quiz saved successfully!", "Success");
                return;
            }

            // --- SCENARIO 2: TUTEE SUBMITTING ---
            int score = 0;
            int totalQuestions = 0;

            foreach (var child in QuizQuestionsContainer.Children)
            {
                if (child is Border border && border.Child is StackPanel mainStack)
                {
                    totalQuestions++;
                    var choicesPanel = mainStack.Children.OfType<StackPanel>().FirstOrDefault(p => p.Children.Count > 0 && p.Children[0] is RadioButton);

                    if (choicesPanel != null)
                    {
                        foreach (var rb in choicesPanel.Children.OfType<RadioButton>())
                        {
                            if (rb.IsChecked == true)
                            {
                                string selectedChoiceId = rb.Tag.ToString();
                                if (_gradingKey.ContainsKey(selectedChoiceId) && _gradingKey[selectedChoiceId] == true)
                                {
                                    score++;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();
                    string insertQuery = "INSERT INTO QuizResultsTable (QuizID, TuteeID, Score) VALUES (@qid, @uid, @score)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@qid", quizID);
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@score", score);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving result: " + ex.Message);
                return;
            }

            double percentage = totalQuestions > 0 ? (double)score / totalQuestions * 100 : 0;
            MessageBox.Show($"You scored {score} out of {totalQuestions} ({percentage:F0}%)", "Quiz Results");
            LockQuizUI();
        }

        private void LockQuizUI()
        {
            FinishButton.IsEnabled = false;
            FinishButton.Content = "Completed";

            foreach (var child in QuizQuestionsContainer.Children)
            {
                if (child is Border border && border.Child is StackPanel mainStack)
                {
                    var choicesPanel = mainStack.Children.OfType<StackPanel>().FirstOrDefault(p => p.Children.Count > 0 && p.Children[0] is RadioButton);

                    if (choicesPanel != null)
                    {
                        foreach (var rb in choicesPanel.Children.OfType<RadioButton>())
                        {
                            rb.IsEnabled = false;
                            string choiceId = rb.Tag.ToString();

                            if (_gradingKey.ContainsKey(choiceId) && _gradingKey[choiceId] == true)
                            {
                                rb.Foreground = Brushes.Green;
                                rb.FontWeight = FontWeights.Bold;
                                rb.Content = rb.Content + " (Correct)";
                            }
                            else if (rb.IsChecked == true)
                            {
                                rb.Foreground = Brushes.Red;
                                rb.Content = rb.Content + " (Your Answer)";
                            }
                        }
                    }
                }
            }
        }
    }
}