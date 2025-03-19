using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.Extensions;
using RockServers.Helpers;
using RockServers.Mappers;
using RockServers.Models;

namespace RockServers.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public SessionController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] SessionQueryObject queryObject)
        {
            var sessions = _context.Sessions.AsQueryable();
            if (queryObject != null)
            {
                if (queryObject.PostId != null)
                    sessions = sessions.Where(s => s.PostId == queryObject.PostId);
                // If we only want the active session or completed session
                if (queryObject.Active)
                    sessions = sessions.Where(s => s.EndTime == null);
                else if (queryObject.Completed)
                    sessions = sessions.Where(s => s.EndTime != null);
            }
            var fetchedSessions = await sessions.Include(s => s.Users)
                                                .ThenInclude(su => su.AppUser)
                                                .Include(s => s.Post).ToListAsync();
            return Ok(fetchedSessions.Select(s => s.ToSessionDto()));
        }

        [HttpGet("{sessionId:int}")]
        public async Task<IActionResult> Get([FromRoute] int sessionId)
        {
            var session = await _context.Sessions
                                        .Where(s => s.Id == sessionId)
                                        .Include(s => s.Users)
                                        .ThenInclude(su => su.AppUser).FirstOrDefaultAsync();
            if (session == null)
                return NotFound($"Session with Id {sessionId} not found");
            return Ok(session.ToSessionDto());
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateSession([FromBody] int postId)
        {
            Console.WriteLine($"Post ID from request: {postId}");
            var post = await _context.Posts.Where(p => p.Id == postId).FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Post with ID: {postId} not found");
            // Ensure the user Id from the JWT matches the user Id in the post
            var userIdFromHeader = User.GetUserId();
            if (userIdFromHeader == null || userIdFromHeader != post.AppUserId)
                return Unauthorized("Invalid JWT Token");
            // Cannot create a new session if post already has an active session
            var hasActiveSession = await _context.Sessions.Where(s => s.PostId == post.Id)
                                                           .Where(s => s.EndTime == null)
                                                           .ToListAsync();
            if (hasActiveSession.Count > 0)
                return BadRequest($"Post {postId} already has an active session. Please end before creating a new one.");
            var session = new Session
            {
                PostId = postId
            };
            // Add the user who is hosting the session to the list of session users
            await _context.Sessions.AddAsync(session);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { sessionId = session.Id }, session);
        }

        [HttpPatch("{sessionId:int}/addUser")]
        [Authorize]
        public async Task<IActionResult> AddUser([FromRoute] int sessionId)
        {
            var session = await _context.Sessions.Where(s => s.Id == sessionId).FirstOrDefaultAsync();
            if (session == null)
                return NotFound($"Session with Id {sessionId} does not exist");
            // If user already is session, return BadRequest
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User Id not found");
            var sessionExists = await _context.SessionUsers
                                              .Where(s => s.SessionId == sessionId)
                                              .Where(s => s.AppUserId == appUserId).FirstOrDefaultAsync();
            if (sessionExists != null)
                return BadRequest("User already part of session");
            var newSessionUser = new SessionUser
            {
                AppUserId = appUserId,
                SessionId = sessionId
            };
            await _context.SessionUsers.AddAsync(newSessionUser);
            session.Users.Add(newSessionUser);
            await _context.SaveChangesAsync();
            return Ok(newSessionUser);
        }

        [HttpPatch("{sessionId:int}/finish")]
        public async Task<IActionResult> FinishSession([FromRoute] int sessionId)
        {
            var session = await _context.Sessions.Where(s => s.Id == sessionId).FirstOrDefaultAsync();
            if (session == null)
                return NotFound($"Session with ID {sessionId} not found");
            session.EndTime = DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok(session);
        }
    }
}