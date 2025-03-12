using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.Helpers;

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
                Console.WriteLine($"SYED-DEBUG: {queryObject.Active}");
                if (queryObject.PostId != null)
                    sessions = sessions.Where(s => s.PostId == queryObject.PostId);
                // If we only want the active session or completed session
                if (queryObject.Active)
                    sessions = sessions.Where(s => s.EndTime == null);
                else if (queryObject.Completed)
                    sessions = sessions.Where(s => s.EndTime != null);
            }
            var fetchedSessions = await sessions.ToListAsync();
            return Ok(fetchedSessions);
        }

        [HttpGet("{sessionId:int}")]
        public async Task<IActionResult> Get([FromRoute] int sessionId)
        {
            var session = await _context.Sessions.Where(s => s.Id == sessionId).FirstOrDefaultAsync();
            if (session == null)
                return NotFound($"Session with Id {sessionId} not found");
            return Ok(session);
        }
    }
}