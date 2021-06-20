using GLRenderer.Components;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Shaders.Static
{
    public static class UnlitShader
    {
        private static Shader instance;
        public static Shader Instance
        {
            get => instance != null ? instance :
                instance = new Shader(
                    "Shaders/Source/shader.vert", "Shaders/Source/unlit.frag",
                    (shader) =>
                    {
                        int aPosition = shader.GetAttribLocation("aPosition");
                        GL.EnableVertexAttribArray(aPosition);
                        GL.VertexAttribPointer(aPosition, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

                        int aNormal = shader.GetAttribLocation("aNormal");
                        GL.EnableVertexAttribArray(aNormal);
                        GL.VertexAttribPointer(aNormal, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));

                        int aTexCoord = shader.GetAttribLocation("aTexCoord");
                        GL.EnableVertexAttribArray(aTexCoord);
                        GL.VertexAttribPointer(aTexCoord, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
                    },
                    (shader, lights, camera) => { },
                    (shader, material) =>
                    {
                        shader.SetVector3("material.color", material.DiffuseColor);

                        //Textures
                        if (material.DiffuseMap != null)
                        {
                            material.DiffuseMap.Use(TextureUnit.Texture0);
                            shader.SetInt("material.tex", 0);
                            shader.SetInt("material.useTex", 1);
                        }
                        else
                        {
                            shader.SetInt("material.useTex", 0);
                        }
                    }
                );
        }
    }
}
