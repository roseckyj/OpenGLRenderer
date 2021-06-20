using GLRenderer.Components;
using GLRenderer.Mechanics.Classes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Mechanics.Managers
{
    class BlockManager
    {
        private WorldGen generator;
        private Scene scene;

        public Dictionary<Vector2i, Chunk> Chunks = new();
        private HashSet<Vector2i> generatedChunks = new();
        private bool generatingChunk = false;
        private Queue<Chunk> chunkTemp = new();

        public BlockManager(int seed, Scene scene) {
            generator = new WorldGen(seed);
            this.scene = scene;
        }

        public Block GetBlock(Vector3 pos)
        {
            var currentBlock = new Vector3i((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Y), (int)MathF.Floor(pos.Z));
            var chunk = GetChunk(pos);
            if (chunk == null) return new Block(BlockType.Air);
            return chunk.GetBlock(currentBlock);
        }

        public void SetBlock(Vector3 pos, Block block)
        {
            var currentBlock = new Vector3i((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Y), (int)MathF.Floor(pos.Z));
            var chunk = GetChunk(pos);
            if (chunk == null) return;
            chunk.SetBlock(currentBlock, block);
        }

        public Chunk GetChunk(Vector3 pos)
        {
            var currentChunk = new Vector2i((int)Math.Floor(pos.X / 16f), (int)Math.Floor(pos.Z / 16f));
            if (!Chunks.ContainsKey(currentChunk)) return null;
            return Chunks[currentChunk];
        }

        public void Update()
        {
            int renderDistance = 6;
            int viewDistance = renderDistance + 2;

            Vector2i cameraChunk = new Vector2i((int)MathHelper.Floor(scene.Camera.Position.X / 16), (int)MathHelper.Floor(scene.Camera.Position.Z / 16));

            //Console.WriteLine($"Chunk: {cameraChunk.X} {cameraChunk.Y}");

            if (chunkTemp.Count > 0)
            {
                var chunk = chunkTemp.Dequeue();
                chunk.CreateComponent();
                Chunks[chunk.Position] = chunk;
                scene.Components.Add(chunk.Component);
            }

            var selected = Chunks.Where((ch) => Vector2.Distance(cameraChunk, ch.Key) > viewDistance).ToList();
            foreach (var ch in selected)
            {
                ch.Value.Component.Enabled = false;
            }

            if (generatingChunk) return;
            generatingChunk = true;

            Task.Run(() =>
            {
                for (int d = 0; d <= renderDistance; d++)
                {
                    for (float a = 0; a < MathHelper.TwoPi; a += 0.01f)
                    {
                        Vector2i chunkCoords = new(
                            (int)Math.Floor(MathF.Cos(a) * d) + cameraChunk.X,
                            (int)Math.Floor(MathF.Sin(a) * d) + cameraChunk.Y);

                        if (!Chunks.ContainsKey(chunkCoords))
                        {
                            Chunk chunk = generator.GenerateChunk(chunkCoords);
                            chunk.GenerateMesh();
                            chunkTemp.Enqueue(chunk);
                            generatedChunks.Add(chunkCoords);
                            generatingChunk = false;
                            return;
                        }
                        else
                        {
                            Chunks[chunkCoords].Component.Enabled = true;
                        }
                    }
                }
                generatingChunk = false;
            });
        }
    }
}
