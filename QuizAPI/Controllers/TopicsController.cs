using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizAPI.Data;
using QuizAPI.Models;

namespace QuizAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TopicsController : ControllerBase
    {
        private readonly BlazorAppForDataStructuresContext _context;

        public TopicsController(BlazorAppForDataStructuresContext context)
        {
            _context = context;
        }

        // Public: Both clients and admin can access this endpoint
        // GET: api/topics
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopics()
        {
            var topics = await _context.Topics
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();

            if (!topics.Any())
                return NotFound("No topics available.");

            return Ok(topics);
        }

        // Public: Both clients and admin can access this endpoint
        // GET: api/topics/{id}
        [AllowAnonymous]
        [HttpGet("{topicName}/questions")]
        public async Task<IActionResult> GetTopic(string topicName)
        {
            var topic = await _context.Topics
            .Include(t => t.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(t => t.Name == topicName);

            if (topic == null)
            {
                return NotFound($"Topic with Name '{topicName}' not found.");
            }

            return Ok(topic.Questions);
        }

        // Private: Accessed only by admin
        // POST: api/topics
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddTopic([FromBody] Topic topic)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            foreach (var question in topic.Questions)
            {
                if (!question.Answers.Any(a => a.IsCorrect))
                {
                    return BadRequest($"Question '{question.QuestionText}' must have at least one correct answer.");
                }
            }

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            return Ok(topic);
        }

        // Private: Accessed only by admin
        // PUT: api/topics/{name}
        [Authorize(Roles = "Admin")]
        [HttpPut("{name}")]
        public async Task<IActionResult> UpdateTopic(string name, [FromBody] Topic updatedTopic)
        {
            if (string.IsNullOrWhiteSpace(name) || !string.Equals(name, updatedTopic.Name, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid or mismatched Topic Name.");

            var existingTopic = await _context.Topics
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(t => t.Name == name);

            if (existingTopic == null)
                return NotFound($"Topic with Name '{name}' not found.");

            existingTopic.Name = updatedTopic.Name;

            foreach (var updatedQuestion in updatedTopic.Questions)
            {
                if (updatedQuestion.QuestionId == 0)
                {
                    var newQuestion = new Question
                    {
                        QuestionText = updatedQuestion.QuestionText,
                        Answers = new List<Answer>()
                    };

                    foreach (var updatedAnswer in updatedQuestion.Answers)
                    {
                        var newAnswer = new Answer
                        {
                            AnswerText = updatedAnswer.AnswerText,
                            IsCorrect = updatedAnswer.IsCorrect
                        };
                        newQuestion.Answers.Add(newAnswer);
                    }

                    existingTopic.Questions.Add(newQuestion);
                }
                else
                {
                    var existingQuestion = existingTopic.Questions
                       .FirstOrDefault(q => q.QuestionId == updatedQuestion.QuestionId);

                    if (existingQuestion == null)
                    {
                        return BadRequest($"Question ID {updatedQuestion.QuestionId} not found in topic.");
                    }

                    existingQuestion.QuestionText = updatedQuestion.QuestionText;

                    foreach (var updatedAnswer in updatedQuestion.Answers)
                    {
                        if (updatedAnswer.AnswerId == 0)
                        {
                            // new answer
                            existingQuestion.Answers.Add(new Answer
                            {
                                AnswerText = updatedAnswer.AnswerText,
                                IsCorrect = updatedAnswer.IsCorrect
                            });
                        }
                        else
                        {
                            // existing answer
                            var existingAnswer = existingQuestion.Answers
                                .FirstOrDefault(a => a.AnswerId == updatedAnswer.AnswerId);

                            if (existingAnswer == null)
                            {
                                return BadRequest(
                                    $"Answer ID {updatedAnswer.AnswerId} not found under Question ID {updatedQuestion.QuestionId}.");
                            }

                            existingAnswer.AnswerText = updatedAnswer.AnswerText;
                            existingAnswer.IsCorrect = updatedAnswer.IsCorrect;
                        }
                    }

                    // Remove old answers if no longer present in updatedQuestion
                    var updatedAnswerIds = updatedQuestion.Answers.Select(a => a.AnswerId).ToHashSet();
                    var answersToRemove = existingQuestion.Answers
                        .Where(a => !updatedAnswerIds.Contains(a.AnswerId))
                        .ToList();
                    foreach (var answerToRemove in answersToRemove)
                    {
                        _context.Answers.Remove(answerToRemove);
                    }
                }
            }

            var updatedQuestionIds = updatedTopic.Questions.Select(q => q.QuestionId).ToHashSet();
            var questionsToRemove = existingTopic.Questions
                .Where(q => !updatedQuestionIds.Contains(q.QuestionId))
                .ToList();

            foreach (var questionToRemove in questionsToRemove)
            {
                _context.Questions.Remove(questionToRemove);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Private: Accessed only by admin
        // DELETE: api/topics/{id}
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Topic ID.");

            var topic = await _context.Topics
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(t => t.TopicId == id);

            if (topic == null)
                return NotFound($"Topic with ID {id} not found.");

            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
