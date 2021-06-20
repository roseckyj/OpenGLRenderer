using GLRenderer.Components;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Shaders.Static
{
    public static class DepthShader
    {
        private static Shader instance;
        public static Shader Instance
        {
            get => instance != null ? instance :
                instance = new Shader(
                    "Shaders/Source/simple.vert", "Shaders/Source/none.frag",
                    (shader) =>
                    {
                        int aPosition = shader.GetAttribLocation("aPosition");
                        GL.EnableVertexAttribArray(aPosition);
                        GL.VertexAttribPointer(aPosition, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                    },
                    (shader, lights, camera) => { },
                    (shader, material) => { }
                );
        }
    }
}
