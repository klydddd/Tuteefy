using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using TuteefyWPF.Classes;
using TuteefyWPF.Pages;

namespace TuteefyWPF
{
    public partial class TuteefyMain : Window
    {
        private string userRole = string.Empty;
        private string fullName = string.Empty;
        private string CurrentTutorID = string.Empty;

        public TuteefyMain()
        {
            InitializeComponent();

            if (UserSession.IsLoggedIn && UserSession.CurrentUser != null)
            {
                CurrentTutorID = UserSession.CurrentUser.UserID;
                fullName = UserSession.CurrentUser.FullName;
                userRole = UserSession.CurrentUser.UserRole;
                checkRole(userRole);
            }
            else
            {
                this.Loaded += (s, e) =>  // ← Use Loaded event to avoid window error!
                {
                    MessageBox.Show("Please log in first.");
                    MainWindow login = new MainWindow();
                    login.Show();
                    this.Close();
                };
            }
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
                    MainFrame.Navigate(new StudentCards(CurrentTutorID));
                    PageTitle.Content = "Students";
                    break;
                case "Lessons":
                    MainFrame.Navigate(new TuteefyWPF.Pages.LessonsPage(CurrentTutorID));
                    PageTitle.Content = "Lessons";
                    break;
                case "Quizzes":
                    MainFrame.Navigate(new TuteefyWPF.Pages.QuizPage());
                    PageTitle.Content = "Quizzes";
                    break;
                    /*case "QuizView":
                        MainFrame.Navigate(new TuteefyWPF.Pages.QuizPages.QuizView());
                        PageTitle.Content = "Quiz View";
                        break;*/
            }
        }

        private void LogOut_Checked(object sender, RoutedEventArgs e)
        {
            // Clear user session
            UserSession.Logout();

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

        private void MainFrame_Navigated_1(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
        }
    }
}