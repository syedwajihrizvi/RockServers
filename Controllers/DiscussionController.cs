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
                {
                    var searchValue = queryObject.SearchValue.ToLower().Trim().Replace(" ", "");
                    discussions = discussions.Include(d => d.Game).Where(d => (
                        d.Title.ToLower().Trim().Replace(" ", "").Contains(searchValue) ||
                        d.Content.ToLower().Trim().Replace(" ", "").Contains(searchValue) ||
                        d.Game!.Title.ToLower().Trim().Replace(" ", "").Contains(searchValue) ||
                        d.Tags!.ToLower().Trim().Contains(searchValue)
                    ));
                }
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

        [HttpPost]
        [RequestSizeLimit(52428800)] // 50 MB
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
                    var generatedUniqueFileName = Guid.NewGuid().ToString();
                    var otherImageFileOutputPath = Path.Combine("wwwroot/uploads/images", $"{generatedUniqueFileName}.webp");
                    using var otherImage = await Image.LoadAsync(img.OpenReadStream());
                    await otherImage.SaveAsync(otherImageFileOutputPath, new WebpEncoder());
                    var imgPublicUrl = $"/uploads/images/{generatedUniqueFileName}.webp";
                    otherImages.Add(generatedUniqueFileName);
                }
            }

            // Extract the videos
            List<string> videos = [];
            if (createDiscussionDto.OtherVideos.Length > 0)
            {
                foreach (var video in createDiscussionDto.OtherVideos)
                {
                    var generatedUniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(video.FileName)}";
                    var otherVideoFileOutputPath = Path.Combine("wwwroot/uploads/videos", generatedUniqueFileName);
                    using (var stream = new FileStream(otherVideoFileOutputPath, FileMode.Create))
                    {
                        await video.CopyToAsync(stream);
                    }
                    var vidPublicUrl = $"/uploads/images/{generatedUniqueFileName}";
                    videos.Add(generatedUniqueFileName);
                }
            }

            var newDiscussion = new Discussion
            {
                Title = createDiscussionDto.Title,
                Content = createDiscussionDto.Content,
                AppUserId = appUserId,
                GameId = createDiscussionDto.GameId,
                OtherImages = otherImages,
                VideoPaths = videos
            };

            // Extract the thumbnail depending on its type
            if (createDiscussionDto.ThumbnailFile != null)
            {
                var thumbnailFile = createDiscussionDto.ThumbnailFile;
                if (thumbnailFile == null || thumbnailFile.Length == 0)
                    return BadRequest("No file found in request");
                // Get the type of file
                var fileType = thumbnailFile.ContentType;
                if (fileType.ToLower().Contains("video"))
                {
                    var generatedUniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailFile.FileName)}";
                    var outputpath = Path.Combine("wwwroot/uploads/videos", generatedUniqueFileName);
                    using (var stream = new FileStream(outputpath, FileMode.Create))
                    {
                        await thumbnailFile.CopyToAsync(stream);
                    }
                    newDiscussion.ThumbnailType = ThumbnailType.Video;
                    newDiscussion.ThumbnailPath = generatedUniqueFileName;
                }
                else if (fileType.ToLower().Contains("image"))
                {
                    // Extract the main image into a path first
                    var generatedUniqueFileName = Guid.NewGuid().ToString();
                    var outputpath = Path.Combine("wwwroot/uploads/images", $"{generatedUniqueFileName}.webp");
                    using var image = await Image.LoadAsync(thumbnailFile.OpenReadStream());
                    await image.SaveAsync(outputpath, new WebpEncoder());
                    var publicUrl = $"/uploads/images/{generatedUniqueFileName}.webp";
                    newDiscussion.ThumbnailType = ThumbnailType.Image;
                    newDiscussion.ThumbnailPath = generatedUniqueFileName;
                }
                else
                {
                    return BadRequest("Invalid file type detected");
                }
            }
            else
            {
                newDiscussion.ThumbnailPath = createDiscussionDto.ThumbnailPath;
            }

            newDiscussion.Tags = string.Join(" ", TagHelper.GenerateTags(newDiscussion.GameId, null));
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

        [HttpPatch("{discussionId:int}/updateViews")]
        public async Task<IActionResult> UpdateDiscussionViews([FromRoute] int discussionId)
        {
            var discussion = await _context.Discussions.Where(d => d.Id == discussionId).FirstOrDefaultAsync();
            if (discussion == null)
                return NotFound($"Discussion with {discussionId} not found");
            discussion.Views += 1;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [RequestSizeLimit(52428800)] // 50 MB
        [HttpPatch("{discussionId:int}")]
        public async Task<IActionResult> UpdateDiscussion([FromForm] UpdateDiscussionDto updateDiscussionDto, [FromRoute] int discussionId)
        {
            var discussion = await _context.Discussions.Where(d => d.Id == discussionId).FirstOrDefaultAsync();
            if (discussion == null)
                return NotFound("Discussion not found");
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid User Credentials");
            if (discussion.AppUserId != appUserId)
                return Unauthorized("Invalid User Credentials");

            if (updateDiscussionDto.Title != null)
                discussion.Title = updateDiscussionDto.Title;
            if (updateDiscussionDto.Content != null)
                discussion.Content = updateDiscussionDto.Content;
            if (updateDiscussionDto.GameId != null)
                discussion.GameId = updateDiscussionDto.GameId;

            if (updateDiscussionDto.ThumbnailFile != null)
            {
                var thumbnailFile = updateDiscussionDto.ThumbnailFile;
                if (thumbnailFile == null || thumbnailFile.Length == 0)
                    return BadRequest("No file found in request");
                // Get the type of file
                var fileType = thumbnailFile.ContentType;
                if (fileType.ToLower().Contains("video"))
                {
                    var generatedUniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailFile.FileName)}";
                    var outputpath = Path.Combine("wwwroot/uploads/videos", generatedUniqueFileName);
                    using (var stream = new FileStream(outputpath, FileMode.Create))
                    {
                        await thumbnailFile.CopyToAsync(stream);
                    }
                    discussion.ThumbnailType = ThumbnailType.Video;
                    discussion.ThumbnailPath = generatedUniqueFileName;
                }
                else if (fileType.ToLower().Contains("image"))
                {
                    // Extract the main image into a path first
                    var generatedUniqueFileName = Guid.NewGuid().ToString();
                    var outputpath = Path.Combine("wwwroot/uploads/images", $"{generatedUniqueFileName}.webp");
                    using var image = await Image.LoadAsync(thumbnailFile.OpenReadStream());
                    await image.SaveAsync(outputpath, new WebpEncoder());
                    var publicUrl = $"/uploads/images/{generatedUniqueFileName}.webp";
                    discussion.ThumbnailType = ThumbnailType.Image;
                    discussion.ThumbnailPath = generatedUniqueFileName;
                }
                else
                {
                    return BadRequest("Invalid file type detected");
                }
            }
            if (updateDiscussionDto.ThumbnailPath != null)
            {
                discussion.ThumbnailPath = updateDiscussionDto.ThumbnailPath;
                discussion.ThumbnailType = ThumbnailType.Image;
            }

            // Check the other images
            if (updateDiscussionDto.ExistingImages != null)
                discussion.OtherImages = updateDiscussionDto.ExistingImages.Where(i => !string.IsNullOrWhiteSpace(i)).ToList();
            if (updateDiscussionDto.ExistingVideos != null)
                discussion.VideoPaths = updateDiscussionDto.ExistingVideos.Where(i => !string.IsNullOrWhiteSpace(i)).ToList(); ;

            // Check newly uploaded files
            if (updateDiscussionDto.NewImages != null)
            {
                foreach (var img in updateDiscussionDto.NewImages)
                {
                    var generatedUniqueFileName = Guid.NewGuid().ToString();
                    var otherImageFileOutputPath = Path.Combine("wwwroot/uploads/images", $"{generatedUniqueFileName}.webp");
                    using var otherImage = await Image.LoadAsync(img.OpenReadStream());
                    await otherImage.SaveAsync(otherImageFileOutputPath, new WebpEncoder());
                    var imgPublicUrl = $"/uploads/images/{generatedUniqueFileName}.webp";
                    discussion.OtherImages.Add(generatedUniqueFileName);
                }
            }
            if (updateDiscussionDto.NewVideos != null)
            {
                foreach (var video in updateDiscussionDto.NewVideos)
                {
                    var generatedUniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(video.FileName)}";
                    var otherVideoFileOutputPath = Path.Combine("wwwroot/uploads/videos", generatedUniqueFileName);
                    using (var stream = new FileStream(otherVideoFileOutputPath, FileMode.Create))
                    {
                        await video.CopyToAsync(stream);
                    }
                    var vidPublicUrl = $"/uploads/images/{generatedUniqueFileName}";
                    discussion.VideoPaths.Add(generatedUniqueFileName);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(discussion);
        }

        [HttpDelete("{discussionId:int}")]
        [Authorize]
        public async Task<IActionResult> Delete([FromRoute] int discussionId)
        {
            var appUserId = User.GetUserId();
            if (appUserId == null)
                return Unauthorized("Invalid User Credentials");
            var discussion = await _context.Discussions.FindAsync(discussionId);
            if (discussion == null)
                return NotFound($"Discussion with {discussionId} not found");
            if (discussion.AppUserId != appUserId)
                return Unauthorized("Invalid User Credentials Provided");
            _context.Remove(discussion);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}