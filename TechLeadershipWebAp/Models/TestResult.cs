using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechLeadershipWebApp.Models
{
    public class TestResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(6)]
        public string ParticipantId { get; set; } = GenerateParticipantId();
        
        [Required]
        [StringLength(100)]
        public string ParticipantName { get; set; } = string.Empty;
        
        [Required]
        [Range(0, 15)]
        public int TechnicalLeadScore { get; set; }
        
        [Required]
        [Range(0, 15)]
        public int TeamLeadScore { get; set; }
        
        [Required]
        [Range(0, 15)]
        public int ArchitectScore { get; set; }
        
        [Required]
        [Range(0, 15)]
        public int MentorScore { get; set; }
        
        [Required]
        [Range(0, 15)]
        public int ProjectManagerScore { get; set; }
        
        [Required]
        public LeadershipType DominantLeadershipType { get; set; }
        
        [NotMapped]
        public int TotalScore => TechnicalLeadScore + TeamLeadScore + ArchitectScore + MentorScore + ProjectManagerScore;
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public string Feedback { get; set; } = string.Empty;
        
        private static string GenerateParticipantId()
        {
            var random = new Random();
            return random.Next(100, 1000).ToString("D3");
        }
    }

    // Add this class to your existing TestResult.cs file
    public class AssessmentResponse
    {
    [Required(ErrorMessage = "Participant name is required")]
    [Display(Name = "Participant Name")]
    public string ParticipantName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please answer all questions")]
    public Dictionary<int, int> Answers { get; set; } = new Dictionary<int, int>();
    }
}