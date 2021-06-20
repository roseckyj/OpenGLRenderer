using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Managers
{
    public abstract class AbstractManager<T>
    {
        protected Dictionary<string, T> data = new();

        public T Get(string key) {
            return data[key];
        }
    }
}
