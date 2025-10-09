 Tech Leadership Assessment Tool

A comprehensive web application for assessing leadership styles in fullstack development teams. This tool helps identify dominant leadership tendencies across five key dimensions and provides personalized feedback for professional development.

 Features

 Core Assessment
- 5 Comprehensive Questions: Each question tests different leadership scenarios
- 5 Leadership Dimensions:
  - Technical Lead: Technical expertise, code quality, and technical decisions
  - Team Lead: Team coordination, communication, and collaboration
  - Architect: System design, architecture, and long-term planning
  - Mentor: Coaching, knowledge sharing, and team development
  - Project Manager: Planning, resources, timelines, and delivery

 Multi-Language Support
- English & Swedish: Full interface and content in both languages
- AI-Powered Translation: Dynamic question generation using Google Gemini AI
- Language Switching: Easy toggle between languages

 Results & Analytics
- Detailed Scoring: Individual scores for each leadership dimension (out of 5)
- Total Score: Overall assessment score (out of 25)
- Dominant Leadership Type: Identification of primary leadership style
- AI-Generated Feedback: Personalized development recommendations
- Progress Visualization: Visual progress bars for each dimension

 Data Management
- Database Storage: SQLite database for persistent data storage
- Participant Tracking: Unique 3-digit IDs for each assessment
- Results Management: View all results and delete functionality
- Export Ready: Structured data for further analysis

 Quick Start

 Prerequisites
- .NET 9.0 SDK
- Google Gemini AI API Key

 Installation

1. Clone the Repository
   ```bash
   git clone <repository-url>
   cd TechLeadershipWebApp
   ```

2. Configure API Key
   - Update the `apiKey` in `Services/GeminiAIService.cs`
   - Or set environment variable: `GEMINI_API_KEY=your_api_key_here`

3. Install Dependencies
   ```bash
   dotnet restore
   ```

4. Run the Application
   ```bash
   dotnet run
   ```

5. Access the Application
   - Open browser to: `https://localhost:5000` or `http://localhost:5000`

 Project Structure

```
TechLeadershipWebApp/
├── Controllers/
│   └── AssessmentController.cs
├── Models/
│   ├── Question.cs
│   ├── Alternative.cs
│   ├── TestResult.cs
│   └── AssessmentResponse.cs
├── Services/
│   ├── IAssessmentService.cs
│   ├── AssessmentService.cs
│   ├── IAIQuestionGenerator.cs
│   └── GeminiAIService.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Views/
│   ├── Assessment/
│   │   ├── Index.cshtml
│   │   ├── Result.cshtml
│   │   └── AllResults.cshtml
│   └── Shared/
│       └── _Layout.cshtml
├── wwwroot/
│   └── css/
│       └── site.css
└── Program.cs
```

 Assessment Methodology

 Question Design
- Each question presents realistic fullstack development scenarios
- 5 alternatives per question, each representing one leadership dimension
- Balanced distribution ensures each dimension is tested equally
- Randomized selection from AI-generated or default question pools

 Scoring System
- 1 point per question answered
- Maximum 5 points per leadership dimension
- Total score out of 25 points
- Dominant type determined by highest individual score

 Leadership Dimensions

 Technical Lead
- Focuses on technical excellence and code quality
- Strong in problem-solving and technical decision-making
- Excels in code reviews and technical guidance

 Team Lead
- Masters team coordination and communication
- Excellent at conflict resolution and team building
- Strong in facilitating collaboration

 Architect
- Strategic thinker with system design focus
- Long-term planning and architectural decisions
- Balances technical debt and innovation

 Mentor
- Develops team members through coaching
- Knowledge sharing and skill development
- Creates learning opportunities

 Project Manager
- Manages timelines, resources, and scope
- Stakeholder communication and project planning
- Risk assessment and delivery focus

 Configuration

 Database
- SQLite: Default database (techleadership.db)
- Automatic Migration: Database created on first run
- Entity Framework Core: ORM for data management

 AI Integration
- Google Gemini Pro: For question generation and feedback
- Fallback System: Default questions if AI unavailable
- Multi-language: AI prompts optimized for English and Swedish

 Customization
- Modify question count in `GeminiAIService.cs`
- Adjust scoring weights in `AssessmentService.cs`
- Add new languages by extending the language system

 Usage Guide

 Taking an Assessment
1. Select Language: Choose English or Swedish
2. Enter Name: Provide participant name
3. Answer Questions: Select one alternative per question
4. Submit Assessment: Review and submit responses
5. View Results: See scores and personalized feedback

 Admin Functions
- View All Results: Complete history of all assessments
- Delete Individual Results: Remove specific participant data
- Delete All Results: Clear entire database

 Interpreting Results
- High Scores (4-5): Strong natural tendency in this area
- Medium Scores (2-3): Developing competency, room for growth
- Low Scores (0-1): Potential area for significant development
- Balanced Profile: Similar scores across multiple dimensions
- Specialized Profile: One dominant dimension with supporting skills

 Technical Details

 Built With
- ASP.NET Core MVC: Web framework
- Entity Framework Core: Data access
- SQLite: Database engine
- Google Gemini AI: Natural language processing
- Bootstrap 5: Frontend framework
- Font Awesome: Icons

 OOP Principles Applied
- Encapsulation: Clear separation of concerns
- Inheritance: Proper class hierarchies
- Polymorphism: Interface implementations
- Abstraction: Service layer abstractions
- SOLID Principles: Maintainable and extensible design



  Use Cases

 Team Development
- Identify team leadership strengths and gaps
- Plan professional development programs
- Build balanced project teams

 Individual Growth
- Self-assessment for career development
- Identify areas for skill improvement
- Career path planning

 Organizational Planning
- Leadership pipeline development
- Training needs analysis
- Succession planning

  Data Privacy

- Local Storage: Data stored in local SQLite database
- No Personal Data: Only participant names stored
- Easy Deletion: Full control over data retention
- No External Sharing: All data remains within application

  Troubleshooting

 Common Issues

1. API Key Errors
   - Verify Gemini AI API key is valid
   - Check internet connectivity
   - Application will fall back to default questions

2. Database Issues
   - Delete `techleadership.db` to reset
   - Check file permissions in project directory

3. Language Switching
   - Clear browser cache if language doesn't change
   - Verify all language files are present

Logging
- Console logging for debugging
- Error tracking in application logs
- AI response parsing diagnostics

License

This project is for educational and organizational use. Please ensure compliance with Google Gemini AI terms of service when deploying.

Contributing

1. Fork the repository
2. Create feature branch
3. Commit changes
4. Push to branch
5. Create Pull Request

Support

For issues and questions:
1. Check troubleshooting section
2. Review application logs
3. Verify API configuration
4. Test with default questions
   
UML:
<img width="5141" height="5381" alt="UML" src="https://github.com/user-attachments/assets/5fce11db-6c6d-4a3b-b6bb-eb6dfca6546f" />
---

Version: 1.0  
Last Updated: 2025  
Compatibility: .NET 9.0+
