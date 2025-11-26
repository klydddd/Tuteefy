using Microsoft.Win32;
using System;
using System.Collections.Generic; // Needed for List
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace TuteefyWPF.WindowsFolder.LessonsWindows
{
    public partial class LessonViewWindow : Window
    {
        private readonly int _lessonId;
        private readonly string _fileName;
        private readonly string _userRole;
        private readonly string _currentUserId;
        private byte[] _fileBytes;
        private readonly TuteefyWPF.Database db = new TuteefyWPF.Database();

        // Helper class for the ListBox
        public class StudentAssignmentViewModel
        {
            public string TuteeID { get; set; }
            public string FullName { get; set; }
            public bool IsAssigned { get; set; }
        }

        // Updated Constructor
        public LessonViewWindow(int lessonId, string title, string code, string content, string fileName, string role, string userId)
        {
            InitializeComponent();

            _lessonId = lessonId;
            _fileName = fileName;
            _userRole = role;
            _currentUserId = userId;

            TitleText.Text = title;
            CodeText.Text = code;
            ContentText.Text = content;

            SetupFileButtons();

            // If the user is a Tutor, show the assignment list
            if (_userRole == "Tutor")
            {
                StudentSelectionPanel.Visibility = Visibility.Visible;
                LoadStudentsAndAssignments();
            }
        }

        private void SetupFileButtons()
        {
            if (!string.IsNullOrWhiteSpace(_fileName))
            {
                OpenFileButton.Visibility = Visibility.Visible;
                DownloadButton.Visibility = Visibility.Visible;

                FileSection.Inlines.Clear();
                var hl = new Hyperlink(new Run(_fileName)) { ToolTip = _fileName };
                hl.Click += FileLink_Click;
                FileSection.Inlines.Add(hl);
            }
            else
            {
                FileSection.Text = "No file attached";
            }
        }

        private void LoadStudentsAndAssignments()
        {
            List<StudentAssignmentViewModel> studentList = new List<StudentAssignmentViewModel>();
            List<string> assignedIds = new List<string>();

            using (SqlConnection conn = new SqlConnection(db.connectionString))
            {
                conn.Open();

                // 1. Get IDs of students ALREADY assigned to this lesson
                string checkQuery = "SELECT TuteeID FROM LessonAssignmentsTable WHERE LessonID = @lid";
                using (SqlCommand cmd = new SqlCommand(checkQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@lid", _lessonId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            assignedIds.Add(reader["TuteeID"].ToString());
                        }
                    }
                }

                // 2. Get ALL students for this Tutor
                string studQuery = "SELECT TuteeID, FullName FROM TuteeTable WHERE TutorID = @tid";
                using (SqlCommand cmd = new SqlCommand(studQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@tid", _currentUserId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string id = reader["TuteeID"].ToString();
                            string name = reader["FullName"].ToString();

                            studentList.Add(new StudentAssignmentViewModel
                            {
                                TuteeID = id,
                                FullName = name,
                                // Check if ID exists in the assigned list
                                IsAssigned = assignedIds.Contains(id)
                            });
                        }
                    }
                }
            }

            StudentsListBox.ItemsSource = studentList;
        }

        private void SaveAssignmentsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();

                    // 1. Clear existing assignments for this lesson (easiest way to update)
                    string deleteQuery = "DELETE FROM LessonAssignmentsTable WHERE LessonID = @lid";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@lid", _lessonId);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Insert currently checked students
                    string insertQuery = "INSERT INTO LessonAssignmentsTable (LessonID, TuteeID) VALUES (@lid, @tid)";

                    foreach (StudentAssignmentViewModel student in StudentsListBox.Items)
                    {
                        if (student.IsAssigned)
                        {
                            using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@lid", _lessonId);
                                cmd.Parameters.AddWithValue("@tid", student.TuteeID);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                MessageBox.Show("Student assignments updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving assignments: " + ex.Message);
            }
        }

        // ... (Keep EnsureFileLoaded, OpenFile, DownloadButton_Click, Close_Click exactly as they were) ...

        private void EnsureFileLoaded()
        {
            if (_fileBytes != null || _lessonId == 0) return;
            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT FileData FROM LessonsTable WHERE LessonID = @LessonID", conn))
                    {
                        cmd.Parameters.AddWithValue("@LessonID", _lessonId);
                        object o = cmd.ExecuteScalar();
                        if (o != DBNull.Value && o != null)
                            _fileBytes = (byte[])o;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("DB Error: " + ex.Message); }
        }

        private void FileLink_Click(object sender, RoutedEventArgs e) => OpenFile();
        private void OpenFileButton_Click(object sender, RoutedEventArgs e) => OpenFile();
        private void OpenFile()
        {
            EnsureFileLoaded();
            if (_fileBytes == null || _fileBytes.Length == 0) return;
            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "_" + _fileName);
                File.WriteAllBytes(tempFile, _fileBytes);
                Process.Start(new ProcessStartInfo(tempFile) { UseShellExecute = true });
            }
            catch (Exception ex) { MessageBox.Show("Error opening file: " + ex.Message); }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            EnsureFileLoaded();
            if (_fileBytes == null) return;
            var save = new SaveFileDialog { FileName = _fileName, Filter = "All files|*.*" };
            if (save.ShowDialog() == true)
            {
                File.WriteAllBytes(save.FileName, _fileBytes);
                MessageBox.Show("Saved!");
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}