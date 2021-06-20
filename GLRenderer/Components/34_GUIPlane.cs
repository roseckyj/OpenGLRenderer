using GLRenderer.Shaders;
using GLRenderer.Shaders.Static;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    public class GUIPlane : GUI
    {
        private Model model;

        private Vector2 size;

        public GUIPlane(Vector2 position, Vector2 size, Material mat, Vector2 texelTopLeft, Vector2 texelBottomRight): base()
        {
            Position = new Vector3(position.X, position.Y, 0);
            this.size = size;
            model = new Model(new Mesh(new Vertex[] {
                //         Position                       Normal                          Texture
                new Vertex(new Vector3(-1f, -1f, 0f), new Vector3(0f, 0f, -1f), new Vector2(texelTopLeft.X, texelBottomRight.Y)),
                new Vertex(new Vector3( 1f, -1f, 0f), new Vector3(0f, 0f, -1f), new Vector2(texelBottomRight.X, texelBottomRight.Y)),
                new Vertex(new Vector3( 1f,  1f, 0f), new Vector3(0f, 0f, -1f), new Vector2(texelBottomRight.X, texelTopLeft.Y)),

                new Vertex(new Vector3( 1f,  1f, 0f), new Vector3(0f, 0f, -1f), new Vector2(texelBottomRight.X, texelTopLeft.Y)),
                new Vertex(new Vector3(-1f,  1f, 0f), new Vector3(0f, 0f, -1f), new Vector2(texelTopLeft.X, texelTopLeft.Y)),
                new Vertex(new Vector3(-1f, -1f, 0f), new Vector3(0f, 0f, -1f), new Vector2(texelTopLeft.X, texelBottomRight.Y))
            }, mat));
        }

        public override void Render(Component camera, Component playerPos, IEnumerable<Light> lights) {
            Shader shader = UnlitShader.Instance;
            Camera cam = camera as Camera;
            float aspect = cam.AspectRatio;
            Scale = new Vector3(size.X, size.Y * aspect, 0);

            shader.Use();

            shader.SetMatrix4("view", Matrix4.Identity);
            shader.SetMatrix4("projection", Matrix4.Identity);
            shader.SetMatrix4("model", GetModelMatrix());

            model.Render(shader);
        }

        public new void Dispose()
        {
            model.Dispose();
            base.Dispose();
        }
    }
}
