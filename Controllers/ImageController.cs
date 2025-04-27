using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;

namespace RockServers.Controllers
{
    [Route("api/images")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public ImageController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetImages([FromQuery] int gameId)
        {
            var images = await _context.Images.Where(i => i.GameId == gameId).ToListAsync();
            return Ok(images);
        }

        [HttpGet("avatars")]
        public async Task<IActionResult> GetAvatars()
        {
            var avatars = await _context.Avatars.ToListAsync();
            return Ok(avatars);
        }
    }
}