using System;
using System.Data.SqlClient;
using System.Windows;

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



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            username = txtUser.Text.Trim();
            password = txtPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT UserRole, FullName FROM UserTable WHERE UserID=@username AND PasswordHash=@password";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string fullname = reader["FullName"].ToString();
                            string role = reader["UserRole"].ToString();
                            MessageBox.Show("Login successful!");
                            TuteefyMain main = new TuteefyMain(username, role, fullname);
                            main.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Invalid username or password.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
    }
}



