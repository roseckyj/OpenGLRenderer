using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MeshBinder = System.Action<
    GLRenderer.Shaders.Shader>;
using LightBinder = System.Action<
    GLRenderer.Shaders.Shader,
    System.Collections.Generic.IEnumerable<GLRenderer.Components.Light>,
    GLRenderer.Components.Component>;
using MaterialBinder = System.Action<
    GLRenderer.Shaders.Shader,
    GLRenderer.Components.Material>;
using GLRenderer.Components;

namespace GLRenderer.Shaders
{
    public class Shader : IDisposable
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> uniformLocations;
        private readonly MeshBinder meshBinder;
        private readonly LightBinder lightBinder;
        private readonly MaterialBinder materialBinder;

        public Shader(string vertexPath, string fragmentPath, MeshBinder meshBinder, LightBinder lightBinder, MaterialBinder materialBinder)
        {
            this.meshBinder = meshBinder;
            this.lightBinder = lightBinder;
            this.materialBinder = materialBinder;

            // Load shader sources
            string VertexShaderSource;

            using (StreamReader reader = new StreamReader(vertexPath, Encoding.UTF8))
            {
                VertexShaderSource = reader.ReadToEnd();
            }

            string FragmentShaderSource;

            using (StreamReader reader = new StreamReader(fragmentPath, Encoding.UTF8))
            {
                FragmentShaderSource = reader.ReadToEnd();
            }


            // Create shaders
            int VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);

            int FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);


            // Compile shaders
            GL.CompileShader(VertexShader);

            string infoLogVert = GL.GetShaderInfoLog(VertexShader);
            if (infoLogVert != string.Empty)
                Console.WriteLine(infoLogVert);

            GL.CompileShader(FragmentShader);

            string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);

            if (infoLogFrag != string.Empty)
                Console.WriteLine(infoLogFrag);


            // Create a program
            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);


            // Cleanup
            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);


            // Get uniform locations
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            uniformLocations = new Dictionary<string, int>();

            // Loop over all the uniforms,
            for (var i = 0; i < numberOfUniforms; i++)
            {
                // get the name of this uniform,
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                // get the location,
                var location = GL.GetUniformLocation(Handle, key);

                // and then add it to the dictionary.
                uniformLocations.Add(key, location);
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void BindMesh()
        {
            Use();
            meshBinder(this);
        }

        public void BindLights(IEnumerable<Light> lights, Component camera)
        {
            Use();
            lightBinder(this, lights, camera);
        }

        public void BindMaterial(Material material)
        {
            Use();
            materialBinder(this, material);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        #region Uniform setters

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(uniformLocations[name], data);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(uniformLocations[name], true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(uniformLocations[name], data);
        }

        public void SetVector2(string name, Vector2 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform2(uniformLocations[name], data);
        }

        #endregion

        #region Disposing

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }

        #endregion
    }
}
