using System.Windows;
using System.Windows.Controls;

namespace TuteefyWPF
{
    public partial class StudentCardControl : UserControl
    {
        public StudentCardControl()
        {
            InitializeComponent();
        }

        // Dependency Properties
        public string StudentName
        {
            get => (string)GetValue(StudentNameProperty);
            set => SetValue(StudentNameProperty, value);
        }
        public static readonly DependencyProperty StudentNameProperty =
            DependencyProperty.Register(nameof(StudentName), typeof(string), typeof(StudentCardControl));

        public string Subject
        {
            get => (string)GetValue(SubjectProperty);
            set => SetValue(SubjectProperty, value);
        }
        public static readonly DependencyProperty SubjectProperty =
            DependencyProperty.Register(nameof(Subject), typeof(string), typeof(StudentCardControl));

        public string TotalGrade
        {
            get => (string)GetValue(TotalGradeProperty);
            set => SetValue(TotalGradeProperty, value);
        }
        public static readonly DependencyProperty TotalGradeProperty =
            DependencyProperty.Register(nameof(TotalGrade), typeof(string), typeof(StudentCardControl));

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
            MessageBox.Show($"👤 {StudentName}\n📘 {Subject}\n📊 Grade: {TotalGrade}",
                            "Student Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
