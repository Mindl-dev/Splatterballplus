using System;
using Helper;

namespace SplatterServer
{
    public class ArenaTeam
    {
        public Shrine Shrine;
        public CTFOrb ShrineOrb;
        
        public ArenaTeam()
        {            
            Int16 objectId = 0;                      

            ShrineOrb = new CTFOrb(Shrine.Team, objectId);
        }
    }
}
