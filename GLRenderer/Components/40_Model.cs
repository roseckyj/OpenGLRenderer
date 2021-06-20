using GLRenderer.Shaders;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GLRenderer.Components
{
    public class Model : IDisposable
    {
        private Mesh[] meshes;

        public Model(IEnumerable<Mesh> meshes) {
            this.meshes = meshes.ToArray();
        }

        public Model(Mesh mesh)
        {
            meshes = new Mesh[] { mesh };
        }

        public void Render(Shader shader)
        {
            foreach (Mesh m in meshes) {
                m.Render(shader);
            }
        }

        public void Dispose()
        {
            foreach (Mesh m in meshes) {
                m.Dispose();
            }
        }

        public void ReplaceWith(Model newModel) {
            meshes = newModel.meshes;
        }

        public Model Copy()
        {
            Model copy = new Model(this.meshes);
            return copy;
        }

        public Model Inverted() {
            return new Model(meshes.Select((m) => m.Inverted()));
        }

        #region Load .obj

        public static Model FromObjFile(string path)
        {
            List<Vector3> geometric = new();
            List<Vector3> normals = new();
            List<Vector2> textureCoords = new();
            Dictionary<string, Material> materials = new();
            Material material = Material.Default;

            List<Vertex> vertices = new();
            List<uint> indices = new();
            Dictionary<string, uint> vertexIndices = new();

            List<Mesh> meshes = new();

            using (StreamReader file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(' ')) continue;

                    string[] split = line.Split(' ');

                    switch (split[0]) {
                        case "v":
                            geometric.Add(ArrayToVector3(split, 1));
                            break;
                        case "vn":
                            normals.Add(ArrayToVector3(split, 1));
                            break;
                        case "vt":
                            textureCoords.Add(ArrayToVector2(split, 1));
                            break;
                        case "f":
                            for (int i = 1; i < split.Length; i++) {
                                if (!vertexIndices.ContainsKey(split[i]))
                                {
                                    string[] vertPos = split[i].Split('/');
                                    Vector3 geo = Vector3.Zero;
                                    Vector3 norm = Vector3.Zero;
                                    Vector2 tex = Vector2.Zero;
                                    
                                    if (vertPos.Length > 0 && vertPos[0].Length > 0)
                                        geo = geometric[int.Parse(vertPos[0]) - 1];

                                    if (vertPos.Length > 1 && vertPos[1].Length > 0)
                                        tex = textureCoords[int.Parse(vertPos[1]) - 1];

                                    if (vertPos.Length > 2 && vertPos[2].Length > 0)
                                        norm = normals[int.Parse(vertPos[2]) - 1];


                                    vertices.Add(new Vertex(geo, norm, tex));
                                    vertexIndices.Add(split[i], (uint)vertices.Count - 1);
                                }
                            }
                            if (split.Length == 4) {
                                indices.Add(vertexIndices[split[1]]);
                                indices.Add(vertexIndices[split[2]]);
                                indices.Add(vertexIndices[split[3]]);
                            } else if (split.Length == 5)
                            {
                                indices.Add(vertexIndices[split[1]]);
                                indices.Add(vertexIndices[split[2]]);
                                indices.Add(vertexIndices[split[3]]);
                                indices.Add(vertexIndices[split[3]]);
                                indices.Add(vertexIndices[split[4]]);
                                indices.Add(vertexIndices[split[1]]);
                            }
                            break;
                        case "usemtl":
                            if (indices.Count > 0) {
                                meshes.Add(new Mesh(vertices, indices, material));
                                vertices = new();
                                indices = new();
                                vertexIndices = new();
                            }
                            material = materials[split[1]];
                            break;
                        case "mtllib":
                            materials = LoadMtlFile(Path.GetDirectoryName(path) + "/" + split[1]);
                            break;

                        /* TODO: "g" */
                    }
                }
            }
            if (indices.Count > 0)
            {
                meshes.Add(new Mesh(vertices, indices, material));
            }

            Model model = new Model(meshes);
            return model;
        }
        public static Model FromObjFile(string path, Material material) {
            Model model = FromObjFile(path);
            foreach (Mesh m in model.meshes) {
                m.Material = material;
            }
            return model;
        }

        private static Vector3 ArrayToVector3(string[] arr, int offset) {
            return new Vector3(
                float.Parse(arr[offset + 0], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(arr[offset + 1], NumberStyles.Any, CultureInfo.InvariantCulture),
                float.Parse(arr[offset + 2], NumberStyles.Any, CultureInfo.InvariantCulture));
        }

        private static Vector2 ArrayToVector2(string[] arr, int offset)
        {
            return new Vector2(
                float.Parse(arr[offset + 0], NumberStyles.Any, CultureInfo.InvariantCulture),
                1 - float.Parse(arr[offset + 1], NumberStyles.Any, CultureInfo.InvariantCulture));
        }

        private static Dictionary<string, Material> LoadMtlFile(string path) {
            string currentMaterialName = null;
            Material currentMaterial = null;
            Dictionary<string, Material> materials = new();
            bool ambientSet = false;

            using (StreamReader file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length == 0 || line.StartsWith('#') || line.StartsWith(' ')) continue;

                    string[] split = line.Split(' ');

                    switch (split[0])
                    {
                        case "newmtl":
                            if (currentMaterial != null)
                            {
                                materials.Add(currentMaterialName, currentMaterial);
                            }
                            currentMaterialName = split[1];
                            currentMaterial = new Material();
                            ambientSet = false;
                            break;

                        case "Ks":
                            currentMaterial.SpecularColor = ArrayToVector3(split, 1);
                            break;

                        case "Ka":
                            currentMaterial.AmbientColor = ArrayToVector3(split, 1);
                            ambientSet = true;
                            break;

                        case "Kd":
                            currentMaterial.DiffuseColor = ArrayToVector3(split, 1);
                            if (!ambientSet)
                            {
                                currentMaterial.AmbientColor = ArrayToVector3(split, 1);
                            }
                            break;

                        case "Ns":
                            currentMaterial.Shininess = float.Parse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;

                        case "map_Ka":
                            currentMaterial.DiffuseMap = Texture.LoadFromFile(Path.GetDirectoryName(path) + "/" + split[1], true);
                            currentMaterial.AmbientColor = Vector3.One;
                            currentMaterial.DiffuseColor = Vector3.One;
                            break;

                        case "map_Kd":
                            currentMaterial.DiffuseMap = Texture.LoadFromFile(Path.GetDirectoryName(path) + "/" + split[1], true);
                            currentMaterial.AmbientColor = Vector3.One;
                            currentMaterial.DiffuseColor = Vector3.One;
                            break;

                        case "map_Ks":
                            currentMaterial.SpecularMap = Texture.LoadFromFile(Path.GetDirectoryName(path) + "/" + split[1], true);
                            currentMaterial.SpecularColor = Vector3.One;
                            break;
                    }
                }
            }
            if (currentMaterial != null)
            {
                materials.Add(currentMaterialName, currentMaterial);
            }
            return materials;
        }

        #endregion

        #region Primitives

        public static Model Plane() { return Plane(Material.Default); }
        public static Model Plane(Material material)
        {
            Mesh mesh = new Mesh(new Vertex[] {
                //         Position                       Normal                          Texture
                new Vertex(new Vector3( 0.5f, 0f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(-0.5f, 0f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3( 0.5f, 0f,  0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, 0f,  0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3( 0.5f, 0f,  0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, 0f, -0.5f), new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.0f, 1.0f))
            });

            mesh.Material = material;

            return new Model(mesh);
        }

        public static Model Cube() { return Cube(Material.Default); }
        public static Model Cube(Material material)
        {
            float texSize = 1f / 4;

            Mesh mesh = new Mesh(new Vertex[] {
                //         Position                          Normal                  Texture
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0,  1,  0), new Vector2(texSize * (0 + 1), texSize * (1 + 1))), // Top face
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0,  1,  0), new Vector2(texSize * (1 + 1), texSize * (1 + 1))),
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0,  1,  0), new Vector2(texSize * (1 + 1), texSize * (0 + 1))),
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0,  1,  0), new Vector2(texSize * (0 + 1), texSize * (0 + 1))),
                
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0, -1,  0), new Vector2(texSize * (0 + 1), texSize * (0 + 3))), // Bottom face
                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0, -1,  0), new Vector2(texSize * (1 + 1), texSize * (0 + 3))),
                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0, -1,  0), new Vector2(texSize * (1 + 1), texSize * (1 + 3))),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0, -1,  0), new Vector2(texSize * (0 + 1), texSize * (1 + 3))),
                
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(1,  0,  0), new Vector2(texSize * (1 + 0), texSize * (1 + 2))), // Left face
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(1,  0,  0), new Vector2(texSize * (1 + 0), texSize * (0 + 2))),
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(1,  0,  0), new Vector2(texSize * (0 + 0), texSize * (0 + 2))),
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(1,  0,  0), new Vector2(texSize * (0 + 0), texSize * (1 + 2))),

                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(1,  0,  0), new Vector2(texSize * (1 + 2), texSize * (1 + 2))), // Right face
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(1,  0,  0), new Vector2(texSize * (1 + 2), texSize * (0 + 2))),
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(1,  0,  0), new Vector2(texSize * (0 + 2), texSize * (0 + 2))),
                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(1,  0,  0), new Vector2(texSize * (0 + 2), texSize * (1 + 2))),

                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0,  0, -1), new Vector2(texSize * (0 + 1), texSize * (1 + 2))), // Front face
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0,  0, -1), new Vector2(texSize * (0 + 1), texSize * (0 + 2))),
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0,  0, -1), new Vector2(texSize * (1 + 1), texSize * (0 + 2))),
                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0,  0, -1), new Vector2(texSize * (1 + 1), texSize * (1 + 2))),

                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0,  0,  1), new Vector2(texSize * (0 + 3), texSize * (1 + 2))), // Back face
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0,  0,  1), new Vector2(texSize * (0 + 3), texSize * (0 + 2))),
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0,  0,  1), new Vector2(texSize * (1 + 3), texSize * (0 + 2))),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0,  0,  1), new Vector2(texSize * (1 + 3), texSize * (1 + 2)))
            }, new uint[] {
                0, 1, 2, 2, 3, 0,
                4, 5, 6, 6, 7, 4,
                8, 9, 10, 10, 11, 8,
                12, 13, 14, 14, 15, 12,
                16, 17, 18, 18, 19, 16,
                20, 21, 22, 22, 23, 20
            });

            mesh.Material = material;

            return new Model(mesh);
        }

        public static Model Camera() { return Camera(Material.Default); }
        public static Model Camera(Material material)
        {
            Mesh mesh = new Mesh(new Vertex[] {
                //         Position                          Normal                           Texture
                new Vertex(new Vector3(-0.5f, -0.5f,  0.0f), new Vector3( 0.0f,  0.0f, 1.0f), new Vector2(0.0f, 0.0f)), // Front face
                new Vertex(new Vector3(-0.5f,  0.5f,  0.0f), new Vector3( 0.0f,  0.0f, 1.0f), new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f,  0.0f), new Vector3( 0.0f,  0.0f, 1.0f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f,  0.0f), new Vector3( 0.0f,  0.0f, 1.0f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3( 0.5f, -0.5f,  0.0f), new Vector3( 0.0f,  0.0f, 1.0f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.0f), new Vector3( 0.0f,  0.0f, 1.0f), new Vector2(0.0f, 0.0f)),

                new Vertex(new Vector3(-0.5f,  0.5f,  0.0f), new Vector3(-1.0f,  0.0f,  0.5f), new Vector2(1.0f, 0.0f)), // Left face
                new Vertex(new Vector3(-0.5f, -0.5f,  0.0f), new Vector3(-1.0f,  0.0f,  0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3( 0.0f,  0.0f,  1.0f), new Vector3(-1.0f,  0.0f,  0.5f), new Vector2(0.0f, 1.0f)),

                new Vertex(new Vector3( 0.5f, -0.5f,  0.0f), new Vector3( 1.0f,  0.0f,  0.5f), new Vector2(1.0f, 1.0f)), // Right face
                new Vertex(new Vector3( 0.5f,  0.5f,  0.0f), new Vector3( 1.0f,  0.0f,  0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3( 0.0f,  0.0f,  1.0f), new Vector3( 1.0f,  0.0f,  0.5f), new Vector2(0.0f, 1.0f)),

                new Vertex(new Vector3( 0.5f,  0.5f,  0.0f), new Vector3( 0.0f,  1.0f,  0.5f), new Vector2(1.0f, 1.0f)), // Top face
                new Vertex(new Vector3(-0.5f,  0.5f,  0.0f), new Vector3( 0.0f,  1.0f,  0.5f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3( 0.0f,  0.0f,  1.0f), new Vector3( 0.0f,  1.0f,  0.5f), new Vector2(0.0f, 1.0f)),

                new Vertex(new Vector3(-0.5f, -0.5f,  0.0f), new Vector3( 0.0f, -1.0f,  0.5f), new Vector2(1.0f, 0.0f)), // Bottom face
                new Vertex(new Vector3( 0.5f, -0.5f,  0.0f), new Vector3( 0.0f, -1.0f,  0.5f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3( 0.0f,  0.0f,  1.0f), new Vector3( 0.0f, -1.0f,  0.5f), new Vector2(0.0f, 1.0f)),
            });

            mesh.Material = material;

            return new Model(mesh);
        }

        #endregion
    }
}