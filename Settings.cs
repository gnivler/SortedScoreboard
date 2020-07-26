using System.Collections.Generic;

namespace SortedScoreboard
{
    public class Settings
    {
        internal readonly List<string> PlayerSortOrder;
        internal readonly List<string> EnemySortOrder;

        public Settings(Dictionary<string, List<string>> input = null)
        {
            if (input == null)
            {
                PlayerSortOrder = new List<string>
                {
                    "GainedSkills",
                    "ReadyToUpgrade",
                    "Killed",
                    "Wounded",
                    "Kills"
                };

                EnemySortOrder = new List<string>
                {
                    "Killed",
                    "Wounded",
                    "GainedSkills",
                    "ReadyToUpgrade",
                    "Kills"
                };
            }
            else
            {
                PlayerSortOrder = input["Player"];
                EnemySortOrder = input["Enemy"];
            }
        }
    }
}
