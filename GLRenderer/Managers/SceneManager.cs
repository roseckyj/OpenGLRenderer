using GLRenderer.Components;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Managers
{
    public class SceneManager : AbstractManager<Scene>
    {
        public SceneManager() {
            data["_default"] = new Scene();
        }

        public void Create(string key, Scene scene) {
            if (data.ContainsKey(key))
            {
                data[key].Dispose();
            }

            data[key] = scene;
        }
    }
}
