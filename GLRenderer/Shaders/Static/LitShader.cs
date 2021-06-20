using GLRenderer.Components;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Shaders.Static
{
    public static class LitShader
    {
        private static Shader instance;
        public static Shader Instance
        {
            get => instance != null ? instance :
                instance = new Shader(
                    "Shaders/Source/shader.vert", "Shaders/Source/lit.frag",
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
                    (shader, lights, camera) =>
                    {
                        shader.SetVector3("viewPos", camera.Position);

                        // Lights
                        var directionalLights = lights.Where((l) => l is DirectionalLight);
                        if (directionalLights.Count() == 0)
                        {
                            DirectionalLight.None.Use(shader, -1, camera);
                        }
                        else
                        {
                            directionalLights.First().Use(shader, -1, camera);
                        }

                        var pointLights = lights.Where((l) => l is PointLight).Take(10);
                        int i = 0;
                        foreach (var light in pointLights)
                        {
                            light.Use(shader, i, camera);
                            i++;
                        }
                        if (i < 10)
                        {
                            shader.SetInt($"pointLights[{i}].use", 0);
                        }

                        // TODO: SpotLights
                        shader.SetInt("spotLights[0].use", 0);
                    },
                    (shader, material) =>
                    {
                        shader.SetVector3("material.ambientColor", material.AmbientColor);
                        shader.SetVector3("material.diffuseColor", material.DiffuseColor);
                        shader.SetVector3("material.specularColor", material.SpecularColor);

                        shader.SetFloat("material.shininess", material.Shininess);

                        //Textures
                        if (material.DiffuseMap != null)
                        {
                            material.DiffuseMap.Use(TextureUnit.Texture0);
                            shader.SetInt("material.diffuseTex", 0);
                            shader.SetInt("material.useDiffuseTex", 1);
                        }
                        else
                        {
                            shader.SetInt("material.useDiffuseTex", 0);
                        }

                        if (material.SpecularMap != null)
                        {
                            material.SpecularMap.Use(TextureUnit.Texture1);
                            shader.SetInt("material.specularTex", 1);
                            shader.SetInt("material.useSpecularTex", 1);
                        }
                        else
                        {
                            shader.SetInt("material.useSpecularTex", 0);
                        }
                    }
                );
        }
    }
}
