using GLRenderer.Shaders;
using GLRenderer.Shaders.Static;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    abstract public class Light : Component
    {
        public Vector3 AmbientColor { get; set; } = new Vector3(0.1f, 0.1f, 0.1f);
        public Vector3 DiffuseColor { get; set; }
        public Vector3 SpecularColor { get; set; }

        // Shadow map
        private Vector2i shadowSize;
        private Texture depthMap;
        int depthMapFBO;

        public Light() {
            shadowSize = new Vector2i(2048);
            depthMap = Texture.CreateShadowMap(shadowSize);
            depthMapFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap.Handle, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public virtual void Use(Shader shader, int index, Component camera) {
            depthMap.Use(TextureUnit.Texture3 + index);
        }

        public void RenderShadows(Component playerPos, IEnumerable<Solid> solids)
        {
            GL.Viewport(0, 0, shadowSize.X, shadowSize.Y);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);
            foreach (Solid s in solids)
            {
                s.Render(this, playerPos, Enumerable.Empty<Light>(), DepthShader.Instance);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
