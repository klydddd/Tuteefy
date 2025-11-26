using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TuteefyWPF.Classes;
using TuteefyWPF.Models;

namespace TuteefyWPF.UserControls
{
    public partial class TuteeQuizCard : UserControl
    {
        public Quiz Quiz { get; set; }
        public event EventHandler<Quiz> TakeQuizClicked;

        public TuteeQuizCard()
        {
            InitializeComponent();
        }

        public TuteeQuizCard(Quiz quiz) : this()
        {
            Quiz = quiz;
            LoadQuizData();
        }

        private void LoadQuizData()
        {
            if (Quiz != null)
            {
                QuizTitleText.Text = Quiz.Title;
                QuestionCountText.Text = $"{Quiz.TotalQuestions} Questions";

                // Calculate total points (1 point per question in this schema)
                int totalPoints = Quiz.TotalQuestions;
                TotalPointsText.Text = $"{totalPoints} Points";

                CheckQuizCompletion();
            }
        }

        private void CheckQuizCompletion()
        {
            if (UserSession.IsLoggedIn && UserSession.IsTutee)
            {
                bool isCompleted = QuizService.HasTuteeCompletedQuiz(UserSession.CurrentUser.UserID, Quiz.QuizID);

                if (isCompleted)
                {
                    CompletedBadge.Visibility = Visibility.Visible;
                    TakeQuizButton.Content = "VIEW SCORE";

                    // Load and display the score
                    QuizScore score = QuizService.GetTuteeQuizScore(UserSession.CurrentUser.UserID, Quiz.QuizID);
                    if (score != null)
                    {
                        int totalPoints = Quiz.TotalQuestions;
                        ScoreText.Text = $"Score: {score.Score}/{totalPoints}";
                        ScoreDisplay.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    CompletedBadge.Visibility = Visibility.Collapsed;
                    TakeQuizButton.Content = "TAKE QUIZ";
                    ScoreDisplay.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void TakeQuizButton_Click(object sender, RoutedEventArgs e)
        {
            TakeQuizClicked?.Invoke(this, Quiz);
        }

        private void Card_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var scaleTransform = (ScaleTransform)((TransformGroup)CardBorder.RenderTransform).Children[0];

            DoubleAnimation scaleXAnimation = new DoubleAnimation
            {
                To = 1.03,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation scaleYAnimation = new DoubleAnimation
            {
                To = 1.03,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
        }

        private void Card_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var scaleTransform = (ScaleTransform)((TransformGroup)CardBorder.RenderTransform).Children[0];

            DoubleAnimation scaleXAnimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation scaleYAnimation = new DoubleAnimation
            {
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.2),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
        }
    }
}