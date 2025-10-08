using Microsoft.EntityFrameworkCore;
using TechLeadershipWebApp.Models;

namespace TechLeadershipWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<TestResult> TestResults { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Alternative> Alternatives { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed initial questions and alternatives
            modelBuilder.Entity<Question>().HasData(
                new Question { Id = 1, Text = "When your team faces a complex technical challenge, what's your primary approach?" },
                new Question { Id = 2, Text = "A junior developer is struggling with a task. How do you respond?" },
                new Question { Id = 3, Text = "Your team needs to choose a new technology stack. What's your role?" }
            );

            modelBuilder.Entity<Alternative>().HasData(
                // Question 1 alternatives
                new Alternative { Id = 1, Text = "Break it down into smaller problems and assign based on expertise", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 1 },
                new Alternative { Id = 2, Text = "Facilitate a team discussion to brainstorm solutions together", LeadershipType = LeadershipType.TeamLead, QuestionId = 1 },
                new Alternative { Id = 3, Text = "Design an architectural solution that addresses the root cause", LeadershipType = LeadershipType.Architect, QuestionId = 1 },
                new Alternative { Id = 4, Text = "Pair up developers to work through the challenge collaboratively", LeadershipType = LeadershipType.Mentor, QuestionId = 1 },
                new Alternative { Id = 5, Text = "Assess impact on timeline and resources, then adjust plans accordingly", LeadershipType = LeadershipType.ProjectManager, QuestionId = 1 },
                
                // Question 2 alternatives
                new Alternative { Id = 6, Text = "Review their code and provide specific technical suggestions", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 2 },
                new Alternative { Id = 7, Text = "Check if the task was properly explained and offer support", LeadershipType = LeadershipType.TeamLead, QuestionId = 2 },
                new Alternative { Id = 8, Text = "Consider if the architecture or tools are creating unnecessary complexity", LeadershipType = LeadershipType.Architect, QuestionId = 2 },
                new Alternative { Id = 9, Text = "Schedule pairing sessions to help them learn through hands-on experience", LeadershipType = LeadershipType.Mentor, QuestionId = 2 },
                new Alternative { Id = 10, Text = "Evaluate if the task needs to be reassigned or deadline adjusted", LeadershipType = LeadershipType.ProjectManager, QuestionId = 2 },
                
                // Question 3 alternatives
                new Alternative { Id = 11, Text = "Research and present the technical pros and cons of each option", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 3 },
                new Alternative { Id = 12, Text = "Ensure everyone's opinion is heard and guide toward consensus", LeadershipType = LeadershipType.TeamLead, QuestionId = 3 },
                new Alternative { Id = 13, Text = "Evaluate how each option fits into the long-term system architecture", LeadershipType = LeadershipType.Architect, QuestionId = 3 },
                new Alternative { Id = 14, Text = "Help team members understand the learning curve and growth opportunities", LeadershipType = LeadershipType.Mentor, QuestionId = 3 },
                new Alternative { Id = 15, Text = "Analyze timeline, cost, and resource implications of each option", LeadershipType = LeadershipType.ProjectManager, QuestionId = 3 }
            );
        }
    }
}