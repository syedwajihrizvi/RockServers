using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.DTO.Comments;
using RockServers.Extensions;
using RockServers.Helpers;
using RockServers.Mappers;

namespace RockServers.Controllers
{
    [Route("api/discussionComments")]
    [ApiController]
    public class DiscussionCommentController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DiscussionCommentController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var comments = await _context.DiscussionComments.ToListAsync();
            return Ok(comments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDiscussionCommentDto createDiscussionCommentDto)
        {
            // Ensure the Discussion Id sent was valid
            var discussion = await _context.Discussions.Where(d => d.Id == createDiscussionCommentDto.DiscussionId).FirstOrDefaultAsync();
            if (discussion == null)
                return NotFound($"Discussion with Id {createDiscussionCommentDto.DiscussionId} not found.");
            // Get the user Id of the person commenting
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User is invalid. Please ensure JWT token is in the request header.");
            var comment = createDiscussionCommentDto.ToDiscussionCommentFromCreate(appUserId);
            await _context.DiscussionComments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return Ok(createDiscussionCommentDto);

        }
    }
}