using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace TuteefyWPF
{
    /// <summary>
    /// Interaction logic for SignupWindow.xaml
    /// </summary>
    public partial class SignupWindow : Window
    {
        private Database db;
        public SignupWindow()
        {
            InitializeComponent();
            db = new Database();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string fullName = txtFullName.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            string role = "Tutor";

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                string query = "INSERT INTO UserTable (UserRole, FullName, Email, PasswordHash)" +
                                "VALUES (@UserRole, @FullName, @Email, @PasswordHash)" +
                                "SELECT SCOPE_IDENTITY();" ;
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserRole", role);
                cmd.Parameters.AddWithValue("@FullName", fullName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", password);
                
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            MessageBox.Show("Signup Successful!");
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
