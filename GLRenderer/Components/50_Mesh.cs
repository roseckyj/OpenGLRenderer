using GLRenderer.Shaders;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    public class Mesh : IDisposable
    {
        private Vertex[] vertices;
        private uint[] indices;
        private List<Shader> initializedShaders = new();

        public Material Material { get; set; } = Material.Default;

        private int VertexBufferObject;
        private int VertexArrayObject;
        private int ElementBufferObject;        


        public Mesh(IEnumerable<Vertex> vertices) {
            this.vertices = vertices.ToArray();
            this.indices = null;
        }

        public Mesh(IEnumerable<Vertex> vertices, IEnumerable<uint> indices)
        {
            this.vertices = vertices.ToArray();
            this.indices = indices.ToArray();
        }

        public Mesh(IEnumerable<Vertex> vertices, Material material)
        {
            this.vertices = vertices.ToArray();
            this.indices = null;
            Material = material;
        }

        public Mesh(IEnumerable<Vertex> vertices, IEnumerable<uint> indices, Material material)
        {
            this.vertices = vertices.ToArray();
            this.indices = indices.ToArray();
            Material = material;
        }

        public Mesh Inverted() {
            if (indices != null)
            {
                return new Mesh(vertices, indices.Reverse(), Material);
            }
            else
            {
                return new Mesh(vertices.Reverse(), Material);
            }
        }

        private void Init(Shader shader) {
            VertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(VertexArrayObject);

            float[] vertArray = Vertex.ToArray(vertices);

            VertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertArray.Length * sizeof(float), vertArray, BufferUsageHint.StaticDraw);

            if (indices != null)
            {
                ElementBufferObject = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
            }

            shader.BindMesh();
            initializedShaders.Add(shader);
        }

        public void Render(Shader shader)
        {
            shader.Use();
            if (!initializedShaders.Contains(shader)) {
                Init(shader);
            }
            GL.BindVertexArray(VertexArrayObject);

            Material.Use(shader);

            if (indices != null)
            {
                GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            else
            {
                GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);
            }
        }

        public void Dispose()
        {
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteBuffer(ElementBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);
        }
    }
}