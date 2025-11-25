using System;
using System.Windows;
using System.Windows.Controls;
using TuteefyWPF.WindowsFolder.LessonsWindows;

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

        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register("FilePath", typeof(string), typeof(QuizAndLessonCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LessonIdProperty =
            DependencyProperty.Register("LessonId", typeof(int), typeof(QuizAndLessonCard), new PropertyMetadata(0));

        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName", typeof(string), typeof(QuizAndLessonCard), new PropertyMetadata(string.Empty));

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

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public int LessonId
        {
            get { return (int)GetValue(LessonIdProperty); }
            set { SetValue(LessonIdProperty, value); }
        }

        public string FileName
        {
            get { return (string)GetValue(FileNameProperty); }
            set { SetValue(FileNameProperty, value); }
        }

        public QuizAndLessonCard()
        {
            InitializeComponent();
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            // Pass LessonId and FileName; LessonViewWindow will fetch bytes on demand
            if (!string.IsNullOrWhiteSpace(LessonContent) || !string.IsNullOrWhiteSpace(FileName))
            {
                var win = new LessonViewWindow(LessonId, Title, Code, LessonContent ?? string.Empty, FileName ?? string.Empty);
                TuteefyWPF.Classes.WindowHelper.ShowDimmedDialog(Window.GetWindow(this), win);
                return;
            }

            MessageBox.Show($"Opening: {Title}");
        }
    }
}