using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private string assignedTuteeId = null;

        public QuizView(string id, string tutorId)
        {
            InitializeComponent();
            quizID = id;
            tutorID = tutorId;
            LoadQuizData();
        }

        private void LoadQuizData()
        {
            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                // load quiz header and current assigned student
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

                // populate students for this tutor
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

                // load questions + choices in a single query (no nested readers)
                const string sql = @"
                    SELECT q.QuestionID, q.QuestionText, c.ChoiceText, c.IsCorrect
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

                        string currentQuestionId = null;
                        Border currentBorder = null;
                        StackPanel currentStack = null;
                        StackPanel currentChoicesPanel = null;

                        while (r.Read())
                        {
                            var qid = r["QuestionID"]?.ToString();
                            var qtext = r["QuestionText"]?.ToString();

                            // new question encountered
                            if (currentQuestionId == null || !string.Equals(currentQuestionId, qid, StringComparison.Ordinal))
                            {
                                // add previous question UI to container
                                if (currentBorder != null)
                                {
                                    currentBorder.Child = currentStack;
                                    QuizQuestionsContainer.Children.Add(currentBorder);
                                }

                                currentQuestionId = qid;

                                // build UI block for the new question
                                currentBorder = new Border
                                {
                                    CornerRadius = new CornerRadius(8),
                                    Background = Brushes.White,
                                    BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                                    BorderThickness = new Thickness(1),
                                    Padding = new Thickness(12),
                                    Margin = new Thickness(0, 0, 0, 12)
                                };

                                currentStack = new StackPanel();
                                var qtb = new TextBlock
                                {
                                    Text = qtext ?? "",
                                    FontWeight = FontWeights.SemiBold,
                                    TextWrapping = TextWrapping.Wrap,
                                    Margin = new Thickness(0, 0, 0, 8)
                                };
                                currentStack.Children.Add(qtb);

                                currentChoicesPanel = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };
                                currentStack.Children.Add(currentChoicesPanel);
                            }

                            // add a choice row if present (LEFT JOIN may produce NULLs)
                            if (r["ChoiceText"] != DBNull.Value)
                            {
                                var choiceText = r["ChoiceText"].ToString();
                                var isCorrectObj = r["IsCorrect"];
                                var isCorrect = (isCorrectObj != DBNull.Value) && (bool)isCorrectObj;

                                var cb = new RadioButton
                                {
                                    IsEnabled = false,
                                    Content = choiceText,
                                    IsChecked = isCorrect,
                                    Margin = new Thickness(0, 2, 0, 2)
                                };
                                currentChoicesPanel.Children.Add(cb);
                            }
                        }

                        // add the last question UI
                        if (currentBorder != null)
                        {
                            currentBorder.Child = currentStack;
                            QuizQuestionsContainer.Children.Add(currentBorder);
                        }
                    }
                }
            }
        }

        private void AssignStudentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssignStudentComboBox.SelectedItem is ComboBoxItem item)
            {
                string selectedId = item.Tag?.ToString();
                if (string.Equals(selectedId, assignedTuteeId)) return;

                // update DB
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

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            // existing behavior for authoring (if tutor)
            QuizQuestionCard newCard = new QuizQuestionCard(quizID);
            newCard.Tag = questionCounter;
            QuizQuestionsContainer.Children.Add(newCard);
            AddQuestionTracker(questionCounter);
            questionCounter++;
        }

        private void AddQuestionTracker(int questionNumber)
        {
            TextBlock trackerItem = new TextBlock
            {
                Text = $"Question {questionNumber}",
                Foreground = new SolidColorBrush(Colors.Gray),
                FontSize = 13,
                Margin = new Thickness(0, 5, 0, 5),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = questionNumber
            };
            trackerItem.MouseLeftButtonDown += TrackerItem_Click;
            QuestionTrackerPanel.Children.Add(trackerItem);
        }

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

        private void TrackerItem_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock trackerItem && trackerItem.Tag is int questionNumber)
            {
                var card = QuizQuestionsContainer.Children
                    .OfType<QuizQuestionCard>()
                    .FirstOrDefault(c => c.Tag is int tag && tag == questionNumber);

                if (card != null) card.BringIntoView();
            }
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            // placeholder
        }
    }
}