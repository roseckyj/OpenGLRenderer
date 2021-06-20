using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    public class Vertex
    {
        public Vector3 Position { get; }
        public Vector3 Normal { get; }
        public Vector2 TextureCoord { get; } = Vector2.Zero;


        public Vertex(Vector3 position, Vector3 normal)
        {
            Position = position;
            normal.Normalize();
            Normal = normal;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 textureCoord)
        {
            Position = position;
            Normal = normal;
            TextureCoord = textureCoord;
        }

        public static float[] ToArray(IEnumerable<Vertex> vertices) {
            int count = vertices.Count();
            float[] result = new float[count * 8];
            for (int i = 0; i < count; i++) {
                Vertex v = vertices.ElementAt(i);
                result[i * 8 + 0] = v.Position.X;
                result[i * 8 + 1] = v.Position.Y;
                result[i * 8 + 2] = v.Position.Z;

                result[i * 8 + 3] = v.Normal.X;
                result[i * 8 + 4] = v.Normal.Y;
                result[i * 8 + 5] = v.Normal.Z;

                result[i * 8 + 6] = v.TextureCoord.X;
                result[i * 8 + 7] = v.TextureCoord.Y;
            }
            return result;
        }
    }
}