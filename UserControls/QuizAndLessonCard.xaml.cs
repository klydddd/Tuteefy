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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TuteefyWPF.UserControls
{
    /// <summary>
    /// Interaction logic for QuizAndLessonCard.xaml
    /// </summary>
    public partial class QuizAndLessonCard : UserControl
    {

        public static readonly DependencyProperty TitleProperty =
       DependencyProperty.Register("Title", typeof(string), typeof(QuizAndLessonCard));

        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register("Code", typeof(string), typeof(QuizAndLessonCard));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public string Code
        {
            get { return (string)GetValue(CodeProperty); }
            set { SetValue(CodeProperty, value); }
        }
        public QuizAndLessonCard()
        {
            InitializeComponent();
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            // Your click handler logic here
        }
    }
}
