using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RockServers.Data;
using RockServers.DTO;
using RockServers.Helpers;
using RockServers.Mappers;

namespace RockServers.Controllers
{
    [Route("api/games")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public GameController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GameQueryObject queryObject)
        {
            var games = _context.Games.AsQueryable();
            if (queryObject.Title != null)
                games = games.Where(g => g.Title.Contains(queryObject.Title));
            if (queryObject.Slug != null)
                games = games.Where(g => g.Slug.Contains(queryObject.Slug));
            if (queryObject.Posts_eq != null)
                games = games.Where(g => g.Posts.Count() == queryObject.Posts_eq);
            if (queryObject.Posts_gte != null)
                games = games.Where(g => g.Posts.Count() >= queryObject.Posts_gte);
            if (queryObject.Posts_lte != null)
                games = games.Where(g => g.Posts.Count() <= queryObject.Posts_lte);
            if (queryObject.SortBy != null)
            {
                if (queryObject.SortBy.Equals("title", StringComparison.CurrentCultureIgnoreCase))
                    games = queryObject.Ascending ? games.OrderBy(g => g.Title) : games.OrderByDescending(g => g.Title);
                else if (queryObject.SortBy.Equals("id", StringComparison.CurrentCultureIgnoreCase))
                    games = queryObject.Ascending ? games.OrderBy(g => g.Id) : games.OrderByDescending(g => g.Id);
                else if (queryObject.SortBy.Equals("slug", StringComparison.CurrentCultureIgnoreCase))
                    games = queryObject.Ascending ? games.OrderBy(g => g.Slug) : games.OrderByDescending(g => g.Slug);
            }
            var gameDtos = await games.Select(g => g.ToCreatedGameDto()).ToListAsync();
            return Ok(gameDtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            var game = await _context.Games.FirstOrDefaultAsync(s => s.Id == id);
            if (game == null)
                return NotFound();
            return Ok(game.ToCreatedGameDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateGameDto createGameDto)
        {
            // If database already includes game with title, Bad request
            var slug = createGameDto.GetSlug();
            var gameExists = await _context.Games.Where(g => g.Slug == slug).FirstOrDefaultAsync();
            if (gameExists != null)
                return BadRequest("Game with this title already exists");
            var game = createGameDto.ToGameFromCreate();
            await _context.Games.AddAsync(game);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = game.Id }, game);
        }


    }
}