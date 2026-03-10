using System;
using System.Linq;
using Helper;
using Color = System.Drawing.Color;

namespace SplatterServer
{
    public class ArenaTeamCollection : ListCollection<ArenaTeam>
    {
        public ArenaTeam NoTeam => this.FirstOrDefault(t => t.Team == Team.NoTeam);

        /*public ArenaTeam NoTeam
        {
            get
            {
                return this[0];
            }
        }*/

        public ArenaTeam Red
        {
            get
            {
                return this[1];
            }
        }

        public ArenaTeam Blue
        {
            get
            {
                return this[2];
            }
        }
        public ArenaTeam Yellow
        {
            get
            {
                return this[3];
            }
        }
        public ArenaTeam Green
        {
            get
            {
                return this[4];
            }
        }
        public ArenaTeamCollection(int NoTeam, int Red, int Yellow, int Blue, int Green)
        {
            if (NoTeam == 1)
            {
                Add(new ArenaTeam(Team.NoTeam));
            }
            else
            {
                if (Red > 0) Add(new ArenaTeam(Team.Red));    // Index 1
                if (Blue > 0) Add(new ArenaTeam(Team.Blue));   // Index 2
                if (Yellow > 0) Add(new ArenaTeam(Team.Yellow)); // Index 3
                if (Green > 0) Add(new ArenaTeam(Team.Green));  // Index 4      
            }
        }

        public bool HasTeam(Team targetTeam)
        {
            // Returns true if the team exists in our collection and isn't "NoTeam"
            return this.Any(t => t.Team == targetTeam && t.Team != Team.NoTeam);
        }
        public bool IsFFA => this.Count <= 1 || this.All(t => t.Team == Team.NoTeam);

        public ArenaTeam FindByTeam(Team team)
        {
            return this.FirstOrDefault(arenaTeam => team == arenaTeam.Team);
        }       

        public Boolean IsPlayerCarryingFlag(ArenaPlayer arenaPlayer)
        {
            return this.Any(arenaTeam => arenaPlayer == arenaTeam.Flag.FlagPlayer);
        }

        public ArenaTeam GetCarriedFlagTeam(ArenaPlayer arenaPlayer)
        {
            return this.FirstOrDefault(arenaTeam => arenaPlayer == arenaTeam.Flag.FlagPlayer);
        }
    }
}
