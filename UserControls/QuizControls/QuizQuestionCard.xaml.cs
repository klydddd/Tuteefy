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

namespace TuteefyWPF.UserControls
{
    /// <summary>
    /// Interaction logic for QuizQuestionCard.xaml
    /// </summary>
    public partial class QuizQuestionCard : UserControl
    {
        public QuizQuestionCard()
        {
            InitializeComponent();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            // Find the parent StackPanel (QuizQuestionsContainer)
            var parent = FindParent<StackPanel>(this);

            if (parent != null)
            {
                // Get the question number from Tag
                int questionNumber = (int)this.Tag;

                // Remove this card from the parent
                parent.Children.Remove(this);

                // Find and remove the corresponding tracker item
                RemoveTrackerItem(questionNumber);
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

            // Find the parent StackPanel (QuizQuestionsContainer)
            var parent = FindParent<StackPanel>(this);

            if (parent != null)
            {
                // Find the QuizView page to get the question counter
                var quizViewPage = FindParent<Page>(this);

                if (quizViewPage is TuteefyWPF.Pages.QuizPages.QuizView quizView)
                {
                    // Get the current highest question number
                    int maxQuestionNumber = parent.Children
                        .OfType<QuizQuestionCard>()
                        .Select(card => card.Tag is int tag ? tag : 0)
                        .DefaultIfEmpty(0)
                        .Max();

                    // Create a new QuizQuestionCard
                    QuizQuestionCard newCard = new QuizQuestionCard();
                    newCard.Tag = maxQuestionNumber + 1;

                    // Add the new card to the container
                    parent.Children.Add(newCard);

                    // Add a corresponding tracker item
                    AddQuestionTracker(maxQuestionNumber + 1);

                    // Scroll the new card into view
                    newCard.BringIntoView();
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

        private void RemoveTrackerItem(int questionNumber)
        {
            // Find the QuizView page
            var quizViewPage = FindParent<Page>(this);

            if (quizViewPage is TuteefyWPF.Pages.QuizPages.QuizView quizView)
            {
                // Access the QuestionTrackerPanel through reflection or public property
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
        }

        private void AddQuestionTracker(int questionNumber)
        {
            // Find the QuizView page
            var quizViewPage = FindParent<Page>(this);

            if (quizViewPage is TuteefyWPF.Pages.QuizPages.QuizView quizView)
            {
                // Access the QuestionTrackerPanel
                var trackerPanel = quizView.FindName("QuestionTrackerPanel") as StackPanel;

                if (trackerPanel != null)
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

                    // Add click event to scroll to the question
                    trackerItem.MouseLeftButtonDown += (s, e) =>
                    {
                        var card = FindParent<StackPanel>(this).Children
                            .OfType<QuizQuestionCard>()
                            .FirstOrDefault(c => c.Tag is int tag && tag == questionNumber);

                        card?.BringIntoView();
                    };

                    trackerPanel.Children.Add(trackerItem);
                }
            }
        }

        private void QuestionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}