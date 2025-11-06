using System.Windows;
using System.Windows.Controls;

namespace TuteefyWPF
{
    public partial class StudentCardControl : UserControl
    {
        public StudentCardControl()
        {
            InitializeComponent();
        }

        // Dependency Properties
        public string StudentName
        {
            get => (string)GetValue(StudentNameProperty);
            set => SetValue(StudentNameProperty, value);
        }
        public static readonly DependencyProperty StudentNameProperty =
            DependencyProperty.Register(nameof(StudentName), typeof(string), typeof(StudentCardControl));

        public string Subject
        {
            get => (string)GetValue(SubjectProperty);
            set => SetValue(SubjectProperty, value);
        }
        public static readonly DependencyProperty SubjectProperty =
            DependencyProperty.Register(nameof(Subject), typeof(string), typeof(StudentCardControl));

        public string TotalGrade
        {
            get => (string)GetValue(TotalGradeProperty);
            set => SetValue(TotalGradeProperty, value);
        }
        public static readonly DependencyProperty TotalGradeProperty =
            DependencyProperty.Register(nameof(TotalGrade), typeof(string), typeof(StudentCardControl));

        // Button click handler
        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"👤 {StudentName}\n📘 {Subject}\n📊 Grade: {TotalGrade}",
                            "Student Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
