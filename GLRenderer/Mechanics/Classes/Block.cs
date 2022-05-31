using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Mechanics.Classes
{
    public class Block
    {
        public BlockType Type { get; }

        public Block(BlockType type) {
            Type = type;
        }
    }

    public enum BlockType {
        Air,
        Grass,
        Dirt,
        Stone,
        Bedrock,
        Log,
        Leaves,
        Cactus,
        Sand,
        Sandstone,
        Torch
    }
}
