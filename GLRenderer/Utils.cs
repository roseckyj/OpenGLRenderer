using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer
{
    static class Utils
    {
        public static Quaternion LookAt(Vector3 sourcePoint, Vector3 destPoint)
        {
            Vector3 v = destPoint - sourcePoint;
            float r = (float)MathHelper.Sqrt(v.X * v.X + v.Y * v.Y); // I'm not suppose to take Z into account?
            float yaw = (float)MathHelper.Atan2(v.Y, v.X);
            float pitch = (float)MathHelper.Atan2(v.Z, r);
            return Quaternion.FromEulerAngles(pitch, yaw, 0.0f);
        }
    }
}
