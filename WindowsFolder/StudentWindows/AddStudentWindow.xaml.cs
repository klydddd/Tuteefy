using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace TuteefyWPF.WindowsFolder.StudentWindows
{
    public partial class AddStudentWindow : Window
    {
        // Temporary storage for students (until database is implemented)
        public static System.Collections.Generic.List<StudentData> Students =
            new System.Collections.Generic.List<StudentData>();

        public AddStudentWindow()
        {
            InitializeComponent();
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