using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RockServers.Data;

namespace RockServers.Helpers
{
    public class TagHelper
    {
        public static List<string> gtaTags = ["gta", "grandtheftauto"];
        public static List<string> redDeadTags = ["red", "reddeadredemption"];
        public static List<string> playstationTags = ["playstation", "ps", "console"];
        public static List<string> xboxTags = ["xbox", "console"];
        public static List<string> computerTags = ["computer", "windows"];

        public static List<string> GenerateTags(int? gameId, int? platformId)
        {
            List<string> tags = [];
            // From the game itsel
            if (gameId == 3 || gameId == 4)
                tags = [.. tags, .. redDeadTags];
            else
                tags = [.. tags, .. gtaTags];

            if (platformId != null)
            {
                if (platformId == 1)
                    tags = [.. tags, .. playstationTags];
                else if (platformId == 2)
                    tags = [.. tags, .. xboxTags];
                else
                    tags = [.. tags, .. computerTags];
            }
            return tags;
        }
    }
}