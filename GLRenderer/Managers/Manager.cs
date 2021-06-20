using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLRenderer.Managers
{
    public static class Manager
    {
        public static SceneManager Scene { get; } = new();
        public static ModelManager Model { get; } = new();
        public static MaterialManager Material { get; } = new();
        public static TextureManager Texture { get; } = new();
    }
}
