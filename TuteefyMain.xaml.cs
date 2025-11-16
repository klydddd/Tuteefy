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
        public TuteefyMain(string role, string name)
        {
            InitializeComponent();
            userRole = role;
            fullName = name;
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
                MainFrame.Navigate(new HomePage());
            }
            else if (role == "Tutee")
            {
                StudentTab.Visibility = Visibility.Collapsed;
                MainFrame.Navigate(new StudentPage(fullName));
            }
        }

        // In TuteefyMain.cs - change from private to public
        public void NavigateToPage(string page)
        {
            switch (page)
            {
                case "Home":
                    checkRole(userRole);
                    PageTitle.Content = "Home";
                    break;
                case "Students":
                    MainFrame.Navigate(new StudentCards());
                    PageTitle.Content = "Students";
                    break;
                case "Lessons":
                    MainFrame.Navigate(new TuteefyWPF.Pages.LessonsPage());
                    PageTitle.Content = "Lessons";
                    break;
                case "Quizzes":
                    MainFrame.Navigate(new TuteefyWPF.Pages.QuizPage(userRole));
                    PageTitle.Content = "Quizzes";
                    break;
                case "QuizView":
                    MainFrame.Navigate(new TuteefyWPF.Pages.QuizPages.QuizView());
                    PageTitle.Content = "Quiz View";
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
            //Show settings window
// filler
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }


}
