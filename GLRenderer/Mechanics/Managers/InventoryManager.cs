using GLRenderer.Mechanics.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Mechanics.Managers
{
    public class InventoryManager
    {
        public List<Item> Items = new();
        public int SelectedIndex = 0;

        public int AddItem(ItemType item, int count) {
            if (Items.Any((i) => i.Type == item))
            {
                var found = Items.First((i) => i.Type == item);
                if (found.Count >= count)
                {
                    found.Count += count;
                }
                else
                {
                    count = found.Count;
                    found.Count = 0;
                }
            }
            else
            {
                Items.Add(new Item(item, count));
            }
            return count;
        }

        public int RemoveItem(ItemType item, int count) {
            return AddItem(item, -count);
        }

        public void AddAfterDig(BlockType block) {
            switch (block) {
                case BlockType.Dirt:
                case BlockType.Grass:
                    AddItem(ItemType.Dirt, 1);
                    break;

                case BlockType.Cactus:
                    AddItem(ItemType.Cactus, 1);
                    break;

                case BlockType.Log:
                    AddItem(ItemType.Log, 1);
                    break;

                case BlockType.Sand:
                    AddItem(ItemType.Sand, 1);
                    break;

                case BlockType.Sandstone:
                    AddItem(ItemType.Sandstone, 1);
                    break;

                case BlockType.Stone:
                    AddItem(ItemType.Cobblestone, 1);
                    break;
            }
        }
    }
}
