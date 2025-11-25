using Microsoft.Win32;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TuteefyWPF.WindowsFolder.StudentWindows
{
    public partial class AddStudentWindow : Window
    {
        // Temporary storage for students
        public static System.Collections.Generic.List<StudentData> Students =
            new System.Collections.Generic.List<StudentData>();

        private string username = string.Empty;
        private Database db = new Database();
        private string selectedImagePath = null;
        private byte[] imageBytes = null;

        public AddStudentWindow(string tutorUser)
        {
            InitializeComponent();
            username = tutorUser;

            // Add click event for photo selection
            PhotoBorder.MouseLeftButtonDown += PhotoBorder_Click;

            // Add drag and drop events
            PhotoBorder.Drop += PhotoBorder_Drop;
            PhotoBorder.DragEnter += PhotoBorder_DragEnter;
        }

        private void PhotoBorder_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select a Profile Photo"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                LoadImage(openFileDialog.FileName);
            }
        }

        private void PhotoBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string file = files[0];
                    string extension = Path.GetExtension(file).ToLower();

                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png" ||
                        extension == ".bmp" || extension == ".gif")
                    {
                        LoadImage(file);
                    }
                    else
                    {
                        MessageBox.Show("Please drop a valid image file.", "Invalid File",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void PhotoBorder_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void LoadImage(string filePath)
        {
            try
            {
                // Read the image file into a byte array
                imageBytes = File.ReadAllBytes(filePath);

                // Display the image
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                ProfileImage.Source = bitmap;
                ProfileImage.Visibility = Visibility.Visible;
                PlaceholderText.Visibility = Visibility.Collapsed;

                selectedImagePath = filePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EnrollStudentButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate user inputs first
            if (!ValidateInputs())
                return;

            // Build Student Object
            var student = new StudentData
            {
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim(),
                Email = EmailTxtBox.Text.Trim(),
                Subject = SubjectTxtBox.Text.Trim(),
                Address = AddressTxtBox.Text.Trim(),
                EnrollmentDate = DateTime.Now,
                PhotoPath = selectedImagePath,
                PhotoBytes = imageBytes
            };

            // Add to temporary storage
            //Students.Add(student);

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                // Check if email already exists
                string checkEmailQuery = "SELECT COUNT(*) FROM UserTable WHERE Email = @Email";
                using (SqlCommand checkCmd = new SqlCommand(checkEmailQuery, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Email", student.Email);
                    int emailCount = (int)checkCmd.ExecuteScalar();

                    if (emailCount > 0)
                    {
                        MessageBox.Show(
                            "This email address is already registered in the system. Please use a different email.",
                            "Duplicate Email",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                        EmailTxtBox.Focus();
                        return;
                    }
                }

                // Get next TuteeID using stored procedure
                string tuteeID = "";

                using (SqlCommand cmd = new SqlCommand("sp_GetNextTuteeID", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@TutorID", username);

                    // Add OUTPUT parameter
                    SqlParameter outputParam = new SqlParameter("@NextTuteeID", SqlDbType.VarChar, 20);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    cmd.ExecuteNonQuery();

                    // Read the output parameter
                    if (outputParam.Value == null || outputParam.Value == DBNull.Value)
                    {
                        MessageBox.Show("Failed to generate new TuteeID.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    tuteeID = outputParam.Value.ToString();
                }

                try
                {
                    // Insert into UserTable
                    string insertUser = @"
                        INSERT INTO UserTable 
                            (UserID, FullName, Email, PasswordHash, UserRole)
                        VALUES 
                            (@UserID, @FullName, @Email, @PasswordHash, 'Tutee')";

                    using (SqlCommand cmd = new SqlCommand(insertUser, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", tuteeID);
                        cmd.Parameters.AddWithValue("@FullName", student.FullName);
                        cmd.Parameters.AddWithValue("@Email", student.Email);
                        cmd.Parameters.AddWithValue("@PasswordHash", tuteeID);
                        cmd.ExecuteNonQuery();
                    }

                    // Insert into TuteeTable with photo
                    string insertTutee = @"
                        INSERT INTO TuteeTable 
                            (TuteeID, TutorID, Subject, Address, EnrollmentDate, FullName, ProfilePhoto)
                        VALUES 
                            (@TuteeID, @TutorID, @Subject, @Address, @EnrollmentDate, @FullName, @ProfilePhoto)";

                    using (SqlCommand cmd = new SqlCommand(insertTutee, conn))
                    {
                        cmd.Parameters.AddWithValue("@TuteeID", tuteeID);
                        cmd.Parameters.AddWithValue("@TutorID", username);
                        cmd.Parameters.AddWithValue("@Subject", student.Subject);
                        cmd.Parameters.AddWithValue("@Address", student.Address);
                        cmd.Parameters.AddWithValue("@EnrollmentDate", student.EnrollmentDate);
                        cmd.Parameters.AddWithValue("@FullName", student.FullName);

                        // Add photo parameter - use DBNull.Value if no photo selected
                        if (imageBytes != null && imageBytes.Length > 0)
                        {
                            cmd.Parameters.Add("@ProfilePhoto", SqlDbType.VarBinary, -1).Value = imageBytes;
                        }
                        else
                        {
                            cmd.Parameters.Add("@ProfilePhoto", SqlDbType.VarBinary, -1).Value = DBNull.Value;
                        }

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show(
                        $"Student {student.FullName} has been enrolled successfully!\n" +
                        $"Assigned Student ID: {tuteeID}",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );

                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Error: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ValidateInputs()
        {
            ResetBorderColors();
            bool isValid = true;
            TextBox firstInvalidField = null;

            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                HighlightInvalidField(FirstNameTextBox, "First Name is required.");
                if (firstInvalidField == null) firstInvalidField = FirstNameTextBox;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                HighlightInvalidField(LastNameTextBox, "Last Name is required.");
                if (firstInvalidField == null) firstInvalidField = LastNameTextBox;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(EmailTxtBox.Text))
            {
                HighlightInvalidField(EmailTxtBox, "Email is required.");
                if (firstInvalidField == null) firstInvalidField = EmailTxtBox;
                isValid = false;
            }
            else if (!IsValidEmail(EmailTxtBox.Text))
            {
                HighlightInvalidField(EmailTxtBox, "Please enter a valid email address.");
                if (firstInvalidField == null) firstInvalidField = EmailTxtBox;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(SubjectTxtBox.Text))
            {
                HighlightInvalidField(SubjectTxtBox, "Subject is required.");
                if (firstInvalidField == null) firstInvalidField = SubjectTxtBox;
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(AddressTxtBox.Text))
            {
                HighlightInvalidField(AddressTxtBox, "Address is required.");
                if (firstInvalidField == null) firstInvalidField = AddressTxtBox;
                isValid = false;
            }

            // Focus on the first invalid field
            if (firstInvalidField != null)
            {
                firstInvalidField.Focus();
                MessageBox.Show("Please fill in all required fields.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            return isValid;
        }

        private void HighlightInvalidField(TextBox textBox, string message)
        {
            var parent = VisualTreeHelper.GetParent(textBox);
            if (parent is Border border)
            {
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 53, 69));
                border.BorderThickness = new Thickness(2);
            }
            textBox.ToolTip = message;
        }

        private void ResetBorderColors()
        {
            var defaultBrush = new SolidColorBrush(Color.FromRgb(199, 184, 255));
            SetBorderColor(FirstNameTextBox, defaultBrush);
            SetBorderColor(LastNameTextBox, defaultBrush);
            SetBorderColor(EmailTxtBox, defaultBrush);
            SetBorderColor(SubjectTxtBox, defaultBrush);
            SetBorderColor(AddressTxtBox, defaultBrush);
        }

        private void SetBorderColor(TextBox textBox, Brush brush)
        {
            var parent = VisualTreeHelper.GetParent(textBox);
            if (parent is Border border)
            {
                border.BorderBrush = brush;
                border.BorderThickness = new Thickness(1);
            }
            textBox.ToolTip = null;
        }

        private bool IsValidEmail(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email.Trim(), pattern);
        }
    }

    public class StudentData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Address { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string PhotoPath { get; set; }
        public byte[] PhotoBytes { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}