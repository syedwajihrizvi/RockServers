using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.DTO;
using RockServers.DTO.Games;
using RockServers.Models;

namespace RockServers.Mappers
{
    static public class GameMappers
    {
        public static Game ToGameFromCreate(this CreateGameDto createGameDto)
        {
            // Create a slug for the game
            // Ex: Red Dead Redemption 2 -> red-dead-redemption-2
            return new Game
            {
                Title = createGameDto.Title,
                Slug = createGameDto.GetSlug()
            };
        }

        public static CreatedGameDto ToCreatedGameDto(this Game gameModel)
        {
            return new CreatedGameDto
            {
                Id = gameModel.Id,
                Title = gameModel.Title,
                Slug = gameModel.Slug
            };
        }
    }
}