using Microsoft.Win32;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace TuteefyWPF.WindowsFolder.LessonsWindows
{
    public partial class AddLessonsWindow : Window
    {
        private readonly TuteefyWPF.Database db = new TuteefyWPF.Database();
        private string _selectedFilePath;
        private string _selectedFileName;
        private byte[] _selectedFileBytes;

        // tutor id should be passed in same way other windows do
        private readonly string _tutorId;

        public AddLessonsWindow(string tutorId)
        {
            InitializeComponent();
            _tutorId = tutorId;
        }

        private void UploadFileButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                Title = "Select lesson file",
                Filter = "All supported files|*.pdf;*.pptx;*.ppt;*.docx;*.doc;*.mp4;*.mp3;*.zip;*.rar;*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dlg.ShowDialog() == true)
            {
                _selectedFilePath = dlg.FileName;
                _selectedFileName = Path.GetFileName(_selectedFilePath);

                try
                {
                    _selectedFileBytes = File.ReadAllBytes(_selectedFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to read file: " + ex.Message, "File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _selectedFileBytes = null;
                    _selectedFileName = null;
                    _selectedFilePath = null;
                    SelectedFileTextBlock.Text = "No file selected";
                    return;
                }

                SelectedFileTextBlock.Text = _selectedFileName;
            }
        }

        private void CreateLessonButton_Click(object sender, RoutedEventArgs e)
        {
            string title = LessonTitleTextBox.Text?.Trim() ?? string.Empty;
            string code = LessonCodeTextBox.Text?.Trim() ?? string.Empty;
            string content = ContentTextBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Please fill in title, code and content.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(db.connectionString))
                {
                    conn.Open();

                    string insert = @"
                        INSERT INTO LessonsTable (TutorID, Title, Content, Code, FileName, FileData, DateCreated)
                        OUTPUT INSERTED.LessonID
                        VALUES (@TutorID, @Title, @Content, @Code, @FileName, @FileData, @DateCreated);";

                    using (SqlCommand cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@TutorID", _tutorId ?? string.Empty);
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Content", content);
                        cmd.Parameters.AddWithValue("@Code", string.IsNullOrWhiteSpace(code) ? (object)DBNull.Value : code);
                        cmd.Parameters.AddWithValue("@FileName", string.IsNullOrWhiteSpace(_selectedFileName) ? (object)DBNull.Value : _selectedFileName);
                        cmd.Parameters.AddWithValue("@FileData", (object)_selectedFileBytes ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@DateCreated", DateTime.UtcNow);

                        // ExecuteScalar will return LessonID from OUTPUT
                        object sid = cmd.ExecuteScalar();
                        int newLessonId = sid != null ? Convert.ToInt32(sid) : 0;
                    }
                }

                MessageBox.Show("Lesson saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                OnLessonCreated();

                this.DialogResult = true;
                this.Close();
            }
            catch (SqlException sqlex)
            {
                MessageBox.Show("Database error saving lesson: " + sqlex.Message, "DB Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving lesson: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler LessonCreated;

        private void OnLessonCreated()
        {
            LessonCreated?.Invoke(this, EventArgs.Empty);
        }
    }
}