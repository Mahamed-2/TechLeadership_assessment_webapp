using Microsoft.AspNetCore.Mvc;
using TechLeadershipWebApp.Models;
using TechLeadershipWebApp.Services;

namespace TechLeadershipWebApp.Controllers
{
    public class AssessmentController : Controller
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AssessmentController> _logger;

        public AssessmentController(IAssessmentService assessmentService, ILogger<AssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string language = "en")
        {
            try
            {
                _logger.LogInformation($"Loading assessment questions in {language}...");
                var questions = await _assessmentService.GetQuestionsAsync(language);
                _logger.LogInformation($"Loaded {questions.Count} questions for assessment in {language}");
                
                ViewBag.CurrentLanguage = language;
                ViewBag.IsEnglish = language == "en";
                ViewBag.IsSwedish = language == "sv";
                
                return View(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading questions");
                ViewBag.Error = "Unable to load assessment questions. Please try again.";
                return View(new List<Question>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(AssessmentResponse response)
        {
            try
            {
                _logger.LogInformation($"Submit action called for {response.ParticipantName} with {response.Answers?.Count ?? 0} answers");

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Model state invalid. Errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                    
                    // Reload questions and return to form
                    var questions = await _assessmentService.GetQuestionsAsync();
                    return View("Index", questions);
                }

                if (response.Answers == null || response.Answers.Count == 0)
                {
                    ModelState.AddModelError("", "Please answer all questions before submitting.");
                    var questions = await _assessmentService.GetQuestionsAsync();
                    return View("Index", questions);
                }

                // Validate that all questions are answered
                var questionsCount = (await _assessmentService.GetQuestionsAsync()).Count;
                if (response.Answers.Count != questionsCount)
                {
                    ModelState.AddModelError("", "Please answer all questions before submitting.");
                    var questions = await _assessmentService.GetQuestionsAsync();
                    return View("Index", questions);
                }

                _logger.LogInformation($"Processing assessment for {response.ParticipantName} with {response.Answers.Count} answers");
                
                var result = await _assessmentService.SubmitAssessmentAsync(response);
                
                _logger.LogInformation($"Assessment completed for {result.ParticipantName} with ID {result.ParticipantId}");
                
                return RedirectToAction("Result", new { id = result.ParticipantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting assessment");
                ModelState.AddModelError("", "An error occurred while processing your assessment. Please try again.");
                var questions = await _assessmentService.GetQuestionsAsync();
                return View("Index", questions);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Result(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Result action called with empty ID");
                return NotFound();
            }

            try
            {
                _logger.LogInformation($"Retrieving result for ID: {id}");
                var result = await _assessmentService.GetResultByIdAsync(id);
                
                if (result == null)
                {
                    _logger.LogWarning($"Result not found for ID: {id}");
                    return NotFound();
                }

                _logger.LogInformation($"Displaying results for {result.ParticipantName} (ID: {id})");
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving result for ID: {id}");
                ViewBag.Error = "Unable to retrieve assessment results.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> AllResults()
        {
            try
            {
                var results = await _assessmentService.GetAllResultsAsync();
                _logger.LogInformation($"Retrieved {results.Count} total results");
                return View(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all results");
                ViewBag.Error = "Unable to retrieve assessment results.";
                return View(new List<TestResult>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllResults()
        {
            try
            {
                _logger.LogInformation("DeleteAllResults action called");
                
                var success = await _assessmentService.DeleteAllResultsAsync();
                
                if (success)
                {
                    _logger.LogInformation("All results deleted successfully");
                    TempData["Success"] = "All assessment results have been deleted successfully.";
                }
                else
                {
                    _logger.LogWarning("No results were deleted");
                    TempData["Error"] = "No results were found to delete.";
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all results");
                TempData["Error"] = "An error occurred while deleting results. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResult(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                _logger.LogInformation($"DeleteResult action called for ID: {id}");
                
                var success = await _assessmentService.DeleteResultAsync(id);
                
                if (success)
                {
                    _logger.LogInformation($"Result {id} deleted successfully");
                    TempData["Success"] = "Assessment result has been deleted successfully.";
                }
                else
                {
                    _logger.LogWarning($"Result {id} not found for deletion");
                    TempData["Error"] = "Result not found or could not be deleted.";
                }
                
                return RedirectToAction("AllResults");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting result {id}");
                TempData["Error"] = "An error occurred while deleting the result. Please try again.";
                return RedirectToAction("AllResults");
            }
        }
    }
}