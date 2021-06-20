using GLRenderer.Components;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Managers
{
    public class ModelManager : AbstractManager<Model>
    {
        public ModelManager() {
            data["_default"] = Model.Cube(); // Maybe empty here?
        }

        public void CreateFromObjFile(string key, string path) {
            if (data.ContainsKey(key)) {
                data[key].Dispose();
            }
            data[key] = data["_default"].Copy();

            Task.Run(() => {
                var k = key;
                var loaded = Model.FromObjFile(path);
                data[k].ReplaceWith(loaded);
            });
        }
    }
}
