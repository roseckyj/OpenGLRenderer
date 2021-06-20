using GLRenderer.Shaders;
using GLRenderer.Shaders.Static;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    public class PointLight : Light
    {
        public float Constant { get; set; } = 1.0f;
        public float Linear { get; set; } = 0.09f;
        public float Quadratic { get; set; } = 0.032f;

        public PointLight(Vector3 position, Vector3 diffuseColor, Vector3 specularColor): base() {
            Position = position;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;
        }

        public override void Use(Shader shader, int index, Component camera)
        {
            shader.SetVector3($"pointLights[{index}].position", Position);
            shader.SetVector3($"pointLights[{index}].ambient", AmbientColor);
            shader.SetVector3($"pointLights[{index}].diffuse", DiffuseColor);
            shader.SetVector3($"pointLights[{index}].specular", SpecularColor);
            shader.SetFloat($"pointLights[{index}].constant", Constant);
            shader.SetFloat($"pointLights[{index}].linear", Linear);
            shader.SetFloat($"pointLights[{index}].quadratic", Quadratic);
            shader.SetMatrix4($"pointLights[{index}].matrix", GetViewMatrix(camera) * GetProjectionMatrix());
            shader.SetInt($"pointLights[{index}].shadowMap", 3 + index);
            shader.SetInt($"pointLights[{index}].use", 1);

            base.Use(shader, index, camera);
        }

        public override void Render(Component camera, Component playerPos, IEnumerable<Light> lights)
        {
            // TODO: use unlit
            var shader = LitShader.Instance;

            shader.Use();

            shader.SetMatrix4("view", camera.GetViewMatrix(camera));
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("model", Matrix4.CreateScale(Vector3.One * 0.15f) * GetModelMatrix());
            shader.SetVector3("viewPos", camera.Position);

            DirectionalLight.None.Use(shader, 0, camera);
            shader.SetInt("spotLights[0].use", 0);
            shader.SetInt("pointLights[0].use", 0);

            Model.Camera(new Material(Vector3.One * 10)).Render(shader);
        }

        public override Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90f), 1f, 0.1f, 50f);
        }

        public override Matrix4 GetViewMatrix(Component playerPos)
        {
            return (Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(new Vector3(0, 30, 0))).Inverted();
        }
    }
}
