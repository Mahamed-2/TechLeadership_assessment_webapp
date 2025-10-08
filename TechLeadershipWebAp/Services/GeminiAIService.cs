using System.Text;
using System.Text.Json;
using TechLeadershipWebApp.Models;

namespace TechLeadershipWebApp.Services
{
    public class GeminiAIService : IAIQuestionGenerator
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "AIzaSyDEEv6FcM-4bfZPeuJt4oJpaAwo54qr4P0";
        private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

        public GeminiAIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Question>> GenerateQuestionsAsync()
        {
            try
            {
                var prompt = @"Generate 3 unique leadership assessment questions for fullstack developers. 
                Each question should have exactly 5 alternatives, each representing one of these leadership types: 
                TechnicalLead, TeamLead, Architect, Mentor, ProjectManager.

                Return ONLY valid JSON in this exact format:
                {
                    ""questions"": [
                        {
                            ""text"": ""question text here"",
                            ""alternatives"": [
                                {""text"": ""alternative 1"", ""type"": ""TechnicalLead""},
                                {""text"": ""alternative 2"", ""type"": ""TeamLead""},
                                {""text"": ""alternative 3"", ""type"": ""Architect""},
                                {""text"": ""alternative 4"", ""type"": ""Mentor""},
                                {""text"": ""alternative 5"", ""type"": ""ProjectManager""}
                            ]
                        }
                    ]
                }";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 1000,
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}?key={_apiKey}", requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var generatedQuestions = ParseAIGeneratedQuestions(content);
                    
                    if (generatedQuestions.Any())
                        return generatedQuestions;
                }

                // Fallback to default questions if API fails
                return GetDefaultQuestions();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI API Error: {ex.Message}");
                return GetDefaultQuestions();
            }
        }

        public async Task<string> GenerateFeedbackAsync(TestResult result)
        {
            try
            {
                var prompt = $@"
                Provide constructive feedback for a fullstack developer with these leadership assessment scores (each out of 3, total out of 15):
                - Technical Lead: {result.TechnicalLeadScore}/3
                - Team Lead: {result.TeamLeadScore}/3  
                - Architect: {result.ArchitectScore}/3
                - Mentor: {result.MentorScore}/3
                - Project Manager: {result.ProjectManagerScore}/3
                - Total Score: {result.TotalScore}/15
                - Dominant Style: {result.DominantLeadershipType}

                Provide 2-3 specific, actionable skills to develop. Focus on practical fullstack development leadership skills.
                Keep it concise and professional.";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.5,
                        maxOutputTokens = 500,
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}?key={_apiKey}", requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var feedback = ExtractFeedbackFromResponse(content);
                    if (!string.IsNullOrEmpty(feedback))
                        return feedback;
                }

                return GetDefaultFeedback(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Feedback Error: {ex.Message}");
                return GetDefaultFeedback(result);
            }
        }

        private List<Question> ParseAIGeneratedQuestions(string apiResponse)
        {
            try
            {
                var questions = new List<Question>();
                using var document = JsonDocument.Parse(apiResponse);
                var candidates = document.RootElement.GetProperty("candidates");
                var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                if (string.IsNullOrEmpty(text))
                    return GetDefaultQuestions();

                // Extract JSON from the response (it might have markdown code blocks)
                var jsonStart = text.IndexOf('{');
                var jsonEnd = text.LastIndexOf('}') + 1;
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = text.Substring(jsonStart, jsonEnd - jsonStart);
                    var aiResponse = JsonSerializer.Deserialize<AIQuestionResponse>(jsonContent);

                    if (aiResponse?.Questions != null)
                    {
                        int questionId = 1;
                        foreach (var aiQuestion in aiResponse.Questions)
                        {
                            var question = new Question { Id = questionId++, Text = aiQuestion.Text };
                            
                            int altId = 1;
                            foreach (var alt in aiQuestion.Alternatives)
                            {
                                if (Enum.TryParse<LeadershipType>(alt.Type, out var leadershipType))
                                {
                                    question.Alternatives.Add(new Alternative 
                                    { 
                                        Id = (questionId - 1) * 5 + altId++,
                                        Text = alt.Text, 
                                        LeadershipType = leadershipType,
                                        QuestionId = question.Id
                                    });
                                }
                            }
                            questions.Add(question);
                        }
                        return questions;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing AI response: {ex.Message}");
            }

            return GetDefaultQuestions();
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

        private string ExtractFeedbackFromResponse(string content)
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                var candidates = document.RootElement.GetProperty("candidates");
                var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                return text?.Trim() ?? GetDefaultFeedback(null);
            }
            catch
            {
                return GetDefaultFeedback(null);
            }
        }

        private string GetDefaultFeedback(TestResult result)
        {
            var feedback = "Skills to develop:\n";
            
            if (result == null)
            {
                return feedback + "• Technical leadership\n• Team collaboration\n• Strategic planning";
            }

            // Provide specific feedback based on scores
            var lowScores = new List<string>();
            if (result.TechnicalLeadScore < 2) lowScores.Add("deep technical decision-making");
            if (result.TeamLeadScore < 2) lowScores.Add("team coordination and conflict resolution");
            if (result.ArchitectScore < 2) lowScores.Add("system architecture planning");
            if (result.MentorScore < 2) lowScores.Add("mentoring and knowledge sharing");
            if (result.ProjectManagerScore < 2) lowScores.Add("project planning and resource management");

            if (lowScores.Any())
            {
                feedback += $"• Focus on improving {string.Join(", ", lowScores.Take(3))}";
            }
            else
            {
                feedback += "• Continue developing all leadership aspects\n• Seek opportunities to lead cross-functional projects\n• Mentor junior team members";
            }

            return feedback;
        }
    }

    // Helper classes for AI response parsing
    public class AIQuestionResponse
    {
        public List<AIQuestion> Questions { get; set; } = new List<AIQuestion>();
    }

    public class AIQuestion
    {
        public string Text { get; set; } = string.Empty;
        public List<AIAlternative> Alternatives { get; set; } = new List<AIAlternative>();
    }

    public class AIAlternative
    {
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}