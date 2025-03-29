using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.DTO.Discussions;
using RockServers.Extensions;
using RockServers.Helpers;
using RockServers.Mappers;
using RockServers.Models;

namespace RockServers.Controllers
{
    [ApiController]
    [Route("api/discussions")]
    public class DiscussionController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DiscussionController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DiscussionQueryObject queryObject)
        {
            var discussions = _context.Discussions.AsQueryable();
            if (queryObject != null)
            {
                if (queryObject.GameId != null)
                    discussions = discussions.Where(g => g.GameId == queryObject.GameId);
            }
            ;
            var discussionDtos = await discussions.Include(d => d.Game)
                                           .Include(d => d.AppUser)
                                           .Select(d => d.ToDiscussionDto())
                                           .ToListAsync();
            return Ok(discussionDtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var discussion = await _context.Discussions.Where(d => d.Id == id)
                                                       .Include(d => d.Game)
                                                       .Include(d => d.AppUser)
                                                       .FirstOrDefaultAsync();
            if (discussion == null)
                return NotFound($"Discussion ID with {id} does not exist.");
            return Ok(discussion.ToDiscussionDto());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateDiscussionDto createDiscussionDto)
        {
            var gameId = createDiscussionDto.GameId;
            var game = await _context.Games.Where(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null)
                return NotFound($"Game with ID {gameId} not found");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return NotFound("Invalid User ID Provided");

            var newDiscussion = new Discussion
            {
                Title = createDiscussionDto.Title,
                Content = createDiscussionDto.Content,
                AppUserId = appUserId,
                GameId = createDiscussionDto.GameId
            };

            await _context.Discussions.AddAsync(newDiscussion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newDiscussion.Id }, newDiscussion);
        }
    }
}