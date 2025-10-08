namespace TechLeadershipWebApp.Models
{
    public class ScoreItemViewModel
    {
        public string Label { get; set; } = string.Empty;
        public int Score { get; set; }
        public int MaxScore { get; set; }
        public int Percentage { get; set; }
    }
}