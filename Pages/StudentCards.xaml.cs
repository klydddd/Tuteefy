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
using TuteefyWPF; // This gives access to StudentCard


namespace TuteefyWPF
{
    /// <summary>
    /// Interaction logic for StudentCards.xaml
    /// </summary>
    public partial class StudentCards : Page
    {
        public StudentCards()
        {
            InitializeComponent();

            AddStudentCard("Alex Cruz", "Mathematics", "92");
            AddStudentCard("Bianca Santos", "Physics", "88");
            AddStudentCard("Carlos Lim", "English", "95");
            AddStudentCard("Alex Cruz", "Mathematics", "92");
            AddStudentCard("Bianca Santos", "Physics", "88");
            AddStudentCard("Carlos Lim", "English", "95");
        }

        private void AddStudentCard(string name, string subject, string grade)
        {
            // Create a new instance of your UserControl
            var card = new StudentCardControl
            {
                StudentName = name,
                Subject = subject,
                TotalGrade = grade
            };

            // Add it to your WrapPanel
            StudentCardsPanel.Children.Add(card);
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                double scrollAmount = e.Delta > 0 ? -50 : 50; // adjust scroll speed
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + scrollAmount);
                e.Handled = true;
            }
        }
    }
}
