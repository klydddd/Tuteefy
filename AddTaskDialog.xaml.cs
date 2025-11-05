using System;
using System.Windows;

namespace TuteefyWPF
{
    public partial class AddTaskDialog : Window
    {
        public string TaskText { get; private set; }

        public AddTaskDialog(DateTime date)
        {
            InitializeComponent();
            DialogTitle.Text = $"Add a task for {date:MMMM dd, yyyy}";
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            TaskText = TaskInput.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
