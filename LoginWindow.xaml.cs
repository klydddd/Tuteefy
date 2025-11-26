using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using TuteefyWPF.Classes;
using TuteefyWPF.Models;

namespace TuteefyWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Database db;
        public String username;
        private String password;

        public MainWindow()
        {
            InitializeComponent();
            db = new Database();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SignupWindow signupWindow = new SignupWindow();
            signupWindow.Show();
            this.Close();
        }



        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    username = txtUser.Text.Trim();
        //    password = txtPassword.Password;

        //    if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        //    {
        //        MessageBox.Show("Please enter both username and password.");
        //        return;
        //    }

        //    using (SqlConnection conn = new SqlConnection(db.connectionString))
        //    {
        //        try
        //        {
        //            conn.Open();

        //            string query = "SELECT UserRole, FullName FROM UserTable WHERE UserID=@username AND PasswordHash=@password";

        //            SqlCommand cmd = new SqlCommand(query, conn);
        //            cmd.Parameters.AddWithValue("@username", username);
        //            cmd.Parameters.AddWithValue("@password", password);
                    

        //            using (SqlDataReader reader = cmd.ExecuteReader())
        //            {
        //                if (reader.Read())
        //                {
        //                    string fullname = reader["FullName"].ToString();
        //                    string role = reader["UserRole"].ToString();
        //                    MessageBox.Show("Login successful!");
        //                    TuteefyMain main = new TuteefyMain();
        //                    main.Show();
        //                    this.Close();
        //                }
        //                else
        //                {
        //                    MessageBox.Show("Invalid username or password.");
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("Error: " + ex.Message);
        //        }
        //    }
        //}

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = txtUser.Text.Trim();  // Use Email, not Username
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your email and password.", "Missing Information",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                User user = AuthenticateUser(email, password);

                if (user != null)
                {
                    // Set the user session
                    UserSession.CurrentUser = user;

                    MessageBox.Show($"Welcome, {user.FullName}!", "Login Successful",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Navigate to main window
                    // Replace "TuteefyMain" with your actual main window name
                    TuteefyMain mainWindow = new TuteefyMain();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid email or password.", "Login Failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during login: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private User AuthenticateUser(string email, string password)
        {
            // IMPORTANT: In your database, users log in with EMAIL, not Username
            string query = @"
                SELECT UserID, FullName, Email, PasswordHash, UserRole
                FROM UserTable
                WHERE Email = @Email";

            var parameters = new Dictionary<string, object>
            {
                { "@Email", email }
            };

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string storedPassword = row["PasswordHash"].ToString();

                    // IMPORTANT: This is simple password comparison
                    // In production, use BCrypt or similar password hashing!
                    if (storedPassword == password)
                    {
                        return new User
                        {
                            UserID = row["UserID"].ToString(),  // varchar(20)
                            FullName = row["FullName"].ToString(),
                            Email = row["Email"].ToString(),
                            PasswordHash = row["PasswordHash"].ToString(),
                            UserRole = row["UserRole"].ToString()
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Authentication error: {ex.Message}", ex);
            }

            return null;
        }
    }
}



