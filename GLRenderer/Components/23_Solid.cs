using GLRenderer.Shaders;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    public class Solid : Component
    {
        public Model Model { get; }
        public Shader Shader { get; }

        public bool CastShadow = true;
        public bool Visible = true;

        public Solid(Model model, Shader shader): base()
        {
            Model = model;
            Shader = shader;
        }

        public override void Render(Component camera, Component playerPos, IEnumerable<Light> lights) {
            Render(camera, playerPos, lights, Shader);
        }

        public void Render(Component camera, Component playerPos, IEnumerable<Light> lights, Shader shader)
        {
            if (!((camera is Camera && Visible) || (camera is Light && CastShadow))) return;
            
            shader.Use();

            shader.SetMatrix4("view", camera.GetViewMatrix(playerPos));
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("model", GetModelMatrix());

            shader.BindLights(lights, camera);
            Model.Render(shader);
        }

        public new void Dispose()
        {
            Model.Dispose();
            base.Dispose();
        }
    }
}
