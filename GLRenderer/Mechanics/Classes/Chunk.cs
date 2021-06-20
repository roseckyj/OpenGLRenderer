using GLRenderer.Components;
using GLRenderer.Managers;
using GLRenderer.Mechanics.Utils;
using GLRenderer.Shaders.Static;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Mechanics.Classes
{
    public class Chunk
    {
        public Block[,,] Blocks { get; set; }
        public Solid Component { get; private set; }
        public Vector2i Position { get; }

        public Chunk(Vector2i position, Block[,,] blocks) {
            Blocks = blocks;
            Position = position;
        }

        private IEnumerable<Vertex> vertices_tmp;
        private IEnumerable<uint> indices_tmp;
        public void GenerateMesh() {
            var mesh = MeshGenerator.GenerateChunkMesh(this);
            vertices_tmp = mesh.Item1;
            indices_tmp = mesh.Item2;
        }

        public void CreateComponent()
        {
            if (Component != null) {
                Component.Dispose();
            }
            Component = new Solid(new Model(new Mesh(vertices_tmp, indices_tmp, Manager.Material.Get("block"))), LitShader.Instance)
            {
                Position = new Vector3(Position.X * 16, 0, Position.Y * 16)
            };
        }

        public Block GetBlock(Vector3i position)
        {
            if (position.Y < 0 || position.Y > 255) return new Block(BlockType.Air);
            
            return Blocks[(position.X % 16 + 16) % 16, position.Y, (position.Z % 16 + 16) % 16];
        }

        public void SetBlock(Vector3i position, Block block)
        {
            if (position.Y < 0 || position.Y > 255) return;

            Blocks[(position.X % 16 + 16) % 16, position.Y, (position.Z % 16 + 16) % 16] = block;
        }
    }
}
