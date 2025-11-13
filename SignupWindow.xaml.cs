using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
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
using Wpf.Ui.Controls;

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
            string tutorID = string.Empty;
            string fullName = txtFullName.Text;
            string email = txtEmail.Text;
            string password = txtPassword.Password;
            string role = "Tutor";

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) ){
                System.Windows.MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (!email.Contains("@"))
            {
                System.Windows.MessageBox.Show("Please enter a valid email address.");
                return;
            }

            if (password != txtConfirmPass.Password)
            {
                System.Windows.MessageBox.Show("Passwords do not match.");
                return;
            }

            if(checkPriv.IsChecked == false)
            {
                System.Windows.MessageBox.Show("You must agree to the terms and conditions.");
                return;
            }

            string GetNextTutorID(SqlConnection conn)
            {
                string query = "SELECT TOP 1 TutorID FROM TutorTable ORDER BY TutorID DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                var lastId = cmd.ExecuteScalar() as string;

                if (string.IsNullOrEmpty(lastId))
                    return "TR001";

                int num = int.Parse(lastId.Substring(2));
                return "TR" + (num + 1).ToString("D3");
            }

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                tutorID = GetNextTutorID(conn);

                string insertUser = @"INSERT INTO UserTable (UserID, FullName, Email, PasswordHash, UserRole)
                                    VALUES (@UserID, @FullName, @Email, @PasswordHash, 'Tutor')";
                using (SqlCommand cmd = new SqlCommand(insertUser, conn))
                {
                    cmd.Parameters.AddWithValue("@UserID", tutorID);
                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@PasswordHash", password);
                    cmd.ExecuteNonQuery();
                }

                string insertTutor = "INSERT INTO TutorTable (TutorID) VALUES (@TutorID)";
                using (SqlCommand cmd = new SqlCommand(insertTutor, conn))
                {
                    cmd.Parameters.AddWithValue("@TutorID", tutorID);
                    cmd.ExecuteNonQuery();
                }
            }

            System.Windows.MessageBox.Show("Signup Successful!\nPlease check your email for your username.");
            SendEmail(email, tutorID);
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void SendEmail(string email, string tutorID)
        {
            try
            {
                string senderEmail = "tuteefy2025@gmail.com";
                string senderPassword = "mixo kzfz fids tkdp";

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail);
                mail.To.Add(email);
                mail.Subject = "Tuteefy Registration";
                mail.IsBodyHtml = true;
                mail.Body = "Thank you for signing up for Tuteefy!<p>Here is your username: " + tutorID;

                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                smtpClient.EnableSsl = true;

                smtpClient.Send(mail);
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
