using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace GLRenderer.Components
{
    public class Texture : IDisposable
    {
        public int Handle { get; private set; }

        private string source;


        private Texture(int glHandle)
        {
            Handle = glHandle;
        }

        private Texture(string source)
        {
            this.source = source;
        }

        public void Use(TextureUnit unit)
        {
            if (Handle < 1) Init();

            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2D, Handle);
        }


        public void Dispose()
        {
            GL.DeleteTexture(Handle);
        }

        public void ReplaceWith(Texture newTex)
        {
            Handle = newTex.Handle;
        }

        public Texture Copy()
        {
            Texture copy = new Texture(this.Handle);
            return copy;
        }

        public static Texture LoadFromFile(string path, bool isInThread = false)
        {
            if (isInThread) return new Texture(path);

            Texture t = new(path);
            t.Init();
            return t;
        }

        private void Init() {
            if (Handle > 0 || source == null) return;

            Handle = GL.GenTexture();

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            using (var image = new Bitmap(source))
            {
                var data = image.LockBits(
                    new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba,
                    image.Width,
                    image.Height,
                    0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                    PixelType.UnsignedByte,
                    data.Scan0);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        public static Texture CreateShadowMap(Vector2i size)
        {
            int handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, handle);

            GL.TexImage2D(TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.DepthComponent,
                    size.X,
                    size.Y,
                    0,
                    OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                    PixelType.Float,
                    IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (float)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (float)TextureWrapMode.ClampToBorder);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });

            return new Texture(handle);
        }
    }
}