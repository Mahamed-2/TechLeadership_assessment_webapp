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

        public async Task<List<Question>> GenerateQuestionsAsync(string language = "en")
        {
            try
            {
                string prompt;
                
                if (language == "sv")
                {
                    prompt = @"Generera 5 unika och varierade ledarskapsbedömningsfrågor för fullstack-utvecklare.
                    Varje fråga ska testa olika aspekter av ledarskap och varje måste ha exakt 5 alternativ.
                    
                    VIKTIGT: Varje alternativ måste representera exakt en av dessa ledartyper:
                    - TechnicalLead: Fokuserar på teknisk expertis, kodkvalitet och tekniska beslut
                    - TeamLead: Fokuserar på teamkoordinering, kommunikation och samarbete
                    - Architect: Fokuserar på systemdesign, arkitektur och långsiktig planering
                    - Mentor: Fokuserar på coaching, kunskapsdelning och teamutveckling
                    - ProjectManager: Fokuserar på planering, resurser, tidslinjer och leverans

                    Se till att över alla 5 frågor, varje ledartyp förekommer exakt 5 gånger totalt (en gång per fråga).
                    
                    Returnera ENDAST giltig JSON i detta exakta format:
                    {
                        ""questions"": [
                            {
                                ""text"": ""unik fråga om ledarskapsscenario i tech"",
                                ""alternatives"": [
                                    {""text"": ""alternativ 1"", ""type"": ""TechnicalLead""},
                                    {""text"": ""alternativ 2"", ""type"": ""TeamLead""},
                                    {""text"": ""alternativ 3"", ""type"": ""Architect""},
                                    {""text"": ""alternativ 4"", ""type"": ""Mentor""},
                                    {""text"": ""alternativ 5"", ""type"": ""ProjectManager""}
                                ]
                            }
                        ]
                    }

                    Gör frågorna realistiska för fullstack-utvecklingssammanhang på svenska.";
                }
                else
                {
                    prompt = @"Generate 5 unique and diverse leadership assessment questions for fullstack developers. 
                    Each question should test different aspects of leadership and each must have exactly 5 alternatives.
                    
                    IMPORTANT: Each alternative must represent exactly one of these leadership types:
                    - TechnicalLead: Focuses on technical expertise, code quality, and technical decisions
                    - TeamLead: Focuses on team coordination, communication, and collaboration
                    - Architect: Focuses on system design, architecture, and long-term planning
                    - Mentor: Focuses on coaching, knowledge sharing, and team development
                    - ProjectManager: Focuses on planning, resources, timelines, and delivery

                    Ensure that across all 5 questions, each leadership type appears exactly 5 times total (once per question).
                    
                    Return ONLY valid JSON in this exact format:
                    {
                        ""questions"": [
                            {
                                ""text"": ""unique question about leadership scenario in tech"",
                                ""alternatives"": [
                                    {""text"": ""alternative 1"", ""type"": ""TechnicalLead""},
                                    {""text"": ""alternative 2"", ""type"": ""TeamLead""},
                                    {""text"": ""alternative 3"", ""type"": ""Architect""},
                                    {""text"": ""alternative 4"", ""type"": ""Mentor""},
                                    {""text"": ""alternative 5"", ""type"": ""ProjectManager""}
                                ]
                            }
                        ]
                    }

                    Make the questions realistic for fullstack development contexts.";
                }

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
                        temperature = 0.9,
                        maxOutputTokens = 2500,
                        topP = 0.8,
                        topK = 40
                    }
                };

                Console.WriteLine($"Sending request to Gemini API for 5 questions in {language}...");

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}?key={_apiKey}", requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var generatedQuestions = ParseAIGeneratedQuestions(content, language);
                    
                    if (generatedQuestions.Count == 5)
                    {
                        Console.WriteLine($"Successfully generated {generatedQuestions.Count} questions from AI in {language}");
                        return generatedQuestions;
                    }
                    else
                    {
                        Console.WriteLine($"AI generated {generatedQuestions.Count} questions, expected 5. Using randomized defaults in {language}.");
                        return GetRandomDefaultQuestions(5, language);
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                    return GetRandomDefaultQuestions(5, language);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI API Exception: {ex.Message}");
                return GetRandomDefaultQuestions(5, language);
            }
        }

        private List<Question> ParseAIGeneratedQuestions(string apiResponse, string language)
        {
            try
            {
                Console.WriteLine($"Parsing AI response for {language}...");

                using var document = JsonDocument.Parse(apiResponse);
                var candidates = document.RootElement.GetProperty("candidates");
                var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

                if (string.IsNullOrEmpty(text))
                {
                    Console.WriteLine("Empty response text from AI");
                    return new List<Question>();
                }

                // Extract JSON from the response (it might have markdown code blocks)
                var jsonStart = text.IndexOf('{');
                var jsonEnd = text.LastIndexOf('}') + 1;
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonContent = text.Substring(jsonStart, jsonEnd - jsonStart);
                    Console.WriteLine($"Extracted JSON: {jsonContent}");

                    var aiResponse = JsonSerializer.Deserialize<AIQuestionResponse>(jsonContent);

                    if (aiResponse?.Questions != null)
                    {
                        var questions = new List<Question>();
                        int questionId = 1;
                        
                        foreach (var aiQuestion in aiResponse.Questions.Take(5))
                        {
                            var question = new Question { Id = questionId, Text = aiQuestion.Text.Trim(), Language = language };
                            
                            int altId = 1;
                            foreach (var alt in aiQuestion.Alternatives.Take(5))
                            {
                                if (Enum.TryParse<LeadershipType>(alt.Type, out var leadershipType))
                                {
                                    question.Alternatives.Add(new Alternative 
                                    { 
                                        Id = (questionId - 1) * 5 + altId,
                                        Text = alt.Text.Trim(), 
                                        LeadershipType = leadershipType,
                                        QuestionId = questionId
                                    });
                                    altId++;
                                }
                            }
                            
                            if (question.Alternatives.Count == 5)
                            {
                                questions.Add(question);
                                questionId++;
                            }
                        }

                        Console.WriteLine($"Successfully parsed {questions.Count} questions from AI response in {language}");
                        return questions;
                    }
                }

                Console.WriteLine("Failed to parse valid questions from AI response");
                return new List<Question>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing AI response: {ex.Message}");
                return new List<Question>();
            }
        }

        private List<Question> GetRandomDefaultQuestions(int numberOfQuestions, string language)
        {
            Console.WriteLine($"Using randomized default questions ({numberOfQuestions} questions) in {language}");
            
            if (language == "sv")
            {
                return GetSwedishDefaultQuestions(numberOfQuestions);
            }
            else
            {
                return GetEnglishDefaultQuestions(numberOfQuestions);
            }
        }

        private List<Question> GetEnglishDefaultQuestions(int numberOfQuestions)
        {
            var allQuestions = new List<Question>
            {
                new Question { Id = 1, Text = "When your team faces a complex technical challenge, what's your primary approach?", Language = "en" },
                new Question { Id = 2, Text = "A junior developer is struggling with a task. How do you respond?", Language = "en" },
                new Question { Id = 3, Text = "Your team needs to choose a new technology stack. What's your role?", Language = "en" },
                new Question { Id = 4, Text = "How do you handle conflicts between team members with different technical opinions?", Language = "en" },
                new Question { Id = 5, Text = "When planning a new feature, what's your main consideration?", Language = "en" },
                new Question { Id = 6, Text = "How do you ensure knowledge sharing within your development team?", Language = "en" },
                new Question { Id = 7, Text = "What's your approach when a project is behind schedule?", Language = "en" },
                new Question { Id = 8, Text = "How do you mentor junior developers on your team?", Language = "en" },
                new Question { Id = 9, Text = "What's your role in code review processes?", Language = "en" },
                new Question { Id = 10, Text = "How do you prioritize technical debt versus new features?", Language = "en" }
            };

            foreach (var question in allQuestions)
            {
                AddAlternativesToQuestion(question, "en");
            }

            return SelectRandomQuestions(allQuestions, numberOfQuestions, "en");
        }

        private List<Question> GetSwedishDefaultQuestions(int numberOfQuestions)
        {
            var allQuestions = new List<Question>
            {
                new Question { Id = 1, Text = "När ditt team står inför en komplex teknisk utmaning, vad är ditt primära tillvägagångssätt?", Language = "sv" },
                new Question { Id = 2, Text = "En junior utvecklare kämpar med en uppgift. Hur agerar du?", Language = "sv" },
                new Question { Id = 3, Text = "Ditt team behöver välja en ny teknikstack. Vad är din roll?", Language = "sv" },
                new Question { Id = 4, Text = "Hur hanterar du konflikter mellan teammedlemmar med olika tekniska åsikter?", Language = "sv" },
                new Question { Id = 5, Text = "Vad är ditt huvudsakliga övervägande när du planerar en ny funktion?", Language = "sv" },
                new Question { Id = 6, Text = "Hur säkerställer du kunskapsdelning inom ditt utvecklingsteam?", Language = "sv" },
                new Question { Id = 7, Text = "Vad är ditt tillvägagångssätt när ett projekt är efter schema?", Language = "sv" },
                new Question { Id = 8, Text = "Hur mentorar du juniora utvecklare i ditt team?", Language = "sv" },
                new Question { Id = 9, Text = "Vad är din roll i kodgranskningsprocesser?", Language = "sv" },
                new Question { Id = 10, Text = "Hur prioriterar du teknisk skuld kontra nya funktioner?", Language = "sv" }
            };

            foreach (var question in allQuestions)
            {
                AddAlternativesToQuestion(question, "sv");
            }

            return SelectRandomQuestions(allQuestions, numberOfQuestions, "sv");
        }

        private List<Question> SelectRandomQuestions(List<Question> allQuestions, int numberOfQuestions, string language)
        {
            var random = new Random();
            var selectedQuestions = allQuestions.OrderBy(x => random.Next()).Take(numberOfQuestions).ToList();

            for (int i = 0; i < selectedQuestions.Count; i++)
            {
                selectedQuestions[i].Id = i + 1;
                foreach (var alt in selectedQuestions[i].Alternatives)
                {
                    alt.QuestionId = i + 1;
                    alt.Id = (i * 5) + (selectedQuestions[i].Alternatives.IndexOf(alt) + 1);
                }
            }

            Console.WriteLine($"Selected {selectedQuestions.Count} random questions in {language}");
            return selectedQuestions;
        }

        private void AddAlternativesToQuestion(Question question, string language)
        {
            if (language == "sv")
            {
                AddSwedishAlternativesToQuestion(question);
            }
            else
            {
                AddEnglishAlternativesToQuestion(question);
            }
        }

        private void AddEnglishAlternativesToQuestion(Question question)
        {
            var alternatives = new List<Alternative>
            {
                new Alternative { Text = GetTechnicalLeadAlternative(question.Text, "en"), LeadershipType = LeadershipType.TechnicalLead },
                new Alternative { Text = GetTeamLeadAlternative(question.Text, "en"), LeadershipType = LeadershipType.TeamLead },
                new Alternative { Text = GetArchitectAlternative(question.Text, "en"), LeadershipType = LeadershipType.Architect },
                new Alternative { Text = GetMentorAlternative(question.Text, "en"), LeadershipType = LeadershipType.Mentor },
                new Alternative { Text = GetProjectManagerAlternative(question.Text, "en"), LeadershipType = LeadershipType.ProjectManager }
            };

            for (int i = 0; i < alternatives.Count; i++)
            {
                alternatives[i].Id = (question.Id - 1) * 5 + i + 1;
                alternatives[i].QuestionId = question.Id;
            }

            question.Alternatives.AddRange(alternatives);
        }

        private void AddSwedishAlternativesToQuestion(Question question)
        {
            var alternatives = new List<Alternative>
            {
                new Alternative { Text = GetTechnicalLeadAlternative(question.Text, "sv"), LeadershipType = LeadershipType.TechnicalLead },
                new Alternative { Text = GetTeamLeadAlternative(question.Text, "sv"), LeadershipType = LeadershipType.TeamLead },
                new Alternative { Text = GetArchitectAlternative(question.Text, "sv"), LeadershipType = LeadershipType.Architect },
                new Alternative { Text = GetMentorAlternative(question.Text, "sv"), LeadershipType = LeadershipType.Mentor },
                new Alternative { Text = GetProjectManagerAlternative(question.Text, "sv"), LeadershipType = LeadershipType.ProjectManager }
            };

            for (int i = 0; i < alternatives.Count; i++)
            {
                alternatives[i].Id = (question.Id - 1) * 5 + i + 1;
                alternatives[i].QuestionId = question.Id;
            }

            question.Alternatives.AddRange(alternatives);
        }

        private string GetTechnicalLeadAlternative(string question, string language)
        {
            if (language == "sv")
            {
                if (question.Contains("teknisk utmaning")) return "Bryt ner den i mindre problem och fördela baserat på expertis";
                if (question.Contains("kämpar")) return "Granska deras kod och ge specifika tekniska förslag";
                if (question.Contains("teknikstack")) return "Forska och presentera de tekniska för- och nackdelarna med varje alternativ";
                return "Tillämpa teknisk expertis för att lösa problemet effektivt";
            }
            else
            {
                if (question.Contains("technical challenge")) return "Break it down into smaller problems and assign based on expertise";
                if (question.Contains("struggling")) return "Review their code and provide specific technical suggestions";
                if (question.Contains("technology stack")) return "Research and present the technical pros and cons of each option";
                return "Apply technical expertise to solve the problem efficiently";
            }
        }

        private string GetTeamLeadAlternative(string question, string language)
        {
            if (language == "sv")
            {
                if (question.Contains("teknisk utmaning")) return "Facilitera en teamdiskussion för att brainstorma lösningar tillsammans";
                if (question.Contains("kämpar")) return "Kontrollera om uppgiften förklarades ordentligt och erbjud stöd";
                if (question.Contains("teknikstack")) return "Se till att allas åsikter hörs och vägled mot konsensus";
                return "Samordna teamets ansträngningar och upprätthåll positiva teamdynamiker";
            }
            else
            {
                if (question.Contains("technical challenge")) return "Facilitate a team discussion to brainstorm solutions together";
                if (question.Contains("struggling")) return "Check if the task was properly explained and offer support";
                if (question.Contains("technology stack")) return "Ensure everyone's opinion is heard and guide toward consensus";
                return "Coordinate team efforts and maintain positive team dynamics";
            }
        }

        private string GetArchitectAlternative(string question, string language)
        {
            if (language == "sv")
            {
                if (question.Contains("teknisk utmaning")) return "Designa en arkitektur-lösning som adresserar roten till problemet";
                if (question.Contains("kämpar")) return "Överväg om arkitekturen eller verktygen skapar onödig komplexitet";
                if (question.Contains("teknikstack")) return "Utvärdera hur varje alternativ passar in i den långsiktiga systemarkitekturen";
                return "Fokusera på skalbara och underhållbara systemdesign";
            }
            else
            {
                if (question.Contains("technical challenge")) return "Design an architectural solution that addresses the root cause";
                if (question.Contains("struggling")) return "Consider if the architecture or tools are creating unnecessary complexity";
                if (question.Contains("technology stack")) return "Evaluate how each option fits into the long-term system architecture";
                return "Focus on scalable and maintainable system design";
            }
        }

        private string GetMentorAlternative(string question, string language)
        {
            if (language == "sv")
            {
                if (question.Contains("teknisk utmaning")) return "Para ihop utvecklare för att arbeta genom utmaningen tillsammans";
                if (question.Contains("kämpar")) return "Schemalägg parprogrammeringssessioner för praktisk inlärning";
                if (question.Contains("teknikstack")) return "Hjälp teammedlemmar förstå inlärningskurvan och utvecklingsmöjligheter";
                return "Fokusera på teamutveckling och kunskapsoverföring";
            }
            else
            {
                if (question.Contains("technical challenge")) return "Pair up developers to work through the challenge collaboratively";
                if (question.Contains("struggling")) return "Schedule pairing sessions to help them learn through hands-on experience";
                if (question.Contains("technology stack")) return "Help team members understand the learning curve and growth opportunities";
                return "Focus on team development and knowledge transfer";
            }
        }

        private string GetProjectManagerAlternative(string question, string language)
        {
            if (language == "sv")
            {
                if (question.Contains("teknisk utmaning")) return "Bedöm påverkan på tidsplan och resurser, justera sedan planerna därefter";
                if (question.Contains("kämpar")) return "Utvärdera om uppgiften behöver omfördelas eller deadline justeras";
                if (question.Contains("teknikstack")) return "Analysera tidsplan, kostnad och resurskonsekvenser för varje alternativ";
                return "Hantera projektomfattning, tidsplan och resurstilldelning";
            }
            else
            {
                if (question.Contains("technical challenge")) return "Assess impact on timeline and resources, then adjust plans accordingly";
                if (question.Contains("struggling")) return "Evaluate if the task needs to be reassigned or deadline adjusted";
                if (question.Contains("technology stack")) return "Analyze timeline, cost, and resource implications of each option";
                return "Manage project scope, timeline, and resource allocation";
            }
        }

        public async Task<string> GenerateFeedbackAsync(TestResult result, string language = "en")
        {
            try
            {
                string prompt;
                
                if (language == "sv")
                {
                    prompt = $@"
                    Ge konstruktiv feedback för en fullstack-utvecklare med dessa ledarskapsbedömningspoäng (av 5):
                    - Teknisk Lead: {result.TechnicalLeadScore}/5
                    - Team Lead: {result.TeamLeadScore}/5  
                    - Arkitekt: {result.ArchitectScore}/5
                    - Mentor: {result.MentorScore}/5
                    - Projektledare: {result.ProjectManagerScore}/5
                    - Totalt resultat: {result.TotalScore}/25
                    - Dominant stil: {GetSwedishLeadershipType(result.DominantLeadershipType)}

                    Ge 2-3 specifika, åtgärdbara färdigheter att utveckla för fullstack-utvecklingsledarskap.
                    Fokusera på praktiska förbättringar och håll det koncist på svenska.";
                }
                else
                {
                    prompt = $@"
                    Provide constructive feedback for a fullstack developer with these leadership assessment scores (out of 5):
                    - Technical Lead: {result.TechnicalLeadScore}/5
                    - Team Lead: {result.TeamLeadScore}/5  
                    - Architect: {result.ArchitectScore}/5
                    - Mentor: {result.MentorScore}/5
                    - Project Manager: {result.ProjectManagerScore}/5
                    - Total Score: {result.TotalScore}/25
                    - Dominant Style: {result.DominantLeadershipType}

                    Provide 2-3 specific, actionable skills to develop for fullstack development leadership.
                    Focus on practical improvements and keep it concise.";
                }

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
                        maxOutputTokens = 500,
                    }
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}?key={_apiKey}", requestBody);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var feedback = ExtractFeedbackFromResponse(content);
                    if (!string.IsNullOrEmpty(feedback))
                    {
                        Console.WriteLine($"AI feedback generated successfully in {language}");
                        return feedback;
                    }
                }

                return GetDefaultFeedback(result, language);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Feedback Error: {ex.Message}");
                return GetDefaultFeedback(result, language);
            }
        }

        // ADD THE MISSING METHOD HERE
        private string ExtractFeedbackFromResponse(string content)
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                var candidates = document.RootElement.GetProperty("candidates");
                var text = candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                return text?.Trim() ?? GetDefaultFeedback(null, "en");
            }
            catch
            {
                return GetDefaultFeedback(null, "en");
            }
        }

        private string GetSwedishLeadershipType(LeadershipType type)
        {
            return type switch
            {
                LeadershipType.TechnicalLead => "Teknisk Lead",
                LeadershipType.TeamLead => "Team Lead",
                LeadershipType.Architect => "Arkitekt",
                LeadershipType.Mentor => "Mentor",
                LeadershipType.ProjectManager => "Projektledare",
                _ => "Okänd"
            };
        }

        private string GetDefaultFeedback(TestResult result, string language)
        {
            if (language == "sv")
            {
                var feedback = "Färdigheter att utveckla:\n";
                
                if (result == null)
                {
                    return feedback + "• Tekniskt ledarskap\n• Teamsamarbete\n• Strategisk planering\n• Mentoringsförmåga\n• Projektkoordinering";
                }

                var lowScores = new List<string>();
                if (result.TechnicalLeadScore < 3) lowScores.Add("tekniskt beslutsfattande");
                if (result.TeamLeadScore < 3) lowScores.Add("teamkoordinering och konflikthantering");
                if (result.ArchitectScore < 3) lowScores.Add("systemarkitekturplanering");
                if (result.MentorScore < 3) lowScores.Add("mentorings- och kunskapsdelningsförmåga");
                if (result.ProjectManagerScore < 3) lowScores.Add("projektplanering och resurshantering");

                if (lowScores.Any())
                {
                    feedback += $"• Fokusera på att förbättra {string.Join(", ", lowScores.Take(3))}";
                }
                else
                {
                    feedback += "• Fortsätt utveckla alla ledarskapsaspekter\n• Sök möjligheter att leda tvärfunktionella projekt\n• Mentorera juniora teammedlemmar";
                }

                return feedback;
            }
            else
            {
                var feedback = "Skills to develop:\n";
                
                if (result == null)
                {
                    return feedback + "• Technical leadership\n• Team collaboration\n• Strategic planning\n• Mentoring abilities\n• Project coordination";
                }

                var lowScores = new List<string>();
                if (result.TechnicalLeadScore < 3) lowScores.Add("deep technical decision-making");
                if (result.TeamLeadScore < 3) lowScores.Add("team coordination and conflict resolution");
                if (result.ArchitectScore < 3) lowScores.Add("system architecture planning");
                if (result.MentorScore < 3) lowScores.Add("mentoring and knowledge sharing");
                if (result.ProjectManagerScore < 3) lowScores.Add("project planning and resource management");

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