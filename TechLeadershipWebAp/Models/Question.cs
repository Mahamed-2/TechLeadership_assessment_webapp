using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechLeadershipWebApp.Models
{
    public class Question
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public string Text { get; set; } = string.Empty;
        
        public string Language { get; set; } = "en"; // Default to English
        
        public virtual List<Alternative> Alternatives { get; set; } = new List<Alternative>();
    }

    public class Alternative
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        public string Text { get; set; } = string.Empty;
        
        [Required]
        public LeadershipType LeadershipType { get; set; }
        
        public int QuestionId { get; set; }
        
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; } = null!;
    }

    public enum LeadershipType
    {
        TechnicalLead = 0,
        TeamLead = 1,
        Architect = 2,
        Mentor = 3,
        ProjectManager = 4
    }

    public enum Language
    {
        English = 0,
        Swedish = 1
    }
}