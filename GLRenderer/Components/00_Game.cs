// #define TERMINAL_RENDERER

using OpenTK.Windowing.Desktop;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace GLRenderer.Components
{
    class Game
    {
        public GameWindow Window;

        public Scene Scene;

        private ConsoleRenderer renderer = new();
        private const int consoleRendererScale = 5;

        private Vector2i size;

        public event EventHandler<UpdateFrameEventArgs> OnFrameUpdate;

        public Game() {
            #if (TERMINAL_RENDERER)
            size = new(Console.WindowWidth * consoleRendererScale, Console.WindowHeight * consoleRendererScale * 2);
            #else
            size = new(800, 600);
            #endif

            Window = new GameWindow(
                new GameWindowSettings { },
                new NativeWindowSettings { Size = size }
            );

            Window.Load += Window_Load;
            Window.RenderFrame += Window_RenderFrame;
            Window.UpdateFrame += Window_UpdateFrame;
            Window.Unload += Window_Unload;
            Window.Resize += Window_Resize;
            #if (TERMINAL_RENDERER)
            //window.IsVisible = false;
            #endif
        }

        public void Start() {
            Window.Run();

            TurnOnDebugging();
        }

        public void Close()
        {
            Window.Close();
        }

        private void Window_Resize(ResizeEventArgs obj)
        {
            Resize(obj.Size);
            GL.Viewport(0, 0, obj.Width, obj.Height);
        }

        private void Window_Load()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            Window.CursorGrabbed = true;
        }

        private void Window_Unload()
        {

        }

        private void Window_UpdateFrame(FrameEventArgs e)
        {
            #if (TERMINAL_RENDERER)
            int width = size.X;
            int height = size.Y;

            var bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var mem = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.PixelStore(PixelStoreParameter.PackRowLength, mem.Stride / 4);
            GL.ReadPixels(0, 0, width, height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, mem.Scan0);
            bmp.UnlockBits(mem);

            Console.CursorVisible = false;
            Task.Run(() =>
            {
                renderer.RenderBitmap(bmp, false);
                bmp.Dispose();
            });
            #endif

            if (OnFrameUpdate != null) OnFrameUpdate(this, new UpdateFrameEventArgs()
            {
                KeyboardState = Window.KeyboardState,
                MouseState = Window.MouseState,
                DeltaTime = e.Time,
                IsFocused = Window.IsFocused,
                WindowSize = size
            });

            CheckError("UpdateFrame");
        }

        private void Window_RenderFrame(FrameEventArgs e)
        {
            #if (TERMINAL_RENDERER)
            Resize(new Vector2i(Console.WindowWidth * consoleRendererScale, Console.WindowHeight * consoleRendererScale * 2));
            #endif

            if (Scene != null) Scene.RenderShadows();

            GL.Viewport(0, 0, size.X, size.Y);
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if (Scene != null) Scene.Camera.AspectRatio = (float)size.X / size.Y;
            if (Scene != null) Scene.Render(e.Time, true);

            Window.SwapBuffers();
            GL.Flush();

            CheckError("RenderFrame");
        }

        private void Resize(Vector2i size)
        {
            if (size == this.size) return;

            this.size = size;

#if (TERMINAL_RENDERER)
            Window.Size = size;
            #endif

            GL.Flush();
        }

        #region OpenGL debuging

        private void CheckError(string title)
        {
            OpenTK.Graphics.OpenGL.ErrorCode code = GL.GetError();
            if (code != OpenTK.Graphics.OpenGL.ErrorCode.NoError)
            {
                Console.WriteLine($"[{title}] OpenGL error: {code}");
            }
        }

        private static void ReceiveMessage(DebugSource debugSource, DebugType type, int id, DebugSeverity severity, int len,
            IntPtr msgPtr, IntPtr customObj)
        {
            var msg = Marshal.PtrToStringAnsi(msgPtr, len);
            Console.WriteLine("Source {0}; Type {1}; id {2}; Severity {3}; msg: '{4}'", debugSource, type, id, severity, msg);
        }
        private static readonly DebugProcArb debugDelegate = new DebugProcArb(ReceiveMessage);

        private void TurnOnDebugging()
        {
            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);
            GCHandle.Alloc(debugDelegate);
            var nullptr = new IntPtr(0);
            GL.Arb.DebugMessageCallback(debugDelegate, nullptr);
        }

        #endregion
    }

    public class UpdateFrameEventArgs : EventArgs
    {
        public KeyboardState KeyboardState;
        public MouseState MouseState;
        public bool IsFocused;
        public Vector2i WindowSize;

        public double DeltaTime;

    }
}
