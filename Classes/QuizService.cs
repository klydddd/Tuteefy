using System;
using System.Collections.Generic;
using System.Data;
using TuteefyWPF.Models;

namespace TuteefyWPF.Classes
{
    public class QuizService
    {
        // Get all quizzes assigned to a specific tutee
        public static List<Quiz> GetQuizzesForTutee(string tuteeID)
        {
            List<Quiz> quizzes = new List<Quiz>();

            string query = @"
                SELECT 
                    q.QuizID, 
                    q.TutorID,
                    q.Title, 
                    q.Description,
                    q.IsAvailable,
                    q.TuteeID,
                    (SELECT COUNT(*) FROM QuestionsTable WHERE QuizID = q.QuizID) as TotalQuestions
                FROM QuizzesTable q
                WHERE q.TuteeID = @TuteeID AND q.IsAvailable = 1
                ORDER BY q.Title";

            var parameters = new Dictionary<string, object>
            {
                { "@TuteeID", tuteeID }
            };

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    quizzes.Add(new Quiz
                    {
                        QuizID = row["QuizID"].ToString(),
                        TutorID = row["TutorID"].ToString(),
                        Title = row["Title"].ToString(),
                        Description = row["Description"]?.ToString() ?? "",
                        IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                        TuteeID = row["TuteeID"]?.ToString(),
                        TotalQuestions = row["TotalQuestions"] != DBNull.Value ? Convert.ToInt32(row["TotalQuestions"]) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading quizzes: {ex.Message}", ex);
            }

            return quizzes;
        }

        // Get all questions for a specific quiz
        public static List<Question> GetQuestionsForQuiz(string quizID)
        {
            List<Question> questions = new List<Question>();

            string query = @"
                SELECT 
                    QuestionID, 
                    QuizID, 
                    QuestionText
                FROM QuestionsTable
                WHERE QuizID = @QuizID
                ORDER BY QuestionID";

            var parameters = new Dictionary<string, object>
            {
                { "@QuizID", quizID }
            };

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    Question question = new Question
                    {
                        QuestionID = row["QuestionID"].ToString(),
                        QuizID = row["QuizID"].ToString(),
                        QuestionText = row["QuestionText"].ToString(),
                        Choices = new List<Choice>()
                    };

                    // Load choices for this question
                    question.Choices = GetChoicesForQuestion(question.QuestionID);

                    questions.Add(question);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading questions: {ex.Message}", ex);
            }

            return questions;
        }

        // Get all choices for a specific question
        public static List<Choice> GetChoicesForQuestion(string questionID)
        {
            List<Choice> choices = new List<Choice>();

            string query = @"
                SELECT 
                    ChoiceID, 
                    QuestionID, 
                    ChoiceText, 
                    IsCorrect
                FROM ChoicesTable
                WHERE QuestionID = @QuestionID
                ORDER BY ChoiceID";

            var parameters = new Dictionary<string, object>
            {
                { "@QuestionID", questionID }
            };

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    choices.Add(new Choice
                    {
                        ChoiceID = row["ChoiceID"].ToString(),
                        QuestionID = row["QuestionID"].ToString(),
                        ChoiceText = row["ChoiceText"].ToString(),
                        IsCorrect = Convert.ToBoolean(row["IsCorrect"])
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading choices: {ex.Message}", ex);
            }

            return choices;
        }

        // Check if tutee has already taken a quiz
        public static bool HasTuteeCompletedQuiz(string tuteeID, string quizID)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM TuteeQuizScoreTable 
                WHERE TuteeID = @TuteeID AND QuizID = @QuizID";

            var parameters = new Dictionary<string, object>
            {
                { "@TuteeID", tuteeID },
                { "@QuizID", quizID }
            };

            try
            {
                object result = DatabaseHelper.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking quiz completion: {ex.Message}", ex);
            }
        }

