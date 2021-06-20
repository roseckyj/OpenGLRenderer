using GLRenderer.Mechanics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Desktop;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GLRenderer
{
    class Program
    {
        static void Main(string[] args)
        {
            GameMechanic mechanic = new();
            mechanic.Start();
        }
    }
}
