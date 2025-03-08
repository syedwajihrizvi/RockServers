using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.Mappers;

namespace RockServers.Controllers
{
    [Route("api/posts")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public PostController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.Posts
                                      .Include(p => p.Game)
                                      .Include(p => p.AppUser)
                                      .Include(p => p.Comments).ToListAsync();
            var postsDtos = posts.Select(p => p.ToPostDto());
            return Ok(postsDtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAll([FromRoute] int id)
        {
            var post = await _context.Posts.Where(p => p.Id == id)
                                     .Include(p => p.Game)
                                     .Include(p => p.AppUser)
                                     .Include(p => p.Comments).FirstOrDefaultAsync();
            if (post == null)
                return NotFound($"Post with {id} not found");
            return Ok(post.ToPostDto());
        }
    };
}