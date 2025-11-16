using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TuteefyWPF.WindowsFolder.StudentWindows
{
    public partial class AddStudentWindow : Window
    {
        // Temporary storage for students (until database is implemented)
        public static System.Collections.Generic.List<StudentData> Students =
            new System.Collections.Generic.List<StudentData>();
        private string username = string.Empty;
        private Database db = new Database();

        public AddStudentWindow(string tutorUser)
        {
            InitializeComponent();
            username = tutorUser;
        }

        private void EnrollStudentButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate all fields
            if (!ValidateInputs())
            {
                return;
            }

            // Create student data object
            var student = new StudentData
            {
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim(),
                Email = EmailTxtBox.Text.Trim(),
                Subject = SubjectTxtBox.Text.Trim(),
                Address = AddressTxtBox.Text.Trim(),
                EnrollmentDate = DateTime.Now
            };

            // Add to temporary storage
            Students.Add(student);

            string GetNextTuteeID(SqlConnection conn)
            {
                string query = "SELECT TOP 1 TuteeID FROM TuteeTable " +
                                "WHERE TutorID = '" + username + "' " +
                                 "ORDER BY TuteeID DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                var lastId = cmd.ExecuteScalar() as string;

                if (string.IsNullOrEmpty(lastId))
                    return username + "-001";

                int num = int.Parse(lastId.Substring(6));
                return username + "-" + (num + 1).ToString("D3");
            }

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                string tuteeID = GetNextTuteeID(conn);

                try
                {
                    string insertUser = @"INSERT INTO UserTable (UserID, FullName, Email, PasswordHash, UserRole)
                                    VALUES (@UserID, @FullName, @Email, @PasswordHash, 'Tutee')";

                    using (SqlCommand cmd = new SqlCommand(insertUser, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", tuteeID);
                        cmd.Parameters.AddWithValue("@FullName", student.FullName);
                        cmd.Parameters.AddWithValue("@Email", student.Email);
                        cmd.Parameters.AddWithValue("@PasswordHash", tuteeID);
                        cmd.ExecuteNonQuery();
                    }

                    string insertTutee = "INSERT INTO TuteeTable (TuteeID, TutorID, Subject, Address, EnrollmentDate, FullName)" +
                                         "VALUES (@TuteeID, @TutorID, @Subject, @Address, @EnrollmentDate, @FullName)";
                    using (SqlCommand cmd = new SqlCommand(insertTutee, conn))
                    {
                        cmd.Parameters.AddWithValue("@TuteeID", tuteeID);
                        cmd.Parameters.AddWithValue("@TutorID", username);
                        cmd.Parameters.AddWithValue("@Subject", student.Subject);
                        cmd.Parameters.AddWithValue("@Address", student.Address);
                        cmd.Parameters.AddWithValue("@EnrollmentDate", student.EnrollmentDate);
                        cmd.Parameters.AddWithValue("@EnrollmentDate", student.FullName);
                        cmd.ExecuteNonQuery();
                    }

                    // Show success message
                    MessageBox.Show(
                        $"Student {student.FirstName} {student.LastName} has been enrolled successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    // Close the window
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private bool ValidateInputs()
        {
            // Reset all borders to default
            ResetBorderColors();

            bool isValid = true;

            // Validate First Name
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                HighlightInvalidField(FirstNameTextBox, "First Name is required.");
                isValid = false;
            }

            // Validate Last Name
            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                HighlightInvalidField(LastNameTextBox, "Last Name is required.");
                isValid = false;
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(EmailTxtBox.Text))
            {
                HighlightInvalidField(EmailTxtBox, "Email is required.");
                isValid = false;
            }
            else if (!IsValidEmail(EmailTxtBox.Text))
            {
                HighlightInvalidField(EmailTxtBox, "Please enter a valid email address.");
                isValid = false;
            }

            // Validate Subject
            if (string.IsNullOrWhiteSpace(SubjectTxtBox.Text))
            {
                HighlightInvalidField(SubjectTxtBox, "Subject is required.");
                isValid = false;
            }
            

            // Validate Address
            if (string.IsNullOrWhiteSpace(AddressTxtBox.Text))
            {
                HighlightInvalidField(AddressTxtBox, "Address is required.");
                isValid = false;
            }

            return isValid;
        }

        private void HighlightInvalidField(System.Windows.Controls.TextBox textBox, string message)
        {
            // Find the parent Border
            var parent = VisualTreeHelper.GetParent(textBox);
            if (parent is System.Windows.Controls.Border border)
            {
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Red
                border.BorderThickness = new Thickness(2);
            }

            // Show tooltip with error message
            textBox.ToolTip = message;
            textBox.Focus();
        }

        private void ResetBorderColors()
        {
            // Reset all borders to default purple color
            var defaultBrush = new SolidColorBrush(Color.FromRgb(199, 184, 255)); // #C7B8FF

            SetBorderColor(FirstNameTextBox, defaultBrush);
            SetBorderColor(LastNameTextBox, defaultBrush);
            SetBorderColor(EmailTxtBox, defaultBrush);
            SetBorderColor(SubjectTxtBox, defaultBrush);
            SetBorderColor(AddressTxtBox, defaultBrush);
        }

        private void SetBorderColor(System.Windows.Controls.TextBox textBox, Brush brush)
        {
            var parent = VisualTreeHelper.GetParent(textBox);
            if (parent is System.Windows.Controls.Border border)
            {
                border.BorderBrush = brush;
                border.BorderThickness = new Thickness(1);
            }
            textBox.ToolTip = null;
        }

        private bool IsValidEmail(string email)
        {
            // Simple email validation regex
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email.Trim(), pattern);
        }

        private void Calendar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }

    // Student data class (temporary until database is implemented)
    public class StudentData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Address { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string PhotoPath { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}