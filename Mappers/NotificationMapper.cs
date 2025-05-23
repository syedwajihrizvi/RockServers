using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Notifications;
using RockServers.Models;
using SixLabors.ImageSharp;

namespace RockServers.Mappers
{
    public static class NotificationMapper
    {
        public static NotificationDto ToNotificationDto(
            this Notification notification, string entityContent, int? postOrDesc)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Engager = notification?.Engager?.ToMinimalUserInformationDto(),
                Target = notification?.Target?.ToMinimalUserInformationDto(),
                EntityId = postOrDesc != null ? postOrDesc : notification?.EntityId,
                EntityContent = entityContent,
                IsRead = notification?.IsRead,
                CreatedAt = notification!.CreatedAt,
            };
        }
    }
}