using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace TuteefyWPF.ViewModels  // Changed to a separate namespace for ViewModels
{
    // Models
    public class Task : INotifyPropertyChanged
    {
        private string _taskName;
        private string _taskTime;

        public string TaskName
        {
            get => _taskName;
            set
            {
                _taskName = value;
                OnPropertyChanged(nameof(TaskName));
            }
        }

        public string TaskTime
        {
            get => _taskTime;
            set
            {
                _taskTime = value;
                OnPropertyChanged(nameof(TaskTime));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class Day : INotifyPropertyChanged
    {
        private int _dayNumber;
        private bool _isEnabled;
        private bool _isSelected;
        private ICommand _selectCommand;

        public int DayNumber
        {
            get => _dayNumber;
            set
            {
                _dayNumber = value;
                OnPropertyChanged(nameof(DayNumber));
            }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public ICommand SelectCommand
        {
            get => _selectCommand;
            set
            {
                _selectCommand = value;
                OnPropertyChanged(nameof(SelectCommand));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class Week
    {
        public Day[] Days { get; set; } = new Day[7];
    }

    // ViewModel
    public class CalendarViewModel : INotifyPropertyChanged
    {
        // Properties
        private DateTime _currentDate;
        private DateTime _selectedDate;
        private ObservableCollection<Week> _weeks;
        private ObservableCollection<Task> _tasksForSelectedDate;
        private string _newTaskName;
        private string _newTaskTime;
        private Dictionary<DateTime, ObservableCollection<Task>> _tasksDictionary;

        public DateTime CurrentDate
        {
            get => _currentDate;
            set
            {
                _currentDate = value;
                OnPropertyChanged(nameof(CurrentDate));
                OnPropertyChanged(nameof(CurrentMonthYear));
                GenerateWeeks();
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                UpdateTasksForSelectedDate();
            }
        }

        public ObservableCollection<Week> Weeks
        {
            get => _weeks;
            set
            {
                _weeks = value;
                OnPropertyChanged(nameof(Weeks));
            }
        }

        public ObservableCollection<Task> TasksForSelectedDate
        {
            get => _tasksForSelectedDate;
            set
            {
                _tasksForSelectedDate = value;
                OnPropertyChanged(nameof(TasksForSelectedDate));
            }
        }

        public string NewTaskName
        {
            get => _newTaskName;
            set
            {
                _newTaskName = value;
                OnPropertyChanged(nameof(NewTaskName));
            }
        }

        public string NewTaskTime
        {
            get => _newTaskTime;
            set
            {
                _newTaskTime = value;
                OnPropertyChanged(nameof(NewTaskTime));
            }
        }

        public string CurrentMonthYear => CurrentDate.ToString("MMMM yyyy");

        // Commands
        public ICommand PreviousMonthCommand { get; }
        public ICommand NextMonthCommand { get; }
        public ICommand AddTaskCommand { get; }

        // Constructor
        public CalendarViewModel()
        {
            CurrentDate = DateTime.Today;
            SelectedDate = DateTime.Today;
            _tasksDictionary = new Dictionary<DateTime, ObservableCollection<Task>>();
            Weeks = new ObservableCollection<Week>();
            TasksForSelectedDate = new ObservableCollection<Task>();

            PreviousMonthCommand = new RelayCommand(() => CurrentDate = CurrentDate.AddMonths(-1));
            NextMonthCommand = new RelayCommand(() => CurrentDate = CurrentDate.AddMonths(1));
            AddTaskCommand = new RelayCommand(AddTask);

            GenerateWeeks();
            UpdateTasksForSelectedDate();
        }

        // Methods
        private void GenerateWeeks()
        {
            Weeks.Clear();
            DateTime firstDayOfMonth = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(CurrentDate.Year, CurrentDate.Month);
            int startDayOfWeek = (int)firstDayOfMonth.DayOfWeek;

            Week currentWeek = new Week();
            int dayCounter = 1;

            // Fill previous month's days if needed
            for (int i = 0; i < startDayOfWeek; i++)
            {
                DateTime prevMonthDate = firstDayOfMonth.AddDays(-(startDayOfWeek - i));
                currentWeek.Days[i] = new Day
                {
                    DayNumber = prevMonthDate.Day,
                    IsEnabled = false,
                    SelectCommand = new RelayCommand(() => { }) // Disabled, no action
                };
            }

            // Fill current month's days
            for (int i = startDayOfWeek; i < 7 && dayCounter <= daysInMonth; i++)
            {
                DateTime date = new DateTime(CurrentDate.Year, CurrentDate.Month, dayCounter);
                currentWeek.Days[i] = new Day
                {
                    DayNumber = dayCounter,
                    IsEnabled = true,
                    IsSelected = date.Date == SelectedDate.Date,
                    SelectCommand = new RelayCommand(() => SelectedDate = date)
                };
                dayCounter++;
            }

            Weeks.Add(currentWeek);

            // Fill remaining weeks
            while (dayCounter <= daysInMonth)
            {
                currentWeek = new Week();
                for (int i = 0; i < 7 && dayCounter <= daysInMonth; i++)
                {
                    DateTime date = new DateTime(CurrentDate.Year, CurrentDate.Month, dayCounter);
                    currentWeek.Days[i] = new Day
                    {
                        DayNumber = dayCounter,
                        IsEnabled = true,
                        IsSelected = date.Date == SelectedDate.Date,
                        SelectCommand = new RelayCommand(() => SelectedDate = date)
                    };
                    dayCounter++;
                }
                Weeks.Add(currentWeek);
            }

            // Fill next month's days if needed to complete the last week
            if (Weeks.Last().Days.Any(d => d == null))
            {
                int nextMonthDay = 1;
                for (int i = 0; i < 7; i++)
                {
                    if (Weeks.Last().Days[i] == null)
                    {
                        Weeks.Last().Days[i] = new Day
                        {
                            DayNumber = nextMonthDay++,
                            IsEnabled = false,
                            SelectCommand = new RelayCommand(() => { }) // Disabled
                        };
                    }
                }
            }
        }

        private void UpdateTasksForSelectedDate()
        {
            if (_tasksDictionary.TryGetValue(SelectedDate.Date, out var tasks))
            {
                TasksForSelectedDate = new ObservableCollection<Task>(tasks);
            }
            else
            {
                TasksForSelectedDate = new ObservableCollection<Task>();
            }
            // Update IsSelected for all days
            foreach (var week in Weeks)
            {
                foreach (var day in week.Days.Where(d => d != null))
                {
                    DateTime date = new DateTime(CurrentDate.Year, CurrentDate.Month, day.DayNumber);
                    day.IsSelected = date.Date == SelectedDate.Date;
                }
            }
        }

        private void AddTask()
        {
            if (!string.IsNullOrWhiteSpace(NewTaskName) && !string.IsNullOrWhiteSpace(NewTaskTime))
            {
                DateTime date = SelectedDate.Date;
                if (!_tasksDictionary.ContainsKey(date))
                {
                    _tasksDictionary[date] = new ObservableCollection<Task>();
                }
                _tasksDictionary[date].Add(new Task { TaskName = NewTaskName, TaskTime = NewTaskTime });
                NewTaskName = string.Empty;
                NewTaskTime = string.Empty;
                UpdateTasksForSelectedDate();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Simple RelayCommand implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}