using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Mechanics.Classes
{
    public class Item
    {
        public ItemType Type;
        public int Count;

        public Item(ItemType type, int count)
        {
            Type = type;
            Count = count;
        }
    }

    public enum ItemType {
        Dirt,
        Log,
        Plank,
        Cobblestone,
        Sand,
        Cactus,
        Sandstone
    }
}
