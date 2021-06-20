using GLRenderer.Components;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Managers
{
    public class TextureManager : AbstractManager<Texture>
    {
        public TextureManager() {
            data["_default"] = Texture.CreateShadowMap(new Vector2i(1));
        }

        public void CreateFromFile(string key, string path) {
            if (data.ContainsKey(key))
            {
                data[key].Dispose();
            }

            data[key] = Texture.LoadFromFile(path, false);
        }
    }
}
