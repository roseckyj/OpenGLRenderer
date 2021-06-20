using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLRenderer.Mechanics.Classes;
using GLRenderer.Mechanics.Utils;
using OpenTK.Mathematics;

namespace GLRenderer.Mechanics
{
    public class WorldGen
    {
        public int Seed { get; }

        private Random random;

        public WorldGen(int seed)
        {
            Seed = seed;
            random = new Random(seed);
            NoiseHelper.Seed = seed;
        }

        private int TerrainHeight(int x, int z)
        {
            float noise1Scale = 0.01f; // Bumps
            float noise2Scale = 0.05f; // Hills
            float noise3Scale = 0.001f; // Mountains

            return (int)(
                NoiseHelper.CarmodyNoise(
                    x * noise1Scale,
                    1,
                    z * noise1Scale,
                    false, true) * 20 +
                NoiseHelper.CarmodyNoise(
                    x * noise2Scale,
                    1,
                    z * noise2Scale,
                    false, true) * 7 +
                NoiseHelper.CarmodyNoise(
                    x * noise3Scale,
                    1,
                    z * noise3Scale,
                    false, true) * 80 +
                100);
        }

        private bool TreeNoise(int x, int z, float threshold)
        {
            return NoiseHelper.CarmodyNoise(x * 100, 10, z * 100, false, true) > threshold;
        }
        private int TreeSize(int x, int z)
        {
            return (int)(NoiseHelper.CarmodyNoise(x * 100, 20, z * 100, false, true) * 2) + 5;
        }

        public Chunk GenerateChunk(Vector2i position)
        {
            float biomeScale = 0.001f;

            Block[,,] blocks = new Block[16, 256, 16];

            for (int x = 0; x < 16; x++)
                for (int z = 0; z < 16; z++)
                {
                    int posX = (position.X * 16 + x);
                    int posZ = (position.Y * 16 + z);

                    int height = TerrainHeight(posX, posZ);

                    switch ((int)(NoiseHelper.CarmodyNoise(
                        posX * biomeScale,
                        1,
                        posZ * biomeScale,
                        false, true) * 1) + 2)
                    {
                        case 0:
                            // Desert
                            {
                                for (int y = 0; y < 256; y++)
                                {
                                    BlockType type = BlockType.Air;
                                    if (y == 0)
                                    {
                                        type = BlockType.Bedrock;
                                    }
                                    else if (height - 5 > y)
                                    {
                                        type = BlockType.Stone;
                                    }
                                    else if (height - 4 > y)
                                    {
                                        type = BlockType.Sandstone;
                                    }
                                    else if (height > y)
                                    {
                                        type = BlockType.Sand;
                                    }
                                    blocks[x, y, z] = new Block(type);
                                }

                                // Features
                                if (TreeNoise(posX, posZ, 0.95f))
                                {
                                    // Spawn cactus
                                    int size = random.Next(1, 4);
                                    for (int i = 0; i < size; i++)
                                    {
                                        blocks[x, height + i, z] = new Block(BlockType.Cactus);
                                    }
                                }
                            }
                            break;

                        case 1:
                            // Plains
                            {
                                for (int y = 0; y < 256; y++)
                                {
                                    BlockType type = BlockType.Air;
                                    if (y == 0)
                                    {
                                        type = BlockType.Bedrock;
                                    }
                                    else if (height - 4 > y)
                                    {
                                        type = BlockType.Stone;
                                    }
                                    else if (height - 1 > y)
                                    {
                                        type = BlockType.Dirt;
                                    }
                                    else if (height > y)
                                    {
                                        type = BlockType.Grass;
                                    }
                                    else if (y > height && y < height + 15)
                                    {
                                        // Leaves
                                        for (int x1 = -3; x1 <= 3; x1++)
                                        {
                                            for (int z1 = -3; z1 <= 3; z1++)
                                            {
                                                if (TreeNoise(posX + x1, posZ + z1, 0.98f))
                                                {
                                                    int y1 = TerrainHeight(posX + x1, posZ + z1);
                                                    int size = TreeSize(posX + x1, posZ + z1);
                                                    if (Vector3.Distance(new Vector3i(posX + x1, y1 + size, posZ + z1), new Vector3i(posX, y, posZ)) < size * 0.5f)
                                                    {
                                                        type = BlockType.Leaves;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    blocks[x, y, z] = new Block(type);
                                }

                                // Features
                                if (TreeNoise(posX, posZ, 0.98f))
                                {
                                    // Spawn tree
                                    int size = TreeSize(posX, posZ);
                                    for (int i = 0; i < size; i++)
                                    {
                                        blocks[x, height + i, z] = new Block(BlockType.Log);
                                    }
                                }
                            }
                            break;

                        case 2:
                            // Forrest
                            {
                                for (int y = 0; y < 256; y++)
                                {
                                    BlockType type = BlockType.Air;
                                    if (y == 0)
                                    {
                                        type = BlockType.Bedrock;
                                    }
                                    else if (height - 4 > y)
                                    {
                                        type = BlockType.Stone;
                                    }
                                    else if (height - 1 > y)
                                    {
                                        type = BlockType.Dirt;
                                    }
                                    else if (height > y)
                                    {
                                        type = BlockType.Grass;
                                    }
                                    else if (y > height && y < height + 15)
                                    {
                                        // Leaves
                                        for (int x1 = -3; x1 <= 3; x1++)
                                        {
                                            for (int z1 = -3; z1 <= 3; z1++)
                                            {
                                                if (TreeNoise(posX + x1, posZ + z1, 0.92f))
                                                {
                                                    int y1 = TerrainHeight(posX + x1, posZ + z1);
                                                    int size = TreeSize(posX + x1, posZ + z1);
                                                    if (Vector3.Distance(new Vector3i(posX + x1, y1 + size, posZ + z1), new Vector3i(posX, y, posZ)) < size * 0.5f)
                                                    {
                                                        type = BlockType.Leaves;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    blocks[x, y, z] = new Block(type);
                                }

                                // Features
                                if (TreeNoise(posX, posZ, 0.92f))
                                {
                                    // Spawn tree
                                    int size = TreeSize(posX, posZ);
                                    for (int i = 0; i < size; i++)
                                    {
                                        blocks[x, height + i, z] = new Block(BlockType.Log);
                                    }
                                }
                            }
                            break;
                    }
                }

            return new Chunk(position, blocks);
        }
    }
}
