using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLRenderer.Components;
using GLRenderer.Managers;
using GLRenderer.Mechanics.Classes;
using GLRenderer.Mechanics.Utils;
using GLRenderer.Shaders.Static;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GLRenderer.Mechanics
{
    public class GameMechanic
    {
        private Game game;
        private Scene scene;

        private WorldGen generator;

        private Dictionary<Vector2i, Chunk> chunks = new();
        private HashSet<Vector2i> generatedChunks = new();
        private bool generatingChunk = false;
        private Queue<Chunk> chunkTemp = new();

        private Solid skybox;
        private Solid selector;
        private Solid character;

        public GameMechanic() {
            game = new Game();
            game.OnFrameUpdate += OnFrameUpdate;

            generator = new WorldGen((int)(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() % 1000000));
        }

        public void Start() {
            Manager.Scene.Create("main", new Scene());
            scene = Manager.Scene.Get("main");
            game.Scene = scene;

            Setup();

            game.Start();
        }

        private void Setup() {
            Manager.Texture.CreateFromFile("texturemap", "Resources/texturemap.png");
            Manager.Texture.CreateFromFile("skybox", "Resources/skybox.png");
            Manager.Texture.CreateFromFile("selector", "Resources/selector.png");

            Manager.Material.Create("block", new Material(Manager.Texture.Get("texturemap")) {
                Shininess = 2,
                SpecularColor = new Vector3(0.1f),
            });
            Manager.Material.Create("skybox", new Material(Manager.Texture.Get("skybox")));
            Manager.Material.Create("selector", new Material(Manager.Texture.Get("selector")));

            Manager.Model.CreateFromObjFile("steve", "Resources/models/steve.obj");

            scene.Components.Add(new Camera(new Vector3(0, 200, 0), game.Window.Size.X / game.Window.Size.Y));
            scene.Components.Add(new DirectionalLight(Quaternion.FromEulerAngles(-1f, 0.5f, 0.8f), Vector3.One, Vector3.One * 0.5f) {
                AmbientColor = Vector3.One * 0.4f
            });

            selector = new Solid(Model.Cube(Manager.Material.Get("selector")), UnlitShader.Instance) {
                Scale = new Vector3(1.01f),
                CastShadow = false
            };
            scene.Components.Add(selector);

            skybox = new Solid(Model.Cube(Manager.Material.Get("skybox")).Inverted(), UnlitShader.Instance) {
                Scale = new Vector3(500)
            };
            scene.Components.Add(skybox);

            character = new Solid(Manager.Model.Get("steve"), LitShader.Instance) {
                Scale = new Vector3(0.2f),
                Visible = false
            };
            scene.Components.Add(character);
        }

        private Vector3 velocity = Vector3.Zero;

        private void OnFrameUpdate(object sender, UpdateFrameEventArgs args) {

            #region Movement
            var input = args.KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                game.Close();
            }

            const float speedFloor = 25f;
            const float speedAir = 18f;
            const float sensitivity = 0.2f;
            const float pad = 0.1f;
            const float height = 1.5f;
            const float dragFloor = 0.85f;
            const float dragAir = 0.92f;
            const float gravity = 0.35f;
            const float sprint = 1.3f;
            const float sneak = 0.5f;
            const float jump = 9.0f;

            var forward = (scene.Camera.Front * new Vector3(1, 0, 1)).Normalized();
            var right = (scene.Camera.Right * new Vector3(1, 0, 1)).Normalized();

            var newPos = scene.Camera.Position;
            var oldPos = scene.Camera.Position;

            float speedCoef = 1;

            bool onFloor = MathF.Abs(velocity.Y) < 0.01f;
            if (input.IsKeyDown(Keys.Space))
            {
                if (onFloor) velocity.Y = jump;
            }
            onFloor = MathF.Abs(velocity.Y) < 0.01f;
            bool sneaking = false;
            if (input.IsKeyDown(Keys.LeftShift))
            {
                speedCoef *= sneak;
                sneaking = true;
            }
            if (input.IsKeyDown(Keys.LeftControl))
            {
                speedCoef *= sprint;
            }
            float speed = (onFloor ? speedFloor : speedAir) * speedCoef;

            if (input.IsKeyDown(Keys.W))
            {
                velocity += forward * speed * (float)args.DeltaTime; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                velocity -= forward * speed * (float)args.DeltaTime; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                velocity -= right * speed * (float)args.DeltaTime; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                velocity += right * speed * (float)args.DeltaTime; // Right
            }

            newPos += velocity * (float)args.DeltaTime;
            if (onFloor)
            {
                velocity.X *= dragFloor;
                velocity.Z *= dragFloor;
            }
            else
            {
                velocity.X *= dragAir;
                velocity.Z *= dragAir;
            }
            velocity.Y -= gravity;

            var oldBlock = new Vector3(MathF.Floor(oldPos.X), MathF.Floor(oldPos.Y - height), MathF.Floor(oldPos.Z));

            if (newPos.Y < oldPos.Y && (GetBlock(newPos + new Vector3(0, -height, 0)).Type != BlockType.Air)) {
                newPos = new Vector3(newPos.X, MathF.Max(oldBlock.Y + height, newPos.Y), newPos.Z);
                velocity.Y = 0;
            }
            if (newPos.Y > oldPos.Y && (GetBlock(newPos + new Vector3(0, pad, 0)).Type != BlockType.Air)) {
                newPos = new Vector3(newPos.X, MathF.Min(MathF.Ceiling(oldPos.Y) - pad, newPos.Y), newPos.Z);
                velocity.Y = 0;
            }
            if (GetBlock(oldPos + new Vector3(1, -height, 0)).Type != BlockType.Air ||
                GetBlock(oldPos + new Vector3(1, -height + 1, 0)).Type != BlockType.Air ||
                (sneaking && GetBlock(oldPos + new Vector3(1, -height - 1, 0)).Type == BlockType.Air) ||
                GetBlock(new Vector3(oldPos.X + 1, MathF.Ceiling(oldPos.Y - height + 1), oldPos.Z)).Type != BlockType.Air)
            {
                if (oldBlock.X + 1 - pad < newPos.X)
                {
                    newPos = new Vector3(oldBlock.X + 1 - pad, newPos.Y, newPos.Z);
                    velocity.X = 0;
                }
            }
            if (GetBlock(oldPos + new Vector3(-1, -height, 0)).Type != BlockType.Air ||
                GetBlock(oldPos + new Vector3(-1, -height + 1, 0)).Type != BlockType.Air ||
                (sneaking && GetBlock(oldPos + new Vector3(-1, -height - 1, 0)).Type == BlockType.Air) ||
                GetBlock(new Vector3(oldPos.X - 1, MathF.Ceiling(oldPos.Y - height + 1), oldPos.Z)).Type != BlockType.Air)
            {
                if (oldBlock.X + pad > newPos.X) {
                    newPos = new Vector3(oldBlock.X + pad, newPos.Y, newPos.Z);
                    velocity.X = 0;
                }
            }
            if (GetBlock(oldPos + new Vector3(0, -height, 1)).Type != BlockType.Air ||
                GetBlock(oldPos + new Vector3(0, -height + 1, 1)).Type != BlockType.Air ||
                (sneaking && GetBlock(oldPos + new Vector3(0, -height - 1, 1)).Type == BlockType.Air) ||
                GetBlock(new Vector3(oldPos.X, MathF.Ceiling(oldPos.Y - height + 1), oldPos.Z + 1)).Type != BlockType.Air)
            {
                if (oldBlock.Z + 1 - pad < newPos.Z)
                {
                    newPos = new Vector3(newPos.X, newPos.Y, oldBlock.Z + 1 - pad);
                    velocity.Z = 0;
                }
            }
            if (GetBlock(oldPos + new Vector3(0, -height, -1)).Type != BlockType.Air ||
                GetBlock(oldPos + new Vector3(0, -height + 1, -1)).Type != BlockType.Air ||
                (sneaking && GetBlock(oldPos + new Vector3(0, -height - 1, -1)).Type == BlockType.Air) ||
                GetBlock(new Vector3(oldPos.X, MathF.Ceiling(oldPos.Y - height + 1), oldPos.Z - 1)).Type != BlockType.Air)
            {
                if (oldBlock.Z + pad > newPos.Z)
                {
                    newPos = new Vector3(newPos.X, newPos.Y, oldBlock.Z + pad);
                    velocity.Z = 0;
                }
            }

            scene.Camera.Position = newPos;
            scene.Camera.Fov = 80 * ((speedCoef - 1) * 0.2f + 1);

            // Get the mouse state
            scene.Camera.RotateWithMouse(args.MouseState, sensitivity);

            #endregion

            #region Ray tracing

            const int reach = 5;

            var pos = scene.Camera.Position;
            var dir = scene.Camera.Front;
            var block = new Vector3i((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Y), (int)MathF.Floor(pos.Z));

            Dictionary<Vector3i, double> hit = new();

            for (int x = -reach; x <= reach; x++)
                for (int y = -reach; y <= reach; y++)
                    for (int z = -reach; z <= reach; z++)
                    {
                        Vector3i targetBlock = block - new Vector3i(x, y, z);
                        if (GetBlock(targetBlock).Type != BlockType.Air) {
                            if (x > 0) {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 1, y - 0, z - 0),
                                    block - new Vector3i(x - 1, y - 0, z - 1),
                                    block - new Vector3i(x - 1, y - 1, z - 0),
                                    block - new Vector3i(x - 1, y - 1, z - 1));

                                if (dist > 0)
                                    hit[targetBlock] = hit.ContainsKey(targetBlock) ? Math.Min(dist, hit[targetBlock]) : dist;
                            }
                            else if (x < 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 0, z - 0),
                                    block - new Vector3i(x - 0, y - 1, z - 0),
                                    block - new Vector3i(x - 0, y - 1, z - 1),
                                    block - new Vector3i(x - 0, y - 0, z - 1));

                                if (dist > 0)
                                    hit[targetBlock] = hit.ContainsKey(targetBlock) ? Math.Min(dist, hit[targetBlock]) : dist;
                            }
                            if (y > 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 1, z - 0),
                                    block - new Vector3i(x - 1, y - 1, z - 0),
                                    block - new Vector3i(x - 1, y - 1, z - 1),
                                    block - new Vector3i(x - 0, y - 1, z - 1));

                                if (dist > 0)
                                    hit[targetBlock] = hit.ContainsKey(targetBlock) ? Math.Min(dist, hit[targetBlock]) : dist;
                            }
                            else if (y < 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 0, z - 0),
                                    block - new Vector3i(x - 0, y - 0, z - 1),
                                    block - new Vector3i(x - 1, y - 0, z - 1),
                                    block - new Vector3i(x - 1, y - 0, z - 0));

                                if (dist > 0)
                                    hit[targetBlock] = hit.ContainsKey(targetBlock) ? Math.Min(dist, hit[targetBlock]) : dist;
                            }
                            if (z > 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 0, z - 1),
                                    block - new Vector3i(x - 0, y - 1, z - 1),
                                    block - new Vector3i(x - 1, y - 1, z - 1),
                                    block - new Vector3i(x - 1, y - 0, z - 1));

                                if (dist > 0)
                                    hit[targetBlock] = hit.ContainsKey(targetBlock) ? Math.Min(dist, hit[targetBlock]) : dist;
                            }
                            else if (z < 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 0, z - 0),
                                    block - new Vector3i(x - 1, y - 0, z - 0),
                                    block - new Vector3i(x - 1, y - 1, z - 0),
                                    block - new Vector3i(x - 0, y - 1, z - 0));

                                if (dist > 0)
                                    hit[targetBlock] = hit.ContainsKey(targetBlock) ? Math.Min(dist, hit[targetBlock]) : dist;
                            }
                        }
                    }

            if (hit.Count > 0)
            {
                var nearest = hit.OrderBy((h) => h.Value).First();
                if (Vector3.Distance(nearest.Key, pos) <= reach)
                {
                    selector.Enabled = true;
                    selector.Position = nearest.Key + new Vector3(0.5f);
                }
            }
            else {
                selector.Enabled = false;
            }

            #endregion

            skybox.Position = scene.Camera.Position;
            character.Position = scene.Camera.Position - new Vector3(0, height, 0);
            character.Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, -scene.Camera.Yaw + MathHelper.PiOver2);

            UpdateChunks();
        }

        private Block GetBlock(Vector3 pos) {
            var currentBlock = new Vector3i((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Y), (int)MathF.Floor(pos.Z));
            var currentChunk = new Vector2i((int)Math.Floor(pos.X / 16f), (int)Math.Floor(pos.Z / 16f));
            if (!chunks.ContainsKey(currentChunk)) return new Block(BlockType.Air);
            return chunks[currentChunk].GetBlock(currentBlock);
        }

        private void UpdateChunks()
        {
            int renderDistance = 6;
            int viewDistance = renderDistance + 2;

            Vector2i cameraChunk = new Vector2i((int)MathHelper.Floor(scene.Camera.Position.X / 16), (int)MathHelper.Floor(scene.Camera.Position.Z / 16));

            //Console.WriteLine($"Chunk: {cameraChunk.X} {cameraChunk.Y}");

            if (chunkTemp.Count > 0) {
                var chunk = chunkTemp.Dequeue();
                chunk.GenerateMesh();
                chunks[chunk.Position] = chunk;
                scene.Components.Add(chunk.Component);
            }

            var selected = chunks.Where((ch) => Vector2.Distance(cameraChunk, ch.Key) > viewDistance).ToList();
            foreach (var ch in selected) {
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

                        if (!chunks.ContainsKey(chunkCoords))
                        {
                            Chunk chunk = generator.GenerateChunk(chunkCoords);
                            chunkTemp.Enqueue(chunk);
                            generatedChunks.Add(chunkCoords);
                            generatingChunk = false;
                            return;
                        }
                        else
                        {
                            chunks[chunkCoords].Component.Enabled = true;
                        }
                    }
                }
                generatingChunk = false;
            });
        }
    }
}
