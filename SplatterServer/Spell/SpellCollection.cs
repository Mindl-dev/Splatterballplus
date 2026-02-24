using Helper;

namespace SplatterServer
{
    public class SpellCollection : ListCollection<Spell>
    {
        public SpellCollection()
        {
            Add(new Spell());
        }
    }
}
