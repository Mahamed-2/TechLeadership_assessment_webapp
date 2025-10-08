using TechLeadershipWebApp.Models;

namespace TechLeadershipWebApp.Services
{
    public interface IAIQuestionGenerator
    {
        Task<List<Question>> GenerateQuestionsAsync();
        Task<string> GenerateFeedbackAsync(TestResult result);
    }
}