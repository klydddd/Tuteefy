using System.Windows;
using System.Windows.Controls;

namespace TuteefyWPF
{
    public partial class TuteefyMain : Window
    {
        public TuteefyMain()
        {
            InitializeComponent();

            // Set initial page
            MainFrame.Navigate(new Pages.StudentPage());

            // Attach Checked events AFTER initialization to avoid hang
            HomeTab.Checked += (s, e) => NavigateToPage("Home");
            StudentTab.Checked += (s, e) => NavigateToPage("Students");
            LessonsTab.Checked += (s, e) => NavigateToPage("Lessons");
            QuizzesTab.Checked += (s, e) => NavigateToPage("Quizzes");
        }

        // In TuteefyMain.cs - change from private to public
        public void NavigateToPage(string page)
        {
            switch (page)
            {
                case "Home":
                    MainFrame.Navigate(new HomePage());
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
                    MainFrame.Navigate(new TuteefyWPF.Pages.QuizPage());
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
            Application.Current.Shutdown();
        }

        private void Settings_Checked(object sender, RoutedEventArgs e)
        {
            //Show settings window
        }
    }


}
