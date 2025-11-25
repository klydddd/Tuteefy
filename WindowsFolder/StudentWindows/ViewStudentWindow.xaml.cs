using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TuteefyWPF.WindowsFolder.StudentWindows
{
    /// <summary>
    /// Interaction logic for ViewStudentWindow.xaml
    /// </summary>
    public partial class ViewStudentWindow : Window
    {
        private readonly string _tuteeId;
        private readonly TuteefyWPF.Database db = new TuteefyWPF.Database();

        public ViewStudentWindow(string tuteeId)
        {
            InitializeComponent();
            _tuteeId = tuteeId;
            Loaded += ViewStudentWindow_Loaded;
        }

        private void ViewStudentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStudentDetails();
        }

        private void LoadStudentDetails()
        {
            if (string.IsNullOrWhiteSpace(_tuteeId))
            {
                MessageBox.Show("Invalid student id.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();

                    // Join TuteeTable with UserTable to read the email stored in UserTable
                    string query = @"
                        SELECT t.FullName, t.Subject, t.Address, t.ProfilePhoto, u.Email
                        FROM TuteeTable t
                        LEFT JOIN UserTable u ON t.TuteeID = u.UserID
                        WHERE t.TuteeID = @TuteeID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TuteeID", _tuteeId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // FullName exists in table; split into first/last for the UI if needed
                                string fullName = reader["FullName"]?.ToString() ?? string.Empty;
                                var nameParts = fullName.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                                FirstNameTextBox.Text = nameParts.Length > 0 ? nameParts[0] : string.Empty;
                                LastNameTextBox.Text = nameParts.Length > 1 ? nameParts[1] : string.Empty;

                                // Read Email safely (query includes Email via LEFT JOIN)
                                string email = string.Empty;
                                int emailIndex = -1;
                                try { emailIndex = reader.GetOrdinal("Email"); } catch { emailIndex = -1; }
                                if (emailIndex >= 0 && !reader.IsDBNull(emailIndex))
                                    email = reader.GetString(emailIndex);
                                EmailTxtBox.Text = email;

                                SubjectTxtBox.Text = reader["Subject"]?.ToString() ?? string.Empty;
                                AddressTxtBox.Text = reader["Address"]?.ToString() ?? string.Empty;

                                if (!reader.IsDBNull(reader.GetOrdinal("ProfilePhoto")))
                                {
                                    byte[] photo = (byte[])reader["ProfilePhoto"];
                                    LoadProfileImage(photo);
                                }
                                else
                                {
                                    ProfileImage.Visibility = Visibility.Collapsed;
                                    PlaceholderText.Visibility = Visibility.Visible;
                                }
                            }
                            else
                            {
                                MessageBox.Show("Student not found.", "Not found", MessageBoxButton.OK, MessageBoxImage.Information);
                                Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading student details: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProfileImage(byte[] photoData)
        {
            try
            {
                using (var stream = new MemoryStream(photoData))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    ProfileImage.Source = bitmap;
                    ProfileImage.Visibility = Visibility.Visible;
                    PlaceholderText.Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
                ProfileImage.Visibility = Visibility.Collapsed;
                PlaceholderText.Visibility = Visibility.Visible;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
