using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace GLRenderer.Components
{
    public class Camera: Component
    {
        private float fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        public float AspectRatio { get; set; }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                fov = MathHelper.DegreesToRadians(angle);
            }
        }

        private bool firstMove = true;
        private Vector2 lastPos;
        public float Yaw, Pitch;
        public void RotateWithMouse(MouseState mouseState, float sensitivity)
        {
            if (firstMove)
            {
                lastPos = new Vector2(mouseState.X, mouseState.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mouseState.X - lastPos.X;
                var deltaY = mouseState.Y - lastPos.Y;
                Yaw += MathHelper.DegreesToRadians(deltaX * sensitivity);
                Pitch += MathHelper.DegreesToRadians(deltaY * sensitivity);
                Pitch = MathHelper.Clamp(Pitch, -MathHelper.PiOver2 + 0.001f, MathHelper.PiOver2 - 0.001f);
                lastPos = new Vector2(mouseState.X, mouseState.Y);

                Rotation = Quaternion.FromEulerAngles(Pitch, Yaw, 0).Inverted();
            }
        }

        public override void Render(Component camera, Component playerPos, IEnumerable<Light> lights)
        {
            /*
            var shader = Shader.Lit;

            shader.Use();

            shader.SetMatrix4("view", camera.GetViewMatrix());
            shader.SetMatrix4("projection", camera.GetProjectionMatrix());
            shader.SetMatrix4("model", Matrix4.CreateScale(Vector3.One * 0.3f) * GetModelMatrix());
            shader.SetVector3("viewPos", camera.Position);

            DirectionalLight.None.Use(shader, 0);
            shader.SetInt("spotLights[0].use", 0);
            shader.SetInt("pointLights[0].use", 0);

            Model.Camera().Render(shader);
            */
        }

        public override Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(fov, AspectRatio, 0.01f, 1000f);
        }
    }
}
