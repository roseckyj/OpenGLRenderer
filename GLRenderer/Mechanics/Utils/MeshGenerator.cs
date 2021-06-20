using GLRenderer.Components;
using GLRenderer.Managers;
using GLRenderer.Mechanics.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace GLRenderer.Mechanics.Utils
{
    public static class MeshGenerator
    {
        private static HashSet<BlockType> TransparentBlocks = new() {
            BlockType.Air, BlockType.Leaves, BlockType.Cactus
        };

        public static Mesh GenerateChunkMesh(Chunk chunk)
        {
            float texSize = 1f / 16;

            List<Vertex> vertices = new();
            List<uint> indices = new();

            int faces = 0;

            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 256; y++)
                    for (int z = 0; z < 16; z++)
                    {
                        Block block = chunk.Blocks[x, y, z];
                        if (block.Type == BlockType.Air) continue;

                        var tex = GetTexCoords(block.Type);

                        // Top face
                        if (y == 255 ||
                            (TransparentBlocks.Contains(chunk.Blocks[x, y + 1, z].Type)))
                        {
                            vertices.AddRange(new Vertex[] {
                                VertexBuilder(x, 0, y, 1, z, 1, 0,  1,  0, tex.Item1.X + texSize * 0, tex.Item1.Y + texSize * 1, block.Type),
                                VertexBuilder(x, 1, y, 1, z, 1, 0,  1,  0, tex.Item1.X + texSize * 1, tex.Item1.Y + texSize * 1, block.Type),
                                VertexBuilder(x, 1, y, 1, z, 0, 0,  1,  0, tex.Item1.X + texSize * 1, tex.Item1.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 0, y, 1, z, 0, 0,  1,  0, tex.Item1.X + texSize * 0, tex.Item1.Y + texSize * 0, block.Type)
                            });
                            faces++;
                        }

                        // Bottom face
                        if (y == 0 ||
                            (TransparentBlocks.Contains(chunk.Blocks[x, y - 1, z].Type)))
                        {
                            vertices.AddRange(new Vertex[] {
                                VertexBuilder(x, 0, y, 0, z, 0, 0, -1,  0, tex.Item3.X + texSize * 0, tex.Item3.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 1, y, 0, z, 0, 0, -1,  0, tex.Item3.X + texSize * 1, tex.Item3.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 1, y, 0, z, 1, 0, -1,  0, tex.Item3.X + texSize * 1, tex.Item3.Y + texSize * 1, block.Type),
                                VertexBuilder(x, 0, y, 0, z, 1, 0, -1,  0, tex.Item3.X + texSize * 0, tex.Item3.Y + texSize * 1, block.Type)
                            });
                            faces++;
                        }

                        // Left face
                        if (x == 0 ||
                            (TransparentBlocks.Contains(chunk.Blocks[x - 1, y, z].Type)))
                        {
                            vertices.AddRange(new Vertex[] {
                                VertexBuilder(x, 0, y, 0, z, 1,-1,  0,  0, tex.Item2.X + texSize * 1, tex.Item2.Y + texSize * 1, block.Type),
                                VertexBuilder(x, 0, y, 1, z, 1,-1,  0,  0, tex.Item2.X + texSize * 1, tex.Item2.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 0, y, 1, z, 0,-1,  0,  0, tex.Item2.X + texSize * 0, tex.Item2.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 0, y, 0, z, 0,-1,  0,  0, tex.Item2.X + texSize * 0, tex.Item2.Y + texSize * 1, block.Type),
                            });
                            faces++;
                        }

                        // Right face
                        if (x == 15 ||
                            (TransparentBlocks.Contains(chunk.Blocks[x + 1, y, z].Type)))
                        {
                            vertices.AddRange(new Vertex[] {
                                VertexBuilder(x, 1, y, 0, z, 0, 1,  0,  0, tex.Item2.X + texSize * 1, tex.Item2.Y + texSize * 1, block.Type),
                                VertexBuilder(x, 1, y, 1, z, 0, 1,  0,  0, tex.Item2.X + texSize * 1, tex.Item2.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 1, y, 1, z, 1, 1,  0,  0, tex.Item2.X + texSize * 0, tex.Item2.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 1, y, 0, z, 1, 1,  0,  0, tex.Item2.X + texSize * 0, tex.Item2.Y + texSize * 1, block.Type),
                            });
                            faces++;
                        }

                        // Front face
                        if (z == 0 ||
                            (TransparentBlocks.Contains(chunk.Blocks[x, y, z - 1].Type)))
                        {
                            vertices.AddRange(new Vertex[] {
                                VertexBuilder(x, 0, y, 0, z, 0, 0,  0, -1, tex.Item2.X + texSize * 0, tex.Item2.Y + texSize * 1, block.Type),
                                VertexBuilder(x, 0, y, 1, z, 0, 0,  0, -1, tex.Item2.X + texSize * 0, tex.Item2.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 1, y, 1, z, 0, 0,  0, -1, tex.Item2.X + texSize * 1, tex.Item2.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 1, y, 0, z, 0, 0,  0, -1, tex.Item2.X + texSize * 1, tex.Item2.Y + texSize * 1, block.Type),
                            });
                            faces++;
                        }

                        // Back face
                        if (z == 15 ||
                            (TransparentBlocks.Contains(chunk.Blocks[x, y, z + 1].Type)))
                        {
                            vertices.AddRange(new Vertex[] {
                                VertexBuilder(x, 1, y, 0, z, 1, 0,  0,  1, tex.Item2.X + texSize * 0, tex.Item2.Y + texSize * 1, block.Type),
                                VertexBuilder(x, 1, y, 1, z, 1, 0,  0,  1, tex.Item2.X + texSize * 0, tex.Item2.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 0, y, 1, z, 1, 0,  0,  1, tex.Item2.X + texSize * 1, tex.Item2.Y + texSize * 0, block.Type),
                                VertexBuilder(x, 0, y, 0, z, 1, 0,  0,  1, tex.Item2.X + texSize * 1, tex.Item2.Y + texSize * 1, block.Type)
                            });
                            faces++;
                        }
                    }

            for (uint i = 0; i < faces * 4; i += 4)
            {
                indices.AddRange(new uint[] {
                    i + 0, i + 1, i + 2, i + 2, i + 3, i + 0
                });
            }

            return new Mesh(vertices, indices, Manager.Material.Get("block"));
        }

        private static Vertex VertexBuilder(float x, float sx, float y, float sy, float z, float sz, float nx, float ny, float nz, float u, float v, BlockType type) {
            if (type == BlockType.Cactus && ny == 0) {
                if (nx == 0)
                {
                    if (sz == 1) sz -= 1f / 16;
                    if (sz == 0) sz += 1f / 16;
                }
                else
                {
                    if (sx == 1) sx -= 1f / 16;
                    if (sx == 0) sx += 1f / 16;
                }
            }
            return new Vertex(new Vector3(x + sx, y + sy, z + sz), new Vector3(nx, ny, nz), new Vector2(u, v));
        }

        private static Tuple<Vector2, Vector2, Vector2> GetTexCoords(BlockType type)
        {
            Vector2 top = new(12, 14); // WIP texture
            Vector2 side = new(12, 14); // WIP texture
            Vector2 bottom = new(12, 14); // WIP texture
            switch (type)
            {
                case BlockType.Bedrock:
                    top = new(1, 1);
                    side = new(1, 1);
                    bottom = new(1, 1);
                    break;
                case BlockType.Dirt:
                    top = new(2, 0);
                    side = new(2, 0);
                    bottom = new(2, 0);
                    break;
                case BlockType.Grass:
                    top = new(8, 2);
                    side = new(3, 0);
                    bottom = new(2, 0);
                    break;
                case BlockType.Stone:
                    top = new(1, 0);
                    side = new(1, 0);
                    bottom = new(1, 0);
                    break;
                case BlockType.Log:
                    top = new(5, 1);
                    side = new(4, 1);
                    bottom = new(5, 1);
                    break;
                case BlockType.Leaves:
                    top = new(4, 3);
                    side = new(4, 3);
                    bottom = new(4, 3);
                    break;
                case BlockType.Sandstone:
                    top = new(0, 11);
                    side = new(0, 12);
                    bottom = new(0, 13);
                    break;
                case BlockType.Sand:
                    top = new(2, 1);
                    side = new(2, 1);
                    bottom = new(2, 1);
                    break;
                case BlockType.Cactus:
                    top = new(5, 4);
                    side = new(6, 4);
                    bottom = new(7, 4);
                    break;
            }
            return new(top / 16, side / 16, bottom / 16);
        }
    }
}
