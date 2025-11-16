using System;
using System.Windows;
using System.Windows.Controls;

namespace TuteefyWPF.UserControls
{
    public partial class QuizAndLessonCard : UserControl
    {
        // Dependency Properties
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(QuizAndLessonCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register("Code", typeof(string), typeof(QuizAndLessonCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LessonContentProperty =
            DependencyProperty.Register("LessonContent", typeof(string), typeof(QuizAndLessonCard), new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }

        public string LessonContent
        {
            get { return (string)GetValue(LessonContentProperty); }
            set { SetValue(LessonContentProperty, value); }
        }

        public QuizAndLessonCard()
        {
            InitializeComponent();
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle button click
            MessageBox.Show($"Opening: {Title}");
        }
    }
}