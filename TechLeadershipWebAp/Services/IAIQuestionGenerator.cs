using TechLeadershipWebApp.Models;

namespace TechLeadershipWebApp.Services
{
    public interface IAIQuestionGenerator
    {
        Task<List<Question>> GenerateQuestionsAsync(string language = "en");
        Task<string> GenerateFeedbackAsync(TestResult result, string language = "en");
    }
}