using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace GLRenderer.Components
{
    abstract public class Component : IDisposable
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Rotation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        public bool Enabled = true;

        private Guid Id;

        public Component() {
            Id = Guid.NewGuid();
        }

        public virtual Matrix4 GetModelMatrix() {
            return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
        }

        public virtual Matrix4 GetViewMatrix(Component playerPos)
        {
            return (Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position)).Inverted();
        }

        public virtual Matrix4 GetProjectionMatrix()
        {
            return Matrix4.Identity;
        }

        public abstract void Render(Component camera, Component playerPos, IEnumerable<Light> lights);

        #region Vector helpers (Front, Up, Right)
        public Vector3 Front => ApplyRotationTo(-Vector3.UnitZ);

        public Vector3 Up => ApplyRotationTo(Vector3.UnitY);

        public Vector3 Right => ApplyRotationTo(Vector3.UnitX);

        private Vector3 ApplyRotationTo(Vector3 vector)
        {
            return Matrix3.CreateFromQuaternion(Rotation.Inverted()) * vector;
        }
        #endregion

        #region IDisposable

        public void Dispose()
        {
        }

        #endregion

        #region Operator overrides

        public static bool operator ==(Component a, Component b) {
            if (object.ReferenceEquals(a, null))
            {
                return object.ReferenceEquals(b, null);
            }

            return a.Equals(b);
        }

        public static bool operator !=(Component a, Component b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Component b = (Component)obj;
                return Id == b.Id;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion
    }
}