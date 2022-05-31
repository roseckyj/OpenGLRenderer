using GLRenderer.Shaders;
using GLRenderer.Shaders.Static;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    public class DirectionalLight : Light
    {

        public DirectionalLight(Quaternion orientation, Vector3 diffuseColor, Vector3 specularColor): base() {
            Rotation = orientation;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
        }

        private static DirectionalLight defaultLight;
        public static DirectionalLight Default
        {
            get =>
                defaultLight == null ?
                defaultLight = new DirectionalLight(
                    Quaternion.FromEulerAngles(new Vector3(-0.8f, -1f, -0.2f)),
                    Vector3.One,
                    Vector3.One) :
                defaultLight;
        }

        private static DirectionalLight noneLight;
        public static DirectionalLight None
        {
            get =>
                noneLight == null ?
                noneLight = new DirectionalLight(
                Quaternion.Identity,
                Vector3.Zero,
                Vector3.Zero):
                noneLight;
        }

        public override void Use(Shader shader, int index, Component camera)
        {
            shader.SetVector3("dirLight.direction", -Front);
            shader.SetVector3("dirLight.ambient", AmbientColor);
            shader.SetVector3("dirLight.diffuse", DiffuseColor);
            shader.SetVector3("dirLight.specular", SpecularColor);
            shader.SetMatrix4("dirLight.matrix", GetViewMatrix(camera) * GetProjectionMatrix());
            shader.SetInt("dirLight.shadowMap", 3 + index);

            base.Use(shader, index, camera);
        }

        public override void Render(Component camera, Component playerPos, IEnumerable<Light> lights)
        {
            // TODO: use unlit
            var shader = LitShader.Instance;

            shader.Use();

            shader.SetMatrix4("view", camera.GetViewMatrix(camera));
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("model", Matrix4.CreateScale(new Vector3(20f)) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(-Front * 150f) * Matrix4.CreateTranslation(playerPos.Position));
            shader.SetVector3("viewPos", camera.Position);

            DirectionalLight.None.Use(shader, 0, camera);
            shader.SetInt("spotLights[0].use", 0);
            shader.SetInt("pointLights[0].use", 0);

            Model.Camera(new Material(Vector3.One * 10)).Render(shader);
        }

        public override Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreateOrthographic(100f, 100f,0.1f, 80f);
        }

        public override Matrix4 GetViewMatrix(Component playerPos)
        {
            return (Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(-Front * 40f) * Matrix4.CreateTranslation(playerPos.Position)).Inverted();
        }
    }
}
