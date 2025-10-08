using TechLeadershipWebApp.Models;

namespace TechLeadershipWebApp.Services
{
    public interface IAssessmentService
    {
        Task<List<Question>> GetQuestionsAsync(string language = "en");
        Task<TestResult> SubmitAssessmentAsync(AssessmentResponse response);
        Task<TestResult> GetResultByIdAsync(string participantId);
        Task<List<TestResult>> GetAllResultsAsync();
        Task<bool> DeleteAllResultsAsync();
        Task<bool> DeleteResultAsync(string participantId);
    }
}