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
            MainFrame.Navigate(new HomePage());

            // Attach Checked events AFTER initialization to avoid hang
            HomeTab.Checked += (s, e) => NavigateToPage("Home");
            StudentTab.Checked += (s, e) => NavigateToPage("Students");
            LessonsTab.Checked += (s, e) => NavigateToPage("Lessons");
            QuizzesTab.Checked += (s, e) => NavigateToPage("Quizzes");
        }

        private void NavigateToPage(string page)
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
                    //MainFrame.Navigate(new LessonsPage());
                    PageTitle.Content = "Lessons";
                    break;
                case "Quizzes":
                    //MainFrame.Navigate(new QuizzesPage());
                    PageTitle.Content = "Quizzes";
                    break;
            }
        }

        private void QuizzesTab_Copy_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
