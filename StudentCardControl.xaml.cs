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

namespace TuteefyWPF
{
    /// <summary>
    /// Interaction logic for StudentCardControl.xaml
    /// </summary>
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

        // Click handler
        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"👤 {StudentName}\n📘 {Subject}\n📊 Grade: {TotalGrade}",
                            "Student Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
