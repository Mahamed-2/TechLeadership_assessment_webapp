using TechLeadershipWebApp.Data;
using TechLeadershipWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TechLeadershipWebApp.Services
{
    public class AssessmentService : IAssessmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAIQuestionGenerator _aiGenerator;
        private List<Question> _currentQuestions;

        public AssessmentService(ApplicationDbContext context, IAIQuestionGenerator aiGenerator)
        {
            _context = context;
            _aiGenerator = aiGenerator;
            _currentQuestions = new List<Question>();
        }

        public async Task<List<Question>> GetQuestionsAsync()
        {
            try
            {
                // Always generate new questions from AI for variety
                _currentQuestions = await _aiGenerator.GenerateQuestionsAsync();
                Console.WriteLine($"Generated {_currentQuestions.Count} questions");
                return _currentQuestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Question Generation Error: {ex.Message}");
                
                // If AI fails, use default questions but ensure we have questions
                if (!_currentQuestions.Any())
                {
                    _currentQuestions = GetDefaultQuestions();
                }
                Console.WriteLine($"Using {_currentQuestions.Count} default questions");
                return _currentQuestions;
            }
        }

        public async Task<TestResult> SubmitAssessmentAsync(AssessmentResponse response)
        {
            Console.WriteLine($"Starting assessment submission for: {response.ParticipantName}");
            Console.WriteLine($"Number of answers: {response.Answers.Count}");

            if (!_currentQuestions.Any())
            {
                _currentQuestions = await GetQuestionsAsync();
            }

            var result = new TestResult
            {
                ParticipantName = response.ParticipantName.Trim(),
                TechnicalLeadScore = 0,
                TeamLeadScore = 0,
                ArchitectScore = 0,
                MentorScore = 0,
                ProjectManagerScore = 0
            };

            // Calculate scores based on answers
            foreach (var answer in response.Answers)
            {
                var questionId = answer.Key;
                var alternativeId = answer.Value;

                Console.WriteLine($"Processing answer - Question ID: {questionId}, Alternative ID: {alternativeId}");

                var question = _currentQuestions.FirstOrDefault(q => q.Id == questionId);
                if (question != null)
                {
                    var alternative = question.Alternatives.FirstOrDefault(a => a.Id == alternativeId);
                    if (alternative != null)
                    {
                        Console.WriteLine($"Found alternative: {alternative.Text} with type: {alternative.LeadershipType}");
                        
                        // Add 1 point for each selected alternative in the respective category
                        switch (alternative.LeadershipType)
                        {
                            case LeadershipType.TechnicalLead:
                                result.TechnicalLeadScore++;
                                break;
                            case LeadershipType.TeamLead:
                                result.TeamLeadScore++;
                                break;
                            case LeadershipType.Architect:
                                result.ArchitectScore++;
                                break;
                            case LeadershipType.Mentor:
                                result.MentorScore++;
                                break;
                            case LeadershipType.ProjectManager:
                                result.ProjectManagerScore++;
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Alternative not found for ID: {alternativeId}");
                    }
                }
                else
                {
                    Console.WriteLine($"Question not found for ID: {questionId}");
                }
            }

            Console.WriteLine($"Final scores - Technical: {result.TechnicalLeadScore}, Team: {result.TeamLeadScore}, Architect: {result.ArchitectScore}, Mentor: {result.MentorScore}, PM: {result.ProjectManagerScore}");

            // Determine dominant leadership type
            var scores = new Dictionary<LeadershipType, int>
            {
                { LeadershipType.TechnicalLead, result.TechnicalLeadScore },
                { LeadershipType.TeamLead, result.TeamLeadScore },
                { LeadershipType.Architect, result.ArchitectScore },
                { LeadershipType.Mentor, result.MentorScore },
                { LeadershipType.ProjectManager, result.ProjectManagerScore }
            };

            result.DominantLeadershipType = scores.OrderByDescending(s => s.Value).First().Key;
            Console.WriteLine($"Dominant leadership type: {result.DominantLeadershipType}");
            
            // Generate AI feedback
            try
            {
                result.Feedback = await _aiGenerator.GenerateFeedbackAsync(result);
                Console.WriteLine("AI feedback generated successfully");
            }
            catch (Exception ex)
            {
                result.Feedback = "Skills to develop:\n• Technical leadership\n• Team collaboration\n• Strategic planning\n• Mentoring abilities\n• Project coordination";
                Console.WriteLine($"AI feedback generation failed, using default: {ex.Message}");
            }

            // Save to database
            try
            {
                _context.TestResults.Add(result);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Result saved with ID: {result.ParticipantId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database save error: {ex.Message}");
                throw;
            }

            return result;
        }

        public async Task<TestResult> GetResultByIdAsync(string participantId)
        {
            Console.WriteLine($"Looking up result for ID: {participantId}");
            var result = await _context.TestResults
                .FirstOrDefaultAsync(r => r.ParticipantId == participantId);
            
            if (result == null)
            {
                Console.WriteLine($"No result found for ID: {participantId}");
            }
            else
            {
                Console.WriteLine($"Found result for: {result.ParticipantName}");
            }
            
            return result;
        }

        public async Task<List<TestResult>> GetAllResultsAsync()
        {
            var results = await _context.TestResults
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            
            Console.WriteLine($"Retrieved {results.Count} total results");
            return results;
        }

        public async Task<bool> DeleteAllResultsAsync()
        {
            try
            {
                Console.WriteLine("Deleting all results from database...");
                
                var allResults = await _context.TestResults.ToListAsync();
                _context.TestResults.RemoveRange(allResults);
                
                var rowsAffected = await _context.SaveChangesAsync();
                Console.WriteLine($"Deleted {rowsAffected} results from database");
                
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting all results: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteResultAsync(string participantId)
        {
            try
            {
                Console.WriteLine($"Deleting result with ID: {participantId}");
                
                var result = await _context.TestResults
                    .FirstOrDefaultAsync(r => r.ParticipantId == participantId);
                
                if (result == null)
                {
                    Console.WriteLine($"Result not found for deletion: {participantId}");
                    return false;
                }
                
                _context.TestResults.Remove(result);
                var rowsAffected = await _context.SaveChangesAsync();
                
                Console.WriteLine($"Deleted result for {result.ParticipantName} (ID: {participantId})");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting result {participantId}: {ex.Message}");
                return false;
            }
        }

        private List<Question> GetDefaultQuestions()
        {
            var questions = new List<Question>();

            // Question 1
            var question1 = new Question { Id = 1, Text = "When your team faces a complex technical challenge, what's your primary approach?" };
            question1.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 1, Text = "Break it down into smaller problems and assign based on expertise", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 1 },
                new Alternative { Id = 2, Text = "Facilitate a team discussion to brainstorm solutions together", LeadershipType = LeadershipType.TeamLead, QuestionId = 1 },
                new Alternative { Id = 3, Text = "Design an architectural solution that addresses the root cause", LeadershipType = LeadershipType.Architect, QuestionId = 1 },
                new Alternative { Id = 4, Text = "Pair up developers to work through the challenge collaboratively", LeadershipType = LeadershipType.Mentor, QuestionId = 1 },
                new Alternative { Id = 5, Text = "Assess impact on timeline and resources, then adjust plans accordingly", LeadershipType = LeadershipType.ProjectManager, QuestionId = 1 }
            });

            // Question 2
            var question2 = new Question { Id = 2, Text = "A junior developer is struggling with a task. How do you respond?" };
            question2.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 6, Text = "Review their code and provide specific technical suggestions", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 2 },
                new Alternative { Id = 7, Text = "Check if the task was properly explained and offer support", LeadershipType = LeadershipType.TeamLead, QuestionId = 2 },
                new Alternative { Id = 8, Text = "Consider if the architecture or tools are creating unnecessary complexity", LeadershipType = LeadershipType.Architect, QuestionId = 2 },
                new Alternative { Id = 9, Text = "Schedule pairing sessions to help them learn through hands-on experience", LeadershipType = LeadershipType.Mentor, QuestionId = 2 },
                new Alternative { Id = 10, Text = "Evaluate if the task needs to be reassigned or deadline adjusted", LeadershipType = LeadershipType.ProjectManager, QuestionId = 2 }
            });

            // Question 3
            var question3 = new Question { Id = 3, Text = "Your team needs to choose a new technology stack. What's your role?" };
            question3.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 11, Text = "Research and present the technical pros and cons of each option", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 3 },
                new Alternative { Id = 12, Text = "Ensure everyone's opinion is heard and guide toward consensus", LeadershipType = LeadershipType.TeamLead, QuestionId = 3 },
                new Alternative { Id = 13, Text = "Evaluate how each option fits into the long-term system architecture", LeadershipType = LeadershipType.Architect, QuestionId = 3 },
                new Alternative { Id = 14, Text = "Help team members understand the learning curve and growth opportunities", LeadershipType = LeadershipType.Mentor, QuestionId = 3 },
                new Alternative { Id = 15, Text = "Analyze timeline, cost, and resource implications of each option", LeadershipType = LeadershipType.ProjectManager, QuestionId = 3 }
            });

            questions.Add(question1);
            questions.Add(question2);
            questions.Add(question3);

            return questions;
        }
    }
}