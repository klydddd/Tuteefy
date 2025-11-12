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
using System.Windows.Shapes;

namespace TuteefyWPF.WindowsFolder
{
    /// <summary>
    /// Interaction logic for AddQuizWindow.xaml
    /// </summary>
    public partial class AddQuizWindow : Window
    {
        public AddQuizWindow()
        {
            InitializeComponent();
        }

        private void CreateQuizButton_Click(object sender, RoutedEventArgs e)
        {
            TuteefyWPF.Classes.WindowHelper.UndimDialog(this.Owner);

            // Navigate to QuizView in the main window
            if (this.Owner is TuteefyMain mainWindow)
            {
                // Create the QuizView page with the data
                TuteefyWPF.Pages.QuizPages.QuizView quizViewPage = new TuteefyWPF.Pages.QuizPages.QuizView();
                quizViewPage.QuizTitle.Content = QuizTitleTextBox.Text;
                quizViewPage.QuizDesc.Content = QuizDesc.Text;

                // Navigate to the created page instance
                mainWindow.MainFrame.Navigate(quizViewPage);
            }

            this.Close();
        }
    }
}
