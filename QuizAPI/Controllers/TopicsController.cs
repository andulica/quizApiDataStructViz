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

            if (await _context.Topics.AnyAsync(t => t.Name == topic.Name))
                return Conflict("A topic with the same name already exists.");

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTopic), new { id = topic.TopicId }, topic);
        }

        // Private: Accessed only by admin
        // PUT: api/topics/{id}
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTopic(int id, [FromBody] Topic updatedTopic)
        {
            if (id <= 0 || id != updatedTopic.TopicId)
                return BadRequest("Invalid or mismatched Topic ID.");

            var existingTopic = await _context.Topics
                .Include(t => t.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(t => t.TopicId == id);

            if (existingTopic == null)
                return NotFound($"Topic with ID {id} not found.");

            // Update topic details
            existingTopic.Name = updatedTopic.Name;

            // Update or add questions
            foreach (var updatedQuestion in updatedTopic.Questions)
            {
                var existingQuestion = existingTopic.Questions.FirstOrDefault(q => q.QuestionId == updatedQuestion.QuestionId);

                if (existingQuestion != null)
                {
                    existingQuestion.QuestionText = updatedQuestion.QuestionText;

                    // Update or add answers
                    foreach (var updatedAnswer in updatedQuestion.Answers)
                    {
                        var existingAnswer = existingQuestion.Answers.FirstOrDefault(a => a.AnswerId == updatedAnswer.AnswerId);

                        if (existingAnswer != null)
                        {
                            existingAnswer.AnswerText = updatedAnswer.AnswerText;
                            existingAnswer.IsCorrect = updatedAnswer.IsCorrect;
                        }
                        else
                        {
                            existingQuestion.Answers.Add(updatedAnswer);
                        }
                    }
                }
                else
                {
                    existingTopic.Questions.Add(updatedQuestion);
                }
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
