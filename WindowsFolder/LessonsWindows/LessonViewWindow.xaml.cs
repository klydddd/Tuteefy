using Microsoft.Win32;
using System;
using System.Data;
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
        private byte[] _fileBytes;
        private readonly TuteefyWPF.Database db = new TuteefyWPF.Database();

        public LessonViewWindow(int lessonId, string title, string code, string content, string fileName)
        {
            InitializeComponent();

            _lessonId = lessonId;
            _fileName = fileName;

            TitleText.Text = title;
            CodeText.Text = code;
            ContentText.Text = content;

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
                OpenFileButton.Visibility = Visibility.Collapsed;
                DownloadButton.Visibility = Visibility.Collapsed;
                FileSection.Text = "No file attached";
            }
        }

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
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load file from database: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FileLink_Click(object sender, RoutedEventArgs e) => OpenFile();

        private void OpenFileButton_Click(object sender, RoutedEventArgs e) => OpenFile();

        private void OpenFile()
        {
            EnsureFileLoaded();

            if (_fileBytes == null || _fileBytes.Length == 0)
            {
                MessageBox.Show("File not found in database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + "_" + _fileName);
                File.WriteAllBytes(tempFile, _fileBytes);
                Process.Start(new ProcessStartInfo(tempFile) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            EnsureFileLoaded();

            if (_fileBytes == null || _fileBytes.Length == 0)
            {
                MessageBox.Show("File not available for download.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var save = new SaveFileDialog
                {
                    FileName = _fileName,
                    Filter = "All files|*.*",
                    Title = "Save lesson file as"
                };

                if (save.ShowDialog() == true)
                {
                    File.WriteAllBytes(save.FileName, _fileBytes);
                    MessageBox.Show("File saved to: " + save.FileName, "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}