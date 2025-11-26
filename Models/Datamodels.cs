using System;
using System.Collections.Generic;

namespace TuteefyWPF.Models
{
    public class User
    {
        public string UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string UserRole { get; set; } // "Tutor" or "Tutee"
    }

    public class Tutor
    {
        public string TutorID { get; set; }
    }

    public class Tutee
    {
        public string TuteeID { get; set; }
        public string TutorID { get; set; }
        public string FullName { get; set; }
        public string Subject { get; set; }
        public string Address { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public byte[] ProfilePhoto { get; set; }
    }

    public class Quiz
    {
        public string QuizID { get; set; }
        public string TutorID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsAvailable { get; set; }
        public string TuteeID { get; set; } // Assigned tutee
        public int TotalQuestions { get; set; }
    }

    public class Question
    {
        public string QuestionID { get; set; }
        public string QuizID { get; set; }
        public string QuestionText { get; set; }
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        public string ChoiceID { get; set; }
        public string QuestionID { get; set; }
        public string ChoiceText { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class TuteeAnswer
    {
        public string QuestionID { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class QuizScore
    {
        public string ScoreID { get; set; }
        public string TuteeID { get; set; }
        public string QuizID { get; set; }
        public int Score { get; set; }
        public DateTime DateTaken { get; set; }
    }
}