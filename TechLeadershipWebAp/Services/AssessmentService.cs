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

        public async Task<List<Question>> GetQuestionsAsync(string language = "en")
        {
            try
            {
                // Always generate new questions from AI for variety
                _currentQuestions = await _aiGenerator.GenerateQuestionsAsync(language);
                Console.WriteLine($"Generated {_currentQuestions.Count} questions in {language}");
                return _currentQuestions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Question Generation Error: {ex.Message}");
                
                // If AI fails, use default questions but ensure we have questions
                if (!_currentQuestions.Any())
                {
                    _currentQuestions = GetDefaultQuestions(language);
                }
                Console.WriteLine($"Using {_currentQuestions.Count} default questions in {language}");
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
                }
            }

            Console.WriteLine($"Final scores - Technical: {result.TechnicalLeadScore}, Team: {result.TeamLeadScore}, Architect: {result.ArchitectScore}, Mentor: {result.MentorScore}, PM: {result.ProjectManagerScore}");

            // Verify that each dimension is tested (should be 1 point per question, total 5 per dimension)
            if (result.TechnicalLeadScore + result.TeamLeadScore + result.ArchitectScore + result.MentorScore + result.ProjectManagerScore != 5)
            {
                Console.WriteLine($"WARNING: Total score {result.TotalScore} doesn't equal 5. Questions may not be properly balanced.");
            }

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

        private List<Question> GetDefaultQuestions(string language = "en")
        {
            if (language == "sv")
            {
                return GetSwedishDefaultQuestions();
            }
            else
            {
                return GetEnglishDefaultQuestions();
            }
        }

        private List<Question> GetEnglishDefaultQuestions()
        {
            var questions = new List<Question>();

            // Create 5 default questions in English
            var question1 = new Question { Id = 1, Text = "When your team faces a complex technical challenge, what's your primary approach?", Language = "en" };
            question1.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 1, Text = "Break it down into smaller problems and assign based on expertise", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 1 },
                new Alternative { Id = 2, Text = "Facilitate a team discussion to brainstorm solutions together", LeadershipType = LeadershipType.TeamLead, QuestionId = 1 },
                new Alternative { Id = 3, Text = "Design an architectural solution that addresses the root cause", LeadershipType = LeadershipType.Architect, QuestionId = 1 },
                new Alternative { Id = 4, Text = "Pair up developers to work through the challenge collaboratively", LeadershipType = LeadershipType.Mentor, QuestionId = 1 },
                new Alternative { Id = 5, Text = "Assess impact on timeline and resources, then adjust plans accordingly", LeadershipType = LeadershipType.ProjectManager, QuestionId = 1 }
            });

            var question2 = new Question { Id = 2, Text = "A junior developer is struggling with a task. How do you respond?", Language = "en" };
            question2.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 6, Text = "Review their code and provide specific technical suggestions", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 2 },
                new Alternative { Id = 7, Text = "Check if the task was properly explained and offer support", LeadershipType = LeadershipType.TeamLead, QuestionId = 2 },
                new Alternative { Id = 8, Text = "Consider if the architecture or tools are creating unnecessary complexity", LeadershipType = LeadershipType.Architect, QuestionId = 2 },
                new Alternative { Id = 9, Text = "Schedule pairing sessions to help them learn through hands-on experience", LeadershipType = LeadershipType.Mentor, QuestionId = 2 },
                new Alternative { Id = 10, Text = "Evaluate if the task needs to be reassigned or deadline adjusted", LeadershipType = LeadershipType.ProjectManager, QuestionId = 2 }
            });

            var question3 = new Question { Id = 3, Text = "Your team needs to choose a new technology stack. What's your role?", Language = "en" };
            question3.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 11, Text = "Research and present the technical pros and cons of each option", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 3 },
                new Alternative { Id = 12, Text = "Ensure everyone's opinion is heard and guide toward consensus", LeadershipType = LeadershipType.TeamLead, QuestionId = 3 },
                new Alternative { Id = 13, Text = "Evaluate how each option fits into the long-term system architecture", LeadershipType = LeadershipType.Architect, QuestionId = 3 },
                new Alternative { Id = 14, Text = "Help team members understand the learning curve and growth opportunities", LeadershipType = LeadershipType.Mentor, QuestionId = 3 },
                new Alternative { Id = 15, Text = "Analyze timeline, cost, and resource implications of each option", LeadershipType = LeadershipType.ProjectManager, QuestionId = 3 }
            });

            var question4 = new Question { Id = 4, Text = "How do you handle conflicts between team members with different technical opinions?", Language = "en" };
            question4.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 16, Text = "Analyze the technical merits of each approach objectively", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 4 },
                new Alternative { Id = 17, Text = "Facilitate a meeting to find common ground and build consensus", LeadershipType = LeadershipType.TeamLead, QuestionId = 4 },
                new Alternative { Id = 18, Text = "Propose an architectural pattern that incorporates the best of both ideas", LeadershipType = LeadershipType.Architect, QuestionId = 4 },
                new Alternative { Id = 19, Text = "Coach both developers on constructive technical discussions", LeadershipType = LeadershipType.Mentor, QuestionId = 4 },
                new Alternative { Id = 20, Text = "Assess the impact on project timeline and make a decisive call", LeadershipType = LeadershipType.ProjectManager, QuestionId = 4 }
            });

            var question5 = new Question { Id = 5, Text = "What's your approach when a project is behind schedule?", Language = "en" };
            question5.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 21, Text = "Identify technical bottlenecks and optimize critical paths", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 5 },
                new Alternative { Id = 22, Text = "Boost team morale and coordinate extra collaboration sessions", LeadershipType = LeadershipType.TeamLead, QuestionId = 5 },
                new Alternative { Id = 23, Text = "Reevaluate the architecture for potential simplifications", LeadershipType = LeadershipType.Architect, QuestionId = 5 },
                new Alternative { Id = 24, Text = "Provide additional support and guidance to struggling team members", LeadershipType = LeadershipType.Mentor, QuestionId = 5 },
                new Alternative { Id = 25, Text = "Negotiate scope adjustments and communicate with stakeholders", LeadershipType = LeadershipType.ProjectManager, QuestionId = 5 }
            });

            questions.Add(question1);
            questions.Add(question2);
            questions.Add(question3);
            questions.Add(question4);
            questions.Add(question5);

            return questions;
        }

        private List<Question> GetSwedishDefaultQuestions()
        {
            var questions = new List<Question>();

            // Create 5 default questions in Swedish
            var question1 = new Question { Id = 1, Text = "När ditt team står inför en komplex teknisk utmaning, vad är ditt primära tillvägagångssätt?", Language = "sv" };
            question1.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 1, Text = "Bryt ner den i mindre problem och fördela baserat på expertis", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 1 },
                new Alternative { Id = 2, Text = "Facilitera en teamdiskussion för att brainstorma lösningar tillsammans", LeadershipType = LeadershipType.TeamLead, QuestionId = 1 },
                new Alternative { Id = 3, Text = "Designa en arkitektur-lösning som adresserar roten till problemet", LeadershipType = LeadershipType.Architect, QuestionId = 1 },
                new Alternative { Id = 4, Text = "Para ihop utvecklare för att arbeta genom utmaningen tillsammans", LeadershipType = LeadershipType.Mentor, QuestionId = 1 },
                new Alternative { Id = 5, Text = "Bedöm påverkan på tidsplan och resurser, justera sedan planerna därefter", LeadershipType = LeadershipType.ProjectManager, QuestionId = 1 }
            });

            var question2 = new Question { Id = 2, Text = "En junior utvecklare kämpar med en uppgift. Hur agerar du?", Language = "sv" };
            question2.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 6, Text = "Granska deras kod och ge specifika tekniska förslag", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 2 },
                new Alternative { Id = 7, Text = "Kontrollera om uppgiften förklarades ordentligt och erbjud stöd", LeadershipType = LeadershipType.TeamLead, QuestionId = 2 },
                new Alternative { Id = 8, Text = "Överväg om arkitekturen eller verktygen skapar onödig komplexitet", LeadershipType = LeadershipType.Architect, QuestionId = 2 },
                new Alternative { Id = 9, Text = "Schemalägg parprogrammeringssessioner för praktisk inlärning", LeadershipType = LeadershipType.Mentor, QuestionId = 2 },
                new Alternative { Id = 10, Text = "Utvärdera om uppgiften behöver omfördelas eller deadline justeras", LeadershipType = LeadershipType.ProjectManager, QuestionId = 2 }
            });

            var question3 = new Question { Id = 3, Text = "Ditt team behöver välja en ny teknikstack. Vad är din roll?", Language = "sv" };
            question3.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 11, Text = "Forska och presentera de tekniska för- och nackdelarna med varje alternativ", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 3 },
                new Alternative { Id = 12, Text = "Se till att allas åsikter hörs och vägled mot konsensus", LeadershipType = LeadershipType.TeamLead, QuestionId = 3 },
                new Alternative { Id = 13, Text = "Utvärdera hur varje alternativ passar in i den långsiktiga systemarkitekturen", LeadershipType = LeadershipType.Architect, QuestionId = 3 },
                new Alternative { Id = 14, Text = "Hjälp teammedlemmar förstå inlärningskurvan och utvecklingsmöjligheter", LeadershipType = LeadershipType.Mentor, QuestionId = 3 },
                new Alternative { Id = 15, Text = "Analysera tidsplan, kostnad och resurskonsekvenser för varje alternativ", LeadershipType = LeadershipType.ProjectManager, QuestionId = 3 }
            });

            var question4 = new Question { Id = 4, Text = "Hur hanterar du konflikter mellan teammedlemmar med olika tekniska åsikter?", Language = "sv" };
            question4.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 16, Text = "Analysera de tekniska meriterna för varje tillvägagångssätt objektivt", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 4 },
                new Alternative { Id = 17, Text = "Facilitera ett möte för att hitta gemensam grund och bygga konsensus", LeadershipType = LeadershipType.TeamLead, QuestionId = 4 },
                new Alternative { Id = 18, Text = "Föreslå ett arkitekturmönster som inkorporerar det bästa från båda idéer", LeadershipType = LeadershipType.Architect, QuestionId = 4 },
                new Alternative { Id = 19, Text = "Coacha båda utvecklare i konstruktiva tekniska diskussioner", LeadershipType = LeadershipType.Mentor, QuestionId = 4 },
                new Alternative { Id = 20, Text = "Bedöm påverkan på projekttidsplan och fatta ett beslut", LeadershipType = LeadershipType.ProjectManager, QuestionId = 4 }
            });

            var question5 = new Question { Id = 5, Text = "Vad är ditt tillvägagångssätt när ett projekt är efter schema?", Language = "sv" };
            question5.Alternatives.AddRange(new[]
            {
                new Alternative { Id = 21, Text = "Identifiera tekniska flaskhalsar och optimera kritiska vägar", LeadershipType = LeadershipType.TechnicalLead, QuestionId = 5 },
                new Alternative { Id = 22, Text = "Öka teammoralen och samordna extra samarbetssessioner", LeadershipType = LeadershipType.TeamLead, QuestionId = 5 },
                new Alternative { Id = 23, Text = "Omvärdera arkitekturen för potentiella förenklingar", LeadershipType = LeadershipType.Architect, QuestionId = 5 },
                new Alternative { Id = 24, Text = "Ge extra stöd och vägledning till teammedlemmar som kämpar", LeadershipType = LeadershipType.Mentor, QuestionId = 5 },
                new Alternative { Id = 25, Text = "Förhandla omfångsjusteringar och kommunicera med intressenter", LeadershipType = LeadershipType.ProjectManager, QuestionId = 5 }
            });

            questions.Add(question1);
            questions.Add(question2);
            questions.Add(question3);
            questions.Add(question4);
            questions.Add(question5);

            return questions;
        }
    }
}