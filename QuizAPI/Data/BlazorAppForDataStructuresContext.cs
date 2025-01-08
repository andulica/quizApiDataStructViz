using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizAPI.Models;

namespace QuizAPI.Data
{
    public class BlazorAppForDataStructuresContext : IdentityDbContext<ApplicationUser>
    {
        public BlazorAppForDataStructuresContext(DbContextOptions<BlazorAppForDataStructuresContext> options)
            : base(options)
        {
        }

        // DbSet for querying ApplicationUser
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Topic> Topics { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Topic>()
                .ToTable("Topics")
                .HasKey(t => t.TopicId);

            modelBuilder.Entity<Topic>()
                .Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Question>()
                .ToTable("Questions")
                .HasKey(q => q.QuestionId);

            modelBuilder.Entity<Question>()
                .Property(q => q.QuestionText)
                .IsRequired()
                .HasMaxLength(500);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.Topic)
                .WithMany(t => t.Questions)
                .HasForeignKey(q => q.TopicId);

            modelBuilder.Entity<Answer>()
                .ToTable("Answers")
                .HasKey(a => a.AnswerId);

            modelBuilder.Entity<Answer>()
                .Property(a => a.AnswerText)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId);
        }
    }
}
