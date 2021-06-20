using GLRenderer.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;

namespace GLRenderer.Components
{
    public class Material: IDisposable
    {
        public Vector3 AmbientColor { get; set; } = Vector3.One * 0.8f;
        public Vector3 DiffuseColor { get; set; } = Vector3.One * 0.8f;
        public Vector3 SpecularColor { get; set; } = Vector3.One * 0.5f;

        public Texture DiffuseMap { get; set; } = null;
        public Texture SpecularMap { get; set; } = null;

        public float Shininess { get; set; } = 10f;


        public Material() {
        }

        public Material(Vector3 mainColor)
        {
            AmbientColor = mainColor;
            DiffuseColor = mainColor;
        }

        public Material(Texture mainTexture)
        {
            AmbientColor = Vector3.One;
            DiffuseColor = Vector3.One;
            DiffuseMap = mainTexture;
        }

        private static Material defaultMaterial;
        public static Material Default { get =>
                defaultMaterial == null ?
                defaultMaterial = new Material() :
                defaultMaterial; }

        public void Use(Shader shader) {
            shader.BindMaterial(this);
        }

        public void Dispose()
        {
            if (DiffuseMap != null) DiffuseMap.Dispose();
            if (SpecularMap != null) SpecularMap.Dispose();
        }
    }
}