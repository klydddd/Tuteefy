using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using TuteefyWPF.Pages;

namespace TuteefyWPF
{
    public partial class TuteefyMain : Window
    {
        private string userRole = string.Empty;
        private string fullName = string.Empty;
        private string CurrentTutorID = string.Empty;

        public TuteefyMain(string tutorID, string role, string name)
        {
            InitializeComponent();

            // 1. FIX: Assign variables FIRST so they are ready when pages load
            userRole = role;
            fullName = name;
            CurrentTutorID = tutorID; // Moved this up!

            // 2. Now check role (CurrentTutorID is now valid)
            checkRole(role);

            // Attach Checked events AFTER initialization to avoid hang
            HomeTab.Checked += (s, e) => NavigateToPage("Home");
            StudentTab.Checked += (s, e) => NavigateToPage("Students");
            LessonsTab.Checked += (s, e) => NavigateToPage("Lessons");
            QuizzesTab.Checked += (s, e) => NavigateToPage("Quizzes");
        }

        private void checkRole(string role)
        {
            if (role == "Tutor")
            {
                MainFrame.Navigate(new HomePage(CurrentTutorID));
            }
            else if (role == "Tutee")
            {
                StudentTab.Visibility = Visibility.Collapsed;

                // UPDATED: Pass the ID (CurrentTutorID variable holds the ID for both roles in your main window logic)
                MainFrame.Navigate(new StudentPage(CurrentTutorID, fullName));
            }
        }

        public void NavigateToPage(string page)
        {
            switch (page)
            {
                case "Home":
                    checkRole(userRole);
                    PageTitle.Content = "Home";
                    break;
                case "Students":
                    MainFrame.Navigate(new StudentCards(CurrentTutorID));
                    PageTitle.Content = "Students";
                    break;
                case "Lessons":
                    MainFrame.Navigate(new TuteefyWPF.Pages.LessonsPage(CurrentTutorID, userRole));
                    PageTitle.Content = "Lessons";
                    break;
                case "Quizzes":
                    MainFrame.Navigate(new TuteefyWPF.Pages.QuizPage(userRole, CurrentTutorID));
                    PageTitle.Content = "Quizzes";
                    break;
            }
        }

        private void LogOut_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        private void Settings_Checked(object sender, RoutedEventArgs e)
        {
            // Show settings window
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void MainFrame_Navigated_1(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}