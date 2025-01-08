using System.Text.Json.Serialization;

namespace QuizAPI.Models
{
    public class Answer
    {
        public int AnswerId { get; set; }
        public int QuestionId { get; set; }
        public string? AnswerText { get; set; }
        public bool IsCorrect { get; set; }

        [JsonIgnore] // Prevent circular references during serialization
        public Question? Question { get; set; }
    }
}
