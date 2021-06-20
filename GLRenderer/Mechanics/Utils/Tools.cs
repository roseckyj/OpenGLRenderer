using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Mechanics.Utils
{
    public static class Tools
    {
        /// <summary>
        /// Check for intersection of a ray and a rectangle and returns the distance (or -1 if no intersection found)
        /// 
        /// Adapted from
        /// https://www.scratchapixel.com/lessons/3d-basic-rendering/ray-tracing-rendering-a-triangle/ray-triangle-intersection-geometric-solution
        /// https://courses.cs.washington.edu/courses/csep557/10au/lectures/triangle_intersection.pdf
        /// </summary>
        public static double RayRectangleIntersect(
            Vector3d orig, Vector3d dir,
            Vector3d v0, Vector3d v1, Vector3d v2, Vector3d v3)
        {
            // compute plane's normal
            Vector3d v0v1 = v1 - v0;
            Vector3d v0v2 = v2 - v0;
            // no need to normalize
            Vector3d N = Vector3d.Cross(v0v1, v0v2); // N 

            // Step 1: finding P

            // check if ray and plane are parallel ?
            double NdotRayDirection = Vector3d.Dot(N, dir);
            if (Math.Abs(NdotRayDirection) < 0.001) // almost 0 
                return -1; // they are parallel so they don't intersect ! 

            // compute d parameter using equation 2
            double d = Vector3d.Dot(N, v0);

            // compute t (equation 3)
            double t = (d - Vector3d.Dot(N, orig)) / NdotRayDirection;
            // check if the triangle is in behind the ray
            if (t < 0) return -1; // the triangle is behind 

            // compute the intersection point using equation 1
            Vector3d P = orig + t * dir;

            // Step 2: inside-outside test
            Vector3d C; // vector perpendicular to triangle's plane 
            
            // edge 0
            Vector3d edge0 = v1 - v0;
            Vector3d vp0 = P - v0;
            C = Vector3d.Cross(edge0, vp0);
            if (Vector3d.Dot(C, N) < 0) return -1; // P is on the right side 

            // edge 1
            Vector3d edge1 = v2 - v1;
            Vector3d vp1 = P - v1;
            C = Vector3d.Cross(edge1, vp1);
            if (Vector3d.Dot(C, N) < 0) return -1; // P is on the right side 

            // edge 2
            Vector3d edge2 = v3 - v2;
            Vector3d vp2 = P - v2;
            C = Vector3d.Cross(edge2, vp2);
            if (Vector3d.Dot(C, N) < 0) return -1; // P is on the right side; 

            // edge 3
            Vector3d edge3 = v0 - v3;
            Vector3d vp3 = P - v3;
            C = Vector3d.Cross(edge3, vp3);
            if (Vector3d.Dot(C, N) < 0) return -1; // P is on the right side; 
            
            return t; // this ray hits the triangle 
        }
    }
}