        // Generate a new unique ScoreID
        private static string GenerateScoreID()
        {
            // Get the max ScoreID and increment
            string query = "SELECT TOP 1 ScoreID FROM TuteeQuizScoreTable ORDER BY ScoreID DESC";

            try
            {
                object result = DatabaseHelper.ExecuteScalar(query);
                if (result != null && result != DBNull.Value)
                {
                    string lastID = result.ToString();
                    // Assuming format like "SCR001", "SCR002", etc.
                    if (lastID.Length >= 3)
                    {
                        string prefix = lastID.Substring(0, 3);
                        string numberPart = lastID.Substring(3);
                        if (int.TryParse(numberPart, out int num))
                        {
                            return $"{prefix}{(num + 1):D3}";
                        }
                    }
                }
                // Default first ID
                return "SCR001";
            }
            catch
            {
                // If table is empty or error, start with first ID
                return "SCR001";
            }
        }

        // Save tutee's quiz score
        public static void SaveQuizScore(string tuteeID, string quizID, int score)
        {
            string scoreID = GenerateScoreID();

            string query = @"
                INSERT INTO TuteeQuizScoreTable (ScoreID, TuteeID, QuizID, Score, DateTaken)
                VALUES (@ScoreID, @TuteeID, @QuizID, @Score, @DateTaken)";

            var parameters = new Dictionary<string, object>
            {
                { "@ScoreID", scoreID },
                { "@TuteeID", tuteeID },
                { "@QuizID", quizID },
                { "@Score", score },
                { "@DateTaken", DateTime.Now }
            };

            try
            {
                DatabaseHelper.ExecuteNonQuery(query, parameters);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving quiz score: {ex.Message}", ex);
            }
        }

        // Get tutee's score for a specific quiz
        public static QuizScore GetTuteeQuizScore(string tuteeID, string quizID)
        {
            string query = @"
                SELECT ScoreID, TuteeID, QuizID, Score, DateTaken
                FROM TuteeQuizScoreTable
                WHERE TuteeID = @TuteeID AND QuizID = @QuizID";

            var parameters = new Dictionary<string, object>
            {
                { "@TuteeID", tuteeID },
                { "@QuizID", quizID }
            };

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    return new QuizScore
                    {
                        ScoreID = row["ScoreID"].ToString(),
                        TuteeID = row["TuteeID"].ToString(),
                        QuizID = row["QuizID"].ToString(),
                        Score = Convert.ToInt32(row["Score"]),
                        DateTaken = Convert.ToDateTime(row["DateTaken"])
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving quiz score: {ex.Message}", ex);
            }

            return null;
        }

        // Get all quizzes (for tutors)
        public static List<Quiz> GetAllQuizzes()
        {
            List<Quiz> quizzes = new List<Quiz>();

            string query = @"
                SELECT 
                    q.QuizID, 
                    q.TutorID,
                    q.Title, 
                    q.Description,
                    q.IsAvailable,
                    q.TuteeID,
                    (SELECT COUNT(*) FROM QuestionsTable WHERE QuizID = q.QuizID) as TotalQuestions
                FROM QuizzesTable q
                WHERE q.IsAvailable = 1
                ORDER BY q.Title";

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(query);

                foreach (DataRow row in dt.Rows)
                {
                    quizzes.Add(new Quiz
                    {
                        QuizID = row["QuizID"].ToString(),
                        TutorID = row["TutorID"].ToString(),
                        Title = row["Title"].ToString(),
                        Description = row["Description"]?.ToString() ?? "",
                        IsAvailable = Convert.ToBoolean(row["IsAvailable"]),
                        TuteeID = row["TuteeID"]?.ToString(),
                        TotalQuestions = row["TotalQuestions"] != DBNull.Value ? Convert.ToInt32(row["TotalQuestions"]) : 0
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading quizzes: {ex.Message}", ex);
            }

            return quizzes;
        }

        // Get total points for a quiz (1 point per question)
        public static int GetQuizTotalPoints(string quizID)
        {
            string query = @"
                SELECT COUNT(*) 
                FROM QuestionsTable 
                WHERE QuizID = @QuizID";

            var parameters = new Dictionary<string, object>
            {
                { "@QuizID", quizID }
            };

            try
            {
                object result = DatabaseHelper.ExecuteScalar(query, parameters);
                return Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating quiz points: {ex.Message}", ex);
            }
        }
    }
}