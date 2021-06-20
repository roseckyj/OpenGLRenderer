using GLRenderer.Components;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Managers
{
    public class MaterialManager : AbstractManager<Material>
    {
        public MaterialManager() {
            data["_default"] = Material.Default;
        }

        public void Create(string key, Material mat) {
            if (data.ContainsKey(key))
            {
                data[key].Dispose();
            }
            data[key] = mat;
        }
    }
}
