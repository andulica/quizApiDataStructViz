namespace QuizAPI.Models
{
    public class Topic
    {
        public int TopicId { get; set; }
        public string? Name { get; set; }
        public ICollection<Question>? Questions { get; set; }

    }
}
