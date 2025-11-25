using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TuteefyWPF.Pages.QuizPages;

namespace TuteefyWPF.UserControls.QuizControls
{
    /// <summary>
    /// Interaction logic for QuizCard.xaml
    /// </summary>
    public partial class QuizCard : UserControl
    {
        // expose Title and Code so XAML bindings work
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(QuizCard), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register("Code", typeof(string), typeof(QuizCard), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty TutorIDProperty =
            DependencyProperty.Register("TutorID", typeof(string), typeof(QuizCard), new PropertyMetadata(string.Empty));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Code
        {
            get => (string)GetValue(CodeProperty);
            set => SetValue(CodeProperty, value);
        }

        public string TutorID
        {
            get => (string)GetValue(TutorIDProperty);
            set => SetValue(TutorIDProperty, value);
        }

        public QuizCard()
        {
            InitializeComponent();
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to QuizView and pass quiz id + tutor id so QuizView can load questions and students
            var main = Application.Current.Windows
                        .OfType<TuteefyMain>()
                        .FirstOrDefault();

            if (main != null)
            {
                var page = new QuizView(Code, TutorID);
                main.MainFrame.Navigate(page);
            }
            else
            {
                MessageBox.Show("Unable to find main window to navigate.", "Navigation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
