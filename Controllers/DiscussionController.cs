using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
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
                if (queryObject.SearchValue != null)
                    discussions = discussions.Include(d => d.Game).Where(d => (
                        d.Title.ToLower().Trim().Replace(" ", "").Contains(queryObject.SearchValue.ToLower().Trim().Replace(" ", "")) ||
                        d.Content.ToLower().Trim().Replace(" ", "").Contains(queryObject.SearchValue.ToLower().Trim().Replace(" ", "")) ||
                        d.Game!.Title.ToLower().Trim().Replace(" ", "").Contains(queryObject.SearchValue.ToLower().Trim().Replace(" ", ""))
                    ));

                // Check for latest
                if (queryObject.MostRecent)
                    discussions = discussions.OrderByDescending(d => d.PostedAt);

                // Check if we ant to remove any discussions
                if (queryObject.DiscussionToRemoveId != null)
                    discussions = discussions.Where(d => d.Id != queryObject.DiscussionToRemoveId);

                if (!string.IsNullOrWhiteSpace(queryObject.OrderBy))
                {
                    if (queryObject.OrderBy == "likes")
                        discussions = discussions.OrderByDescending(d => d.Likes);
                    else if (queryObject.OrderBy == "comments")
                        discussions = discussions.OrderByDescending(d => d.DiscussionComments.Count);
                    else if (queryObject.OrderBy == "views")
                        discussions = discussions.OrderByDescending(d => d.Views);
                }

                if (!string.IsNullOrWhiteSpace(queryObject.UserId))
                    discussions = discussions.Where(d => d.AppUserId == queryObject.UserId);

                // Check for limit
                if (queryObject.Limit != null)
                    discussions = discussions.Take((int)queryObject.Limit);
            }
            ;

            var discussionDtos = await discussions.Include(d => d.Game)
                                           .Include(d => d.AppUser)
                                           .ThenInclude(a => a!.Avatar)
                                           .Include(d => d.DiscussionComments)
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
                                                       .ThenInclude(a => a!.Avatar)
                                                       .Include(d => d.DiscussionComments)
                                                       .ThenInclude(c => c.AppUser)
                                                       .ThenInclude(a => a!.Avatar)
                                                       .Include(d => d.DiscussionComments)
                                                       .ThenInclude(c => c.Replies)
                                                       .ThenInclude(r => r.AppUser)
                                                       .FirstOrDefaultAsync();
            if (discussion == null)
                return NotFound($"Discussion ID with {id} does not exist.");
            return Ok(discussion.ToGetDiscussionDto());
        }

        [HttpPost("customImage")]
        [Authorize]
        public async Task<IActionResult> CreateWithCustomImage([FromForm] CreateDiscussionDtoWithCustomImage createDiscussionDto)
        {
            var gameId = createDiscussionDto.GameId;
            var game = await _context.Games.Where(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null)
                return NotFound($"Game with ID {gameId} not found");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return NotFound("Invalid User ID Provided");
            var imageFile = createDiscussionDto.ImageFile;
            if (imageFile == null || imageFile.Length == 0)
                return BadRequest("No file found in request");
            // Extract the main image into a path first
            var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            var outputpath = Path.Combine("wwwroot/uploads/post_images", $"{fileName}.webp");
            using var image = await Image.LoadAsync(imageFile.OpenReadStream());
            await image.SaveAsync(outputpath, new WebpEncoder());
            var publicUrl = $"/uploads/post_images/{fileName}.webp";

            // Extract the other images
            List<string> otherImages = [];
            if (createDiscussionDto.OtherImages.Length > 0)
            {
                foreach (var img in createDiscussionDto.OtherImages)
                {
                    var otherImageFileName = Path.GetFileNameWithoutExtension(img.FileName);
                    var otherImageFileOutputPath = Path.Combine("wwwroot/uploads/post_images", $"{otherImageFileName}.webp");
                    using var otherImage = await Image.LoadAsync(img.OpenReadStream());
                    await otherImage.SaveAsync(otherImageFileOutputPath, new WebpEncoder());
                    var imgPublicUrl = $"/uploads/post_images/{otherImageFileName}.webp";
                    otherImages.Add(otherImageFileName);
                }
            }

            var newDiscussion = new Discussion
            {
                Title = createDiscussionDto.Title,
                Content = createDiscussionDto.Content,
                AppUserId = appUserId,
                GameId = createDiscussionDto.GameId,
                ThumbnailPath = fileName,
                OtherImages = otherImages
            };

            await _context.Discussions.AddAsync(newDiscussion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newDiscussion.Id }, newDiscussion);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromForm] CreateDiscussionDto createDiscussionDto)
        {
            var gameId = createDiscussionDto.GameId;
            var game = await _context.Games.Where(g => g.Id == gameId).FirstOrDefaultAsync();
            if (game == null)
                return NotFound($"Game with ID {gameId} not found");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return NotFound("Invalid User ID Provided");
            // Extract the other images
            List<string> otherImages = [];
            if (createDiscussionDto.OtherImages.Length > 0)
            {
                foreach (var img in createDiscussionDto.OtherImages)
                {
                    var otherImageFileName = Path.GetFileNameWithoutExtension(img.FileName);
                    var otherImageFileOutputPath = Path.Combine("wwwroot/uploads/post_images", $"{otherImageFileName}.webp");
                    using var otherImage = await Image.LoadAsync(img.OpenReadStream());
                    await otherImage.SaveAsync(otherImageFileOutputPath, new WebpEncoder());
                    var imgPublicUrl = $"/uploads/post_images/{otherImageFileName}.webp";
                    otherImages.Add(otherImageFileName);
                }
            }
            // Extract the videos
            List<string> videos = [];
            if (createDiscussionDto.OtherVideos.Length > 0)
            {
                foreach (var video in createDiscussionDto.OtherVideos)
                {
                    var otherVideoFileOutputPath = Path.Combine("wwwroot/uploads/discussion_videos", video.FileName);
                    // Save the video stream directly to the file
                    using (var stream = new FileStream(otherVideoFileOutputPath, FileMode.Create))
                    {
                        await video.CopyToAsync(stream);
                    }
                    var vidPublicUrl = $"/uploads/post_images/{video.FileName}";
                    videos.Add(video.FileName);
                }
            }
            var newDiscussion = new Discussion
            {
                Title = createDiscussionDto.Title,
                Content = createDiscussionDto.Content,
                AppUserId = appUserId,
                GameId = createDiscussionDto.GameId,
                ThumbnailPath = createDiscussionDto.ImagePath,
                OtherImages = otherImages,
                VideoPaths = videos

            };

            await _context.Discussions.AddAsync(newDiscussion);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = newDiscussion.Id }, newDiscussion);
        }

        [HttpPatch("{discussionId:int}/updateLikes")]
        public async Task<IActionResult> UpdateDiscussionLikes([FromRoute] int discussionId, [FromBody] bool increment)
        {
            var discussion = await _context.Discussions.Where(d => d.Id == discussionId).FirstOrDefaultAsync();
            if (discussion == null)
                return NotFound($"Discussion with {discussionId} does not exist");
            discussion.Likes += increment ? 1 : -1;
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("User not valid");
            var appUser = await _context.Users.Where(u => u.Id == appUserId)
                                              .Include(u => u.LikedDicussions)
                                              .FirstOrDefaultAsync();
            if (appUser == null)
                return Unauthorized("User not valid");
            if (increment)
                appUser.LikedDicussions.Add(discussion);
            else
                appUser.LikedDicussions.Remove(discussion);
            var notification = new Notification
            {
                Type = NotificationType.DiscussionLike,
                EngagerId = appUserId,
                TargetId = discussion.AppUserId!,
                EntityId = discussion.Id,
            };

            await Notification.SaveNotification(notification, _context);
            await _context.SaveChangesAsync();
            return Ok(discussion);
        }
    }
}