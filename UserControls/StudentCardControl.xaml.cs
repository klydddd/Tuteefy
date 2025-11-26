using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TuteefyWPF
{
    public partial class StudentCardControl : UserControl
    {
        // Dependency Properties
        public static readonly DependencyProperty StudentNameProperty =
            DependencyProperty.Register("StudentName", typeof(string), typeof(StudentCardControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty SubjectProperty =
            DependencyProperty.Register("Subject", typeof(string), typeof(StudentCardControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty TotalGradeProperty =
            DependencyProperty.Register("TotalGrade", typeof(string), typeof(StudentCardControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ProfilePhotoProperty =
            DependencyProperty.Register("ProfilePhoto", typeof(byte[]), typeof(StudentCardControl),
                new PropertyMetadata(null, OnProfilePhotoChanged));

        public string StudentName
        {
            get { return (string)GetValue(StudentNameProperty); }
            set { SetValue(StudentNameProperty, value); }
        }

        public string Subject
        {
            get { return (string)GetValue(SubjectProperty); }
            set { SetValue(SubjectProperty, value); }
        }

        public string TotalGrade
        {
            get { return (string)GetValue(TotalGradeProperty); }
            set { SetValue(TotalGradeProperty, value); }
        }

        public byte[] ProfilePhoto
        {
            get { return (byte[])GetValue(ProfilePhotoProperty); }
            set { SetValue(ProfilePhotoProperty, value); }
        }

        public StudentCardControl()
        {
            InitializeComponent();
        }

        private static void OnProfilePhotoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as StudentCardControl;
            if (control != null)
            {
                control.LoadProfilePhoto((byte[])e.NewValue);
            }
        }

        private void LoadProfilePhoto(byte[] photoData)
        {
            if (photoData != null && photoData.Length > 0)
            {
                try
                {
                    // Convert byte array to BitmapImage
                    using (var stream = new MemoryStream(photoData))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Important for cross-thread access

                        // 1. Set the ImageSource on the ImageBrush
                        ProfileImageBrush.ImageSource = bitmap;

                        // 2. Show the Border container (which holds the brush)
                        ImageBrushContainer.Visibility = Visibility.Visible;

                        // 3. Hide the fallback icon
                        FallbackIcon.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    // If image loading fails, keep the fallback icon
                    MessageBox.Show($"Error loading profile photo: {ex.Message}", "Image Error",
                        MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Hide the image border and show the icon
                    ImageBrushContainer.Visibility = Visibility.Collapsed;
                    FallbackIcon.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // No photo data - show fallback icon
                // Hide the image border and show the icon
                ImageBrushContainer.Visibility = Visibility.Collapsed;
                FallbackIcon.Visibility = Visibility.Visible;
            }
        }

        private void CardButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle view button click
            MessageBox.Show($"Viewing details for: {StudentName}");
        }
    }
}