using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TuteefyWPF.Classes;
using TuteefyWPF.Models;

namespace TuteefyWPF.WindowsFolder
{
    public partial class TakeQuizWindow : Window
    {
        private Quiz _quiz;
        private List<Question> _questions;
        private Dictionary<string, string> _answers; // QuestionID -> Selected ChoiceID

        public TakeQuizWindow()
        {
            InitializeComponent();
        }

        public TakeQuizWindow(Quiz quiz) : this()
        {
            _quiz = quiz;
            _answers = new Dictionary<string, string>();
            LoadQuizData();
        }

        private void LoadQuizData()
        {
            try
            {
                QuizTitleText.Text = _quiz.Title;
                QuizDescriptionText.Text = _quiz.Description;

                // Load questions
                _questions = QuizService.GetQuestionsForQuiz(_quiz.QuizID);

                if (_questions.Count == 0)
                {
                    MessageBox.Show("This quiz has no questions yet.", "No Questions",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                    return;
                }

                TotalQuestionsText.Text = $"Total Questions: {_questions.Count}";
                DisplayQuestions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quiz: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void DisplayQuestions()
        {
            QuestionsPanel.Children.Clear();

            for (int i = 0; i < _questions.Count; i++)
            {
                var question = _questions[i];

                // Question container
                Border questionBorder = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(20),
                    Margin = new Thickness(0, 0, 0, 20),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect
                    {
                        BlurRadius = 10,
                        ShadowDepth = 0,
                        Opacity = 0.1
                    }
                };

                StackPanel questionStack = new StackPanel();

                // Question number and text
                TextBlock questionNumber = new TextBlock
                {
                    Text = $"Question {i + 1}",
                    FontFamily = new FontFamily("Poppins"),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8DC63F")),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                questionStack.Children.Add(questionNumber);

                TextBlock questionText = new TextBlock
                {
                    Text = question.QuestionText,
                    FontFamily = new FontFamily("Poppins"),
                    FontSize = 16,
                    FontWeight = FontWeights.Medium,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 15)
                };
                questionStack.Children.Add(questionText);

                // Multiple choice options
                DisplayMultipleChoice(questionStack, question);

                questionBorder.Child = questionStack;
                QuestionsPanel.Children.Add(questionBorder);
            }
        }

        private void DisplayMultipleChoice(StackPanel parent, Question question)
        {
            string groupName = $"Question_{question.QuestionID}";

            foreach (var choice in question.Choices)
            {
                RadioButton optionButton = new RadioButton
                {
                    Content = choice.ChoiceText,
                    GroupName = groupName,
                    FontFamily = new FontFamily("Poppins"),
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 5),
                    Tag = choice.ChoiceID, // Store ChoiceID in Tag
                    Cursor = System.Windows.Input.Cursors.Hand
                };

                optionButton.Checked += (s, e) => OptionButton_Checked(question.QuestionID, choice.ChoiceID);

                parent.Children.Add(optionButton);
            }
        }

        private void OptionButton_Checked(string questionID, string choiceID)
        {
            // Store the selected choice ID
            if (_answers.ContainsKey(questionID))
            {
                _answers[questionID] = choiceID;
            }
            else
            {
                _answers.Add(questionID, choiceID);
            }

            UpdateProgress();
        }

        private void UpdateProgress()
        {
            int answeredCount = _answers.Count;
            int totalQuestions = _questions.Count;
            ProgressText.Text = $"Progress: {answeredCount}/{totalQuestions} answered";
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if all questions are answered
            if (_answers.Count < _questions.Count)
            {
                var result = MessageBox.Show(
                    $"You have answered {_answers.Count} out of {_questions.Count} questions.\n" +
                    "Unanswered questions will be marked as incorrect.\n\n" +
                    "Do you want to submit anyway?",
                    "Incomplete Quiz",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            try
            {
                int score = CalculateScore();
                SaveQuizScore(score);

                // Show results
                MessageBox.Show(
                    $"Quiz Completed!\n\n" +
                    $"Your Score: {score} / {_questions.Count}\n" +
                    $"Percentage: {(double)score / _questions.Count * 100:F1}%",
                    "Quiz Results",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting quiz: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int CalculateScore()
        {
            int correctAnswers = 0;

            foreach (var question in _questions)
            {
                // Check if the question was answered
                if (_answers.ContainsKey(question.QuestionID))
                {
                    string selectedChoiceID = _answers[question.QuestionID];

                    // Find the selected choice and check if it's correct
                    var selectedChoice = question.Choices.FirstOrDefault(c => c.ChoiceID == selectedChoiceID);

                    if (selectedChoice != null && selectedChoice.IsCorrect)
                    {
                        correctAnswers++;
                    }
                }
                // If not answered, it's counted as incorrect (0 points)
            }

            return correctAnswers;
        }

        private void SaveQuizScore(int score)
        {
            if (UserSession.IsLoggedIn && UserSession.IsTutee)
            {
                QuizService.SaveQuizScore(UserSession.CurrentUser.UserID, _quiz.QuizID, score);
            }
            else
            {
                throw new Exception("You must be logged in as a tutee to submit the quiz.");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit without submitting?\n" +
                "Your answers will not be saved.",
                "Exit Quiz",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply window dimming effect to parent
            WindowHelper.DimBackgroundWindow(this);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Remove dimming effect
            WindowHelper.RemoveDimBackgroundWindow();
        }
    }
}