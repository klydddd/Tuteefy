// This file should REPLACE your existing QuizPage.xaml.cs code-behind
// OR you can just update the LoadQuizzes() and AddQuizCard() methods

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TuteefyWPF.Classes;
using TuteefyWPF.Models;
using TuteefyWPF.UserControls;
using TuteefyWPF.WindowsFolder;

namespace TuteefyWPF.Pages
{
    public partial class QuizPage : Page
    {
        // Add overloaded constructor if needed
        public QuizPage() : this(null, null)
        {
        }

        public QuizPage(string tuteeID, string tutorID)
        {
            InitializeComponent();
            LoadQuizzes();
        }

        private void LoadQuizzes()
        {
            QuizzesPanel.Children.Clear();

            try
            {
                if (UserSession.IsTutee)
                {
                    // Load quizzes assigned to this tutee
                    List<Quiz> assignedQuizzes = QuizService.GetQuizzesForTutee(UserSession.CurrentUser.UserID);

                    if (assignedQuizzes.Count == 0)
                    {
                        TextBlock noQuizzesText = new TextBlock
                        {
                            Text = "No quizzes assigned to you yet.",
                            FontSize = 16,
                            Foreground = Brushes.Gray,
                            Margin = new Thickness(20),
                            TextAlignment = TextAlignment.Center
                        };
                        QuizzesPanel.Children.Add(noQuizzesText);
                    }
                    else
                    {
                        foreach (var quiz in assignedQuizzes)
                        {
                            AddQuizCard(quiz);
                        }
                    }

                    // Hide Create Quiz button for tutees
                    if (CreateQuizButton != null)
                    {
                        CreateQuizButton.Visibility = Visibility.Collapsed;
                    }
                }
                else if (UserSession.IsTutor)
                {
                    // For tutors, load all quizzes
                    List<Quiz> allQuizzes = QuizService.GetAllQuizzes();

                    if (allQuizzes.Count == 0)
                    {
                        TextBlock noQuizzesText = new TextBlock
                        {
                            Text = "No quizzes created yet.",
                            FontSize = 16,
                            Foreground = Brushes.Gray,
                            Margin = new Thickness(20),
                            TextAlignment = TextAlignment.Center
                        };
                        QuizzesPanel.Children.Add(noQuizzesText);
                    }
                    else
                    {
                        foreach (var quiz in allQuizzes)
                        {
                            AddQuizCard(quiz);
                        }
                    }

                    // Show Create Quiz button for tutors
                    if (CreateQuizButton != null)
                    {
                        CreateQuizButton.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading quizzes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddQuizCard(Quiz quiz)
        {
            TuteeQuizCard quizCard = new TuteeQuizCard(quiz);

            // Subscribe to the TakeQuizClicked event
            quizCard.TakeQuizClicked += QuizCard_TakeQuizClicked;

            QuizzesPanel.Children.Add(quizCard);
        }

        private void QuizCard_TakeQuizClicked(object sender, Quiz quiz)
        {
            try
            {
                // Open the TakeQuizWindow
                TakeQuizWindow takeQuizWindow = new TakeQuizWindow(quiz);
                bool? result = takeQuizWindow.ShowDialog();

                // If the quiz was completed, refresh the quiz list
                if (result == true)
                {
                    LoadQuizzes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening quiz: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateQuizButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement quiz creation window
            MessageBox.Show("Quiz creation feature coming soon!", "Feature Not Available",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadQuizzes();
        }

        // Add other event handlers as needed
    }
}