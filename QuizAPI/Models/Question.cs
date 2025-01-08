using System.Text.Json.Serialization;

namespace QuizAPI.Models
{
    public class Question
    {
        public int QuestionId { get; set; } // Primary Key
        public int TopicId { get; set; }    // Foreign Key

        public string QuestionText { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Answer> Answers { get; set; } = new List<Answer>(); // Initialized to avoid null references

        [JsonIgnore] // Ignore during JSON conversion

        public Topic? Topic { get; set; } // Nullable Topic to avoid validation errors
    }
}
