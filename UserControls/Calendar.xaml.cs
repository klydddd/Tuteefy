using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TuteefyWPF.UserControls
{
    public partial class Calendar : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<DayItem> Days { get; } = new ObservableCollection<DayItem>();
        public ObservableCollection<TaskItem> DisplayedTasks { get; } = new ObservableCollection<TaskItem>();
        public ObservableCollection<string> DisplayedNotes { get; } = new ObservableCollection<string>();

        private readonly Dictionary<DateTime, List<TaskItem>> _tasksByDate = new Dictionary<DateTime, List<TaskItem>>();
        private readonly Dictionary<DateTime, List<string>> _notesByDate = new Dictionary<DateTime, List<string>>();

        private DateTime _currentMonth;
        public DateTime CurrentMonth
        {
            get { return _currentMonth; }
            set { _currentMonth = value; OnPropertyChanged("CurrentMonth"); OnPropertyChanged("DisplayMonthYear"); RefreshCalendar(); }
        }

        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get { return _selectedDate; }
            set { _selectedDate = value.Date; OnPropertyChanged("SelectedDate"); LoadForSelectedDate(); }
        }

        public DateTime Today { get; } = DateTime.Today;
        public string DisplayMonthYear { get { return CurrentMonth.ToString("MMMM yyyy"); } }

        private bool _isAddTaskVisible;
        public bool IsAddTaskVisible { get { return _isAddTaskVisible; } set { _isAddTaskVisible = value; OnPropertyChanged("IsAddTaskVisible"); } }

        private bool _isAddNoteVisible;
        public bool IsAddNoteVisible { get { return _isAddNoteVisible; } set { _isAddNoteVisible = value; OnPropertyChanged("IsAddNoteVisible"); } }

        private int _nextTaskId = 1;

        public Calendar()
        {
            InitializeComponent();
            DataContext = this;
            CurrentMonth = new DateTime(Today.Year, Today.Month, 1);
            SelectedDate = Today;
            AddSampleData();
        }

        private void AddSampleData()
        {
            var t = new TaskItem { Id = _nextTaskId++, Title = "Welcome to Tuteefy", CreatedAt = DateTime.Now };
            SetTaskForDate(Today, t);
            SetNoteForDate(Today, "This is a sample note for today's date.");
            LoadForSelectedDate();
        }

        private void SetTaskForDate(DateTime date, TaskItem task)
        {
            date = date.Date;
            if (!_tasksByDate.ContainsKey(date))
                _tasksByDate[date] = new List<TaskItem>();
            _tasksByDate[date].Add(task);
        }

        private void SetNoteForDate(DateTime date, string note)
        {
            date = date.Date;
            if (!_notesByDate.ContainsKey(date))
                _notesByDate[date] = new List<string>();
            _notesByDate[date].Add(note);
        }

        private void RefreshCalendar()
        {
            Days.Clear();
            DateTime firstOfMonth = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
            int gap = (int)firstOfMonth.DayOfWeek;
            DateTime startDate = firstOfMonth.AddDays(-gap);

            for (int i = 0; i < 42; i++)
            {
                DateTime d = startDate.AddDays(i);
                bool isCurrent = d.Month == CurrentMonth.Month;
                DayItem item = new DayItem
                {
                    Date = d.Date,
                    DayNumberDisplay = d.Day.ToString(),
                    IsCurrentMonth = isCurrent
                };
                Days.Add(item);
            }
            OnPropertyChanged("Days");
        }

        private void LoadForSelectedDate()
        {
            DisplayedTasks.Clear();
            DisplayedNotes.Clear();

            DateTime key = SelectedDate.Date;
            if (_tasksByDate.ContainsKey(key))
                foreach (var t in _tasksByDate[key]) DisplayedTasks.Add(t);
            if (_notesByDate.ContainsKey(key))
                foreach (var n in _notesByDate[key]) DisplayedNotes.Add(n);
        }

        // Navigation buttons
        private void PrevMonthBtn_Click(object sender, RoutedEventArgs e) { CurrentMonth = CurrentMonth.AddMonths(-1); }
        private void NextMonthBtn_Click(object sender, RoutedEventArgs e) { CurrentMonth = CurrentMonth.AddMonths(1); }
        private void TodayBtn_Click(object sender, RoutedEventArgs e) { CurrentMonth = new DateTime(Today.Year, Today.Month, 1); SelectedDate = Today; }

        private void DayButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is DateTime)
                SelectedDate = ((DateTime)btn.Tag).Date;
        }

        // Task / Note logic
        private void ToggleAddTask_Click(object sender, RoutedEventArgs e)
        {
            IsAddTaskVisible = !IsAddTaskVisible;
            IsAddNoteVisible = false;
        }

        private void ToggleAddNote_Click(object sender, RoutedEventArgs e)
        {
            IsAddNoteVisible = !IsAddNoteVisible;
            IsAddTaskVisible = false;
        }

        private void ConfirmAddTask_Click(object sender, RoutedEventArgs e)
        {
            string text = NewTaskTextBox.Text.Trim();
            if (text.Length > 0)
            {
                var t = new TaskItem { Id = _nextTaskId++, Title = text, CreatedAt = DateTime.Now };
                SetTaskForDate(SelectedDate, t);
                LoadForSelectedDate();
                NewTaskTextBox.Text = "";
                IsAddTaskVisible = false;
            }
        }

        private void CancelAddTask_Click(object sender, RoutedEventArgs e)
        {
            NewTaskTextBox.Text = "";
            IsAddTaskVisible = false;
        }

        private void ConfirmAddNote_Click(object sender, RoutedEventArgs e)
        {
            string text = NewNoteTextBox.Text.Trim();
            if (text.Length > 0)
            {
                SetNoteForDate(SelectedDate, text);
                LoadForSelectedDate();
                NewNoteTextBox.Text = "";
                IsAddNoteVisible = false;
            }
        }

        private void CancelAddNote_Click(object sender, RoutedEventArgs e)
        {
            NewNoteTextBox.Text = "";
            IsAddNoteVisible = false;
        }

        private void CompleteTask_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is int)
            {
                int id = (int)btn.Tag;
                DateTime key = SelectedDate.Date;
                if (_tasksByDate.ContainsKey(key))
                {
                    var found = _tasksByDate[key].FirstOrDefault(x => x.Id == id);
                    if (found != null)
                    {
                        _tasksByDate[key].Remove(found);
                        LoadForSelectedDate();
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(prop)); }
    }

    public class DayItem
    {
        public DateTime Date { get; set; }
        public string DayNumberDisplay { get; set; }
        public bool IsCurrentMonth { get; set; }
    }

    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    
}
