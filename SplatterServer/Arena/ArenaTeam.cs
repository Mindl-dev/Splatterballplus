using System;
using Helper;


namespace SplatterServer
{
    public enum Team
    {
        NoTeam,
        Red,
        Blue,
        Yellow,
        Green,
    }
    public class ArenaTeam 
    {   
        public string Name { get; set; }

        public Team Team { get; set; }

        public CTFlag Flag;
        public ArenaTeam(Team Team)
        {            
            Int16 objectId = 0;

            this.Team = Team;

            Flag = new CTFlag(this.Team, objectId);
        }
    }
}
