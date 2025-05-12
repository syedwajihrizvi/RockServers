using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace RockServers.Extensions
{
    public static class AmazonExtensions
    {
        public static async Task<bool> CreateImage(this IAmazonS3 amazonS3, IFormFile thumbnailFile, string filename)
        {
            await using var memorystream = new MemoryStream();
            var generatedUniqueFileName = Guid.NewGuid().ToString();
            using var image = await Image.LoadAsync(thumbnailFile.OpenReadStream());
            await image.SaveAsync(memorystream, new WebpEncoder());
            memorystream.Position = 0;
            var request = new PutObjectRequest
            {
                BucketName = "rockserversbucket",
                Key = $"uploads/images/{filename}.webp",
                InputStream = memorystream,
                ContentType = thumbnailFile.ContentType
            };
            try
            {
                await amazonS3.PutObjectAsync(request);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public static async Task<bool> CreateVideo(this IAmazonS3 amazonS3, IFormFile thumbnailFile, string filename)
        {
            await using var memorystream = new MemoryStream();
            await thumbnailFile.CopyToAsync(memorystream);
            memorystream.Position = 0;
            var request = new PutObjectRequest
            {
                BucketName = "rockserversbucket",
                Key = $"uploads/videos/{filename}",
                InputStream = memorystream,
                ContentType = thumbnailFile.ContentType
            };
            try
            {
                await amazonS3.PutObjectAsync(request);
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }
}