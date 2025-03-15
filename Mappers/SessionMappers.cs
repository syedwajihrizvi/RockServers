using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO.Sessions;
using RockServers.Models;

namespace RockServers.Mappers
{
    public static class SessionMappers
    {
        public static SessionDto ToSessionDto(this Session session)
        {
            return new SessionDto
            {
                Id = session.Id,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Users = [.. session.Users.Select(s => s.ToSessionUserDto())],
                Active = session.Active
            };
        }

        public static SessionUserDto ToSessionUserDto(this SessionUser sessionUser)
        {
            return new SessionUserDto
            {
                SessionId = sessionUser.SessionId,
                UserId = sessionUser.AppUser!.Id,
                Username = sessionUser.AppUser.UserName!

            };
        }
    }
}