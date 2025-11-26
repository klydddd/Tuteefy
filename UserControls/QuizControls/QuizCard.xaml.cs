using System;
using System.Data.SqlClient; // Needed for DB
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TuteefyWPF.Pages.QuizPages;

namespace TuteefyWPF.UserControls.QuizControls
{
    public partial class QuizCard : UserControl
    {
        private Database db = new Database(); // Add Database instance

        // 1. Existing Properties
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(QuizCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty CodeProperty =
            DependencyProperty.Register("Code", typeof(string), typeof(QuizCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TutorIDProperty =
            DependencyProperty.Register("TutorID", typeof(string), typeof(QuizCard), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty UserRoleProperty =
            DependencyProperty.Register("UserRole", typeof(string), typeof(QuizCard), new PropertyMetadata(string.Empty));

        // 2. NEW Property: DisplayText (This will hold either the Code OR the Score)
        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register("DisplayText", typeof(string), typeof(QuizCard), new PropertyMetadata(string.Empty));

        public string CurrentUserID { get; set; }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Code
        {
            get => (string)GetValue(CodeProperty);
            set => SetValue(CodeProperty, value);
        }

        public string TutorID
        {
            get => (string)GetValue(TutorIDProperty);
            set => SetValue(TutorIDProperty, value);
        }

        public string UserRole
        {
            get => (string)GetValue(UserRoleProperty);
            set => SetValue(UserRoleProperty, value);
        }

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }

        public QuizCard()
        {
            InitializeComponent();
            // Listen for when the card is fully loaded to fetch the score
            this.Loaded += QuizCard_Loaded;
        }

        private void QuizCard_Loaded(object sender, RoutedEventArgs e)
        {
            // Default state: Show the Code
            DisplayText = Code;

            // Only check for score if the user is a Tutee and we have their ID
            if (UserRole == "Tutee" && !string.IsNullOrEmpty(CurrentUserID))
            {
                LoadScore();
            }
        }

        private void LoadScore()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();

                    // 1. Get the User's Score
                    string scoreQuery = "SELECT Score FROM QuizResultsTable WHERE QuizID = @qid AND TuteeID = @uid";
                    int userScore = -1;

                    using (SqlCommand cmd = new SqlCommand(scoreQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@qid", Code);
                        cmd.Parameters.AddWithValue("@uid", CurrentUserID);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            userScore = Convert.ToInt32(result);
                        }
                    }

                    // If the user hasn't taken the quiz, stop here.
                    if (userScore == -1) return;

                    // 2. Get Total Questions to calculate percentage
                    string countQuery = "SELECT COUNT(*) FROM QuestionsTable WHERE QuizID = @qid";
                    int totalQuestions = 0;

                    using (SqlCommand cmd = new SqlCommand(countQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@qid", Code);
                        object countResult = cmd.ExecuteScalar();
                        if (countResult != null)
                        {
                            totalQuestions = Convert.ToInt32(countResult);
                        }
                    }

                    // 3. Calculate Percentage
                    if (totalQuestions > 0)
                    {
                        double percentage = (double)userScore / totalQuestions * 100;
                        // "F0" formats it as a whole number (e.g., 85%)
                        DisplayText = $"Score: {percentage:F0}%";
                    }
                    else
                    {
                        // Fallback just in case
                        DisplayText = $"Score: {userScore}";
                    }
                }
            }
            catch
            {
                // If DB fails, simply do nothing (Code remains displayed)
            }
        }
        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            var main = Application.Current.Windows
                        .OfType<TuteefyMain>()
                        .FirstOrDefault();

            if (main != null)
            {
                var page = new QuizView(Code, TutorID, UserRole, CurrentUserID);
                main.MainFrame.Navigate(page);
            }
            else
            {
                MessageBox.Show("Unable to find main window to navigate.", "Navigation Error");
            }
        }
    }
}