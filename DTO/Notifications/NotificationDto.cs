using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Accounts;
using RockServers.Models;

namespace RockServers.DTO.Notifications
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public NotificationType Type { get; set; }
        public MinimalUserInformationDto? Engager { get; set; }
        public MinimalUserInformationDto? Target { get; set; }
        public int? EntityId { get; set; }
        public string EntityContent { get; set; } = string.Empty;
        public bool? IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}