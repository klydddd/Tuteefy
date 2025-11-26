using TuteefyWPF.Models;

namespace TuteefyWPF.Classes
{
    public static class UserSession
    {
        public static User CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static bool IsTutor => CurrentUser != null && CurrentUser.UserRole == "Tutor";

        public static bool IsTutee => CurrentUser != null && CurrentUser.UserRole == "Tutee";

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}