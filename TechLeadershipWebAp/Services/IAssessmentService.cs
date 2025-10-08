using TechLeadershipWebApp.Models;

namespace TechLeadershipWebApp.Services
{
    public interface IAssessmentService
    {
        Task<List<Question>> GetQuestionsAsync();
        Task<TestResult> SubmitAssessmentAsync(AssessmentResponse response);
        Task<TestResult> GetResultByIdAsync(string participantId);
        Task<List<TestResult>> GetAllResultsAsync();
        Task<bool> DeleteAllResultsAsync(); // Add this method
        Task<bool> DeleteResultAsync(string participantId); // Add this method for single deletion
    }
}