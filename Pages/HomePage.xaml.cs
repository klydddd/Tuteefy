using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TuteefyWPF
{
    public partial class HomePage : Page
    {
        public ChartValues<double> ChartValues { get; set; }
        public List<string> Labels { get; set; }
        public DateTime SelectedDate { get; set; }

        public HomePage()
        {
            InitializeComponent();

            ChartValues = new ChartValues<double> { 3, 5, 7, 4, 6, 8, 9 };
            Labels = new List<string> { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

            SelectedDate = DateTime.Today;
            DataContext = this;
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = TaskCalendar.SelectedDate ?? DateTime.Today;

            // Use a modern WPF dialog instead of InputBox
            var dialog = new AddTaskDialog(selectedDate)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                string task = dialog.TaskText?.Trim();
                if (!string.IsNullOrEmpty(task))
                {
                    TaskList.Items.Add($"{selectedDate:MMM dd}: {task}");
                }
            }
        }

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskList.SelectedItem != null)
            {
                var result = MessageBox.Show("Are you sure you want to delete this task?",
                                             "Confirm Delete",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    TaskList.Items.Remove(TaskList.SelectedItem);
                }
            }
            else
            {
                MessageBox.Show("Please select a task to delete.",
                                "No Task Selected",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
        }

    }
}
