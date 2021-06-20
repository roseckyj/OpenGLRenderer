using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLRenderer.Components;
using GLRenderer.Managers;
using GLRenderer.Mechanics.Classes;
using GLRenderer.Mechanics.Managers;
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

        private BlockManager blocks;
        private InventoryManager inventory;

        private Solid skybox;
        private Solid selector;
        private Solid character;
        private Label coords;
        private GUIPlane itemSelector;

        private Vector3i lookAt;
        private Vector3i lookAtSide = new Vector3i(0);

        private Vector3i digging;
        private float digProgress = 0;

        const float speedFloor = 30f;
        const float speedAir = 20f;
        const float sensitivity = 0.2f;
        const float pad = 0.1f;
        const float height = 1.5f;
        const float dragFloor = 0.005f; // Lower number -> higher drag
        const float dragAir = 0.015f;
        const float gravity = 25f; // Higher number -> higher gravity
        const float sprint = 1.3f;
        const float sneak = 0.5f;
        const float jump = 9.0f;

        public GameMechanic() {
            game = new Game();
            game.OnFrameUpdate += OnFrameUpdate;
        }

        public void Start() {
            Manager.Scene.Create("main", new Scene());
            scene = Manager.Scene.Get("main");
            game.Scene = scene;

            blocks = new BlockManager((int)(new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() % 1000000), scene);
            inventory = new InventoryManager();

            Setup();

            game.Start();
        }

        private void Setup() {
            Manager.Texture.CreateFromFile("texturemap", "Resources/texturemap.png");
            Manager.Texture.CreateFromFile("skybox", "Resources/skybox.png");
            Manager.Texture.CreateFromFile("selector", "Resources/selector.png");
            Manager.Texture.CreateFromFile("ascii", "Resources/ascii.png");
            Manager.Texture.CreateFromFile("icons", "Resources/icons.png");
            Manager.Texture.CreateFromFile("widgets", "Resources/widgets.png");
            for (int i = 0; i <= 10; i++)
            {
                Manager.Texture.CreateFromFile($"selector_{i}", $"Resources/Selector/{string.Format("{0:00}", i)}.png");
                Manager.Material.Create($"selector_{i}", new Material(Manager.Texture.Get($"selector_{i}")));

            }

            Manager.Material.Create("block", new Material(Manager.Texture.Get("texturemap")) {
                Shininess = 2,
                SpecularColor = new Vector3(0.1f),
            });
            Manager.Material.Create("skybox", new Material(Manager.Texture.Get("skybox")));
            Manager.Material.Create("selector", new Material(Manager.Texture.Get("selector")));
            Manager.Material.Create("ascii", new Material(Manager.Texture.Get("ascii")));
            Manager.Material.Create("icons", new Material(Manager.Texture.Get("icons")));
            Manager.Material.Create("widgets", new Material(Manager.Texture.Get("widgets")));

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

            coords = new Label(new Vector2(-0.95f, 0.93f), 0.010f, Manager.Material.Get("ascii"), "");
            scene.Components.Add(coords);

            itemSelector = new GUIPlane(new Vector2(0, -0.9f), new Vector2((0.3f / 9) * 1.2f), Manager.Material.Get("widgets"), new Vector2(0f / 256, 22f / 256), new Vector2(24f / 256, 46f / 256));
            scene.Components.Add(itemSelector);

            scene.Components.Add(new GUIPlane(Vector2.Zero, new Vector2(0.02f), Manager.Material.Get("icons"), Vector2.Zero, new Vector2(1 / 16f)));
            scene.Components.Add(new GUIPlane(new Vector2(0, -0.9f), new Vector2(0.3f, 0.3f * (22f / 182)), Manager.Material.Get("widgets"), Vector2.Zero, new Vector2(182f / 256, 22f / 256)));
        }

        private Vector3 velocity = Vector3.Zero;

        private void OnFrameUpdate(object sender, UpdateFrameEventArgs args) {

            UpdateMovement(args);
            UpdateLookAt(args);

            skybox.Position = scene.Camera.Position;
            character.Position = scene.Camera.Position - new Vector3(0, height, 0);
            character.Rotation = Quaternion.FromAxisAngle(Vector3.UnitY, -scene.Camera.Yaw + MathHelper.PiOver2);

            coords.Text = $"X:{Math.Floor(character.Position.X)} Y:{Math.Floor(character.Position.Y)} Z:{Math.Floor(character.Position.Z)}";

            itemSelector.Position = new Vector3((0.3f / 182 * 40) * (inventory.SelectedIndex - 4), -0.9f, -1f);

            blocks.Update();
        }


        private void UpdateMovement(UpdateFrameEventArgs args) {
            var input = args.KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                game.Close();
            }

            var forward = (scene.Camera.Front * new Vector3(1, 0, 1)).Normalized();
            var right = (scene.Camera.Right * new Vector3(1, 0, 1)).Normalized();

            var newPos = scene.Camera.Position;
            var oldPos = scene.Camera.Position;

            float speedCoef = 1;

            bool onFloor = velocity.Y == 0;
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
                velocity.X *= MathF.Pow(dragFloor, (float)args.DeltaTime);
                velocity.Z *= MathF.Pow(dragFloor, (float)args.DeltaTime);
            }
            else
            {
                velocity.X *= MathF.Pow(dragAir, (float)args.DeltaTime);
                velocity.Z *= MathF.Pow(dragAir, (float)args.DeltaTime);
            }
            velocity.Y -= gravity * (float)args.DeltaTime;

            var oldBlock = new Vector3(MathF.Floor(oldPos.X), MathF.Floor(oldPos.Y - height), MathF.Floor(oldPos.Z));

            if (newPos.Y < oldPos.Y && (blocks.GetBlock(newPos + new Vector3(0, -height, 0)).Type != BlockType.Air))
            {
                newPos = new Vector3(newPos.X, MathF.Max(oldBlock.Y + height, newPos.Y), newPos.Z);
                velocity.Y = 0;
            }
            if (newPos.Y > oldPos.Y && (blocks.GetBlock(newPos + new Vector3(0, pad, 0)).Type != BlockType.Air))
            {
                newPos = new Vector3(newPos.X, MathF.Min(MathF.Ceiling(oldPos.Y) - pad, newPos.Y), newPos.Z);
                velocity.Y = -0.01f;
            }
            if (blocks.GetBlock(oldPos + new Vector3(1, -height, 0)).Type != BlockType.Air ||
                blocks.GetBlock(oldPos + new Vector3(1, -height + 1, 0)).Type != BlockType.Air ||
                (sneaking && blocks.GetBlock(oldPos + new Vector3(1, -height - 1, 0)).Type == BlockType.Air) ||
                blocks.GetBlock(new Vector3(oldPos.X + 1, MathF.Ceiling(oldPos.Y - height + 1), oldPos.Z)).Type != BlockType.Air)
            {
                if (oldBlock.X + 1 - pad < newPos.X)
                {
                    newPos = new Vector3(oldBlock.X + 1 - pad, newPos.Y, newPos.Z);
                    velocity.X = 0;
                }
            }
            if (blocks.GetBlock(oldPos + new Vector3(-1, -height, 0)).Type != BlockType.Air ||
                blocks.GetBlock(oldPos + new Vector3(-1, -height + 1, 0)).Type != BlockType.Air ||
                (sneaking && blocks.GetBlock(oldPos + new Vector3(-1, -height - 1, 0)).Type == BlockType.Air) ||
                blocks.GetBlock(new Vector3(oldPos.X - 1, MathF.Ceiling(oldPos.Y - height + 1), oldPos.Z)).Type != BlockType.Air)
            {
                if (oldBlock.X + pad > newPos.X)
                {
                    newPos = new Vector3(oldBlock.X + pad, newPos.Y, newPos.Z);
                    velocity.X = 0;
                }
            }
            if (blocks.GetBlock(oldPos + new Vector3(0, -height, 1)).Type != BlockType.Air ||
                blocks.GetBlock(oldPos + new Vector3(0, -height + 1, 1)).Type != BlockType.Air ||
                (sneaking && blocks.GetBlock(oldPos + new Vector3(0, -height - 1, 1)).Type == BlockType.Air) ||
                blocks.GetBlock(new Vector3(oldPos.X, MathF.Ceiling(oldPos.Y - height + 1), oldPos.Z + 1)).Type != BlockType.Air)
            {
                if (oldBlock.Z + 1 - pad < newPos.Z)
                {
                    newPos = new Vector3(newPos.X, newPos.Y, oldBlock.Z + 1 - pad);
                    velocity.Z = 0;
                }
            }
            if (blocks.GetBlock(oldPos + new Vector3(0, -height, -1)).Type != BlockType.Air ||
                blocks.GetBlock(oldPos + new Vector3(0, -height + 1, -1)).Type != BlockType.Air ||
                (sneaking && blocks.GetBlock(oldPos + new Vector3(0, -height - 1, -1)).Type == BlockType.Air) ||
                blocks.GetBlock(new Vector3(oldPos.X, MathF.Ceiling(oldPos.Y - height + 1), oldPos.Z - 1)).Type != BlockType.Air)
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

            if (args.MouseState.ScrollDelta.Y < 0) {
                inventory.SelectedIndex = (inventory.SelectedIndex + 1) % 9;
            }
            if (args.MouseState.ScrollDelta.Y > 0)
            {
                inventory.SelectedIndex = (inventory.SelectedIndex + 8) % 9;
            }

            if (args.MouseState.IsButtonDown(MouseButton.Left) && lookAtSide.EuclideanLength > 0)
            {
                if (digProgress > 0)
                {
                    if (lookAt == digging)
                    {
                        digProgress += (float)args.DeltaTime;
                    }
                    else
                    {
                        digging = lookAt;
                        digProgress = (float)args.DeltaTime;
                    }
                }
                else
                {
                    digging = lookAt;
                    digProgress = (float)args.DeltaTime;
                }

                if (digProgress >= 1)
                {
                    BlockType type = blocks.GetBlock(digging).Type;
                    blocks.SetBlock(digging, new Block(BlockType.Air));
                    inventory.AddAfterDig(type);
                    var chunk = blocks.GetChunk(digging);
                    chunk.GenerateMesh();
                    var index = scene.Components.IndexOf(chunk.Component);
                    chunk.CreateComponent();
                    scene.Components[index] = chunk.Component;
                    digProgress = 0;
                }
            }
            else
            {
                digProgress = 0;
            }
            selector.Model.Meshes[0].Material = Manager.Material.Get($"selector_{Math.Floor(digProgress * 11)}");
        }

        private void UpdateLookAt(UpdateFrameEventArgs args) {
            const int reach = 5;

            var pos = scene.Camera.Position;
            var dir = scene.Camera.Front;
            var block = new Vector3i((int)MathF.Floor(pos.X), (int)MathF.Floor(pos.Y), (int)MathF.Floor(pos.Z));

            Dictionary<Vector3i, Tuple<double, Vector3i>> hit = new();

            for (int x = -reach; x <= reach; x++)
                for (int y = -reach; y <= reach; y++)
                    for (int z = -reach; z <= reach; z++)
                    {
                        Vector3i targetBlock = block - new Vector3i(x, y, z);
                        if (blocks.GetBlock(targetBlock).Type != BlockType.Air)
                        {
                            if (x > 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 1, y - 0, z - 0),
                                    block - new Vector3i(x - 1, y - 0, z - 1),
                                    block - new Vector3i(x - 1, y - 1, z - 1),
                                    block - new Vector3i(x - 1, y - 1, z - 0));

                                if (dist > 0)
                                    hit[targetBlock] = ((!hit.ContainsKey(targetBlock)) || dist < hit[targetBlock].Item1)
                                    ? new(dist, Vector3i.UnitX)
                                    : hit[targetBlock];
                            }
                            else if (x < 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 0, z - 0),
                                    block - new Vector3i(x - 0, y - 1, z - 0),
                                    block - new Vector3i(x - 0, y - 1, z - 1),
                                    block - new Vector3i(x - 0, y - 0, z - 1));

                                if (dist > 0)
                                    hit[targetBlock] = ((!hit.ContainsKey(targetBlock)) || dist < hit[targetBlock].Item1)
                                    ? new(dist, -Vector3i.UnitX)
                                    : hit[targetBlock];
                            }
                            if (y > 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 1, z - 0),
                                    block - new Vector3i(x - 1, y - 1, z - 0),
                                    block - new Vector3i(x - 1, y - 1, z - 1),
                                    block - new Vector3i(x - 0, y - 1, z - 1));

                                if (dist > 0)
                                    hit[targetBlock] = ((!hit.ContainsKey(targetBlock)) || dist < hit[targetBlock].Item1)
                                    ? new(dist, Vector3i.UnitY)
                                    : hit[targetBlock];
                            }
                            else if (y < 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 0, z - 0),
                                    block - new Vector3i(x - 0, y - 0, z - 1),
                                    block - new Vector3i(x - 1, y - 0, z - 1),
                                    block - new Vector3i(x - 1, y - 0, z - 0));

                                if (dist > 0)
                                    hit[targetBlock] = ((!hit.ContainsKey(targetBlock)) || dist < hit[targetBlock].Item1)
                                    ? new(dist, -Vector3i.UnitY)
                                    : hit[targetBlock];
                            }
                            if (z > 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 0, z - 1),
                                    block - new Vector3i(x - 0, y - 1, z - 1),
                                    block - new Vector3i(x - 1, y - 1, z - 1),
                                    block - new Vector3i(x - 1, y - 0, z - 1));

                                if (dist > 0)
                                    hit[targetBlock] = ((!hit.ContainsKey(targetBlock)) || dist < hit[targetBlock].Item1)
                                    ? new(dist, Vector3i.UnitZ)
                                    : hit[targetBlock];
                            }
                            else if (z < 0)
                            {
                                double dist = Tools.RayRectangleIntersect(pos, dir,
                                    block - new Vector3i(x - 0, y - 0, z - 0),
                                    block - new Vector3i(x - 1, y - 0, z - 0),
                                    block - new Vector3i(x - 1, y - 1, z - 0),
                                    block - new Vector3i(x - 0, y - 1, z - 0));

                                if (dist > 0)
                                    hit[targetBlock] = ((!hit.ContainsKey(targetBlock)) || dist < hit[targetBlock].Item1)
                                    ? new(dist, -Vector3i.UnitZ)
                                    : hit[targetBlock];
                            }
                        }
                    }

            if (hit.Count > 0)
            {
                var nearest = hit.OrderBy((h) => h.Value.Item1).First();
                if (Vector3.Distance(nearest.Key, pos) <= reach)
                {
                    lookAt = nearest.Key;
                    lookAtSide = nearest.Value.Item2;
                }
            }
            else
            {
                selector.Enabled = false;
                lookAtSide = new Vector3i(0);
            }

            if (lookAtSide.EuclideanLength > 0)
            {
                selector.Enabled = true;
                selector.Position = lookAt + new Vector3(0.5f);
            }
            else {
                selector.Enabled = false;
            }
        }
    }
}
