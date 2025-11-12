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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TuteefyWPF.UserControls;

namespace TuteefyWPF.Pages.QuizPages
{
    /// <summary>
    /// Interaction logic for QuizView.xaml
    /// </summary>
    public partial class QuizView : Page
    {
        private int questionCounter = 1;

        public QuizView()
        {
            InitializeComponent();
        }

        private void AddQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new QuizQuestionCard
            QuizQuestionCard newCard = new QuizQuestionCard();

            // Set a name or identifier for the card (optional)
            newCard.Tag = questionCounter;

            // Add the card to the container
            QuizQuestionsContainer.Children.Add(newCard);

            // Add a corresponding tracker item
            AddQuestionTracker(questionCounter);

            // Increment the counter for the next question
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
                Cursor = Cursors.Hand,
                Tag = questionNumber
            };

            // Optional: Add click event to scroll to the question
            trackerItem.MouseLeftButtonDown += TrackerItem_Click;

            QuestionTrackerPanel.Children.Add(trackerItem);
        }

        public void RemoveQuestionTracker(int questionNumber)
        {
            // Find and remove the tracker item with matching Tag
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
            if (sender is TextBlock trackerItem && trackerItem.Tag is int questionNumber)
            {
                // Find the corresponding question card
                var card = QuizQuestionsContainer.Children
                    .OfType<QuizQuestionCard>()
                    .FirstOrDefault(c => c.Tag is int tag && tag == questionNumber);

                if (card != null)
                {
                    // Scroll the card into view
                    card.BringIntoView();
                }
            }
        }
    }
}