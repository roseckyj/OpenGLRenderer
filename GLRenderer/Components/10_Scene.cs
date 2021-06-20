using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    public class Scene : IDisposable
    {
        public List<Component> Components = new();

        public Camera Camera { get => Components.Find((c) => (c is Camera) && c.Enabled) as Camera; }

        public void Render(double deltaTime, bool renderInvisibleObjects)
        {
            if (Camera == null) return;

            var solids = Components.Where((c) => (c is Solid) && c.Enabled).Select((c) => c as Solid);
            var lights = Components.Where((c) => (c is Light) && c.Enabled).Select((c) => c as Light);
            var guis = Components.Where((c) => (c is GUI) && c.Enabled).Select((c) => c as GUI);
            foreach (Light l in lights)
            {
                if(renderInvisibleObjects) l.Render(Camera, Camera, lights);
            }
            foreach (Solid s in solids) {
                s.Render(Camera, Camera, lights);
            }
            foreach (GUI g in guis)
            {
                g.Render(Camera, Camera, lights);
            }

            if (renderInvisibleObjects) Camera.Render(Camera, Camera, lights);
        }

        public void RenderShadows()
        {
            var solids = Components.Where((c) => (c is Solid)).Select((c) => c as Solid);
            var lights = Components.Where((c) => (c is Light)).Select((c) => c as Light);
            foreach (Light l in lights)
            {
                l.RenderShadows(Camera, solids);
            }
        }

        public void Dispose()
        {
            Components.ForEach((c) => c.Dispose());
        }
    }
}
