using System;
using System.Linq;
using Helper;

namespace SplatterServer
{
    public class SignCollection : ListCollection<Sign>
    {
        public Sign FindById(Int16 objectId)
        {
            return this.FirstOrDefault(s => objectId == s.ObjectId);
        }
    }
}
