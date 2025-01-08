namespace QuizAPI.Models
{
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public int UserScore { get; set; } = 0;  // Default score is 0
    }
}
