using GLRenderer.Shaders;
using GLRenderer.Shaders.Static;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GLRenderer.Components
{
    public class Label : GUI
    {
        private List<GUIPlane> chars = new();
        private Vector2 position;
        private float fontSize;
        private Material mat;
        private string text;

        // TODO: finish
        private const string maping =
            "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0" +
            "\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0" + 
            " !\"#$%&'()×+,-./" +
            "0123456789:;<=>?" + 
            "@ABCDEFGHIJKLMNO" + 
            "PQRSTUVWXYZ[\\]^_" + 
            "`abcdefghijklmno" + 
            "pqrstuvwxyz{|}~\0";

        public string Text {
            get => text;
            set {
                if (text == value) return;
                text = value;
                UpdateComponents();
            }
        }

        public Label(Vector2 position, float fontSize, Material mat, string text): base()
        {
            this.position = position;
            this.fontSize = fontSize;
            this.mat = mat;
            this.text = text;
            UpdateComponents();
        }

        private void UpdateComponents()
        {
            foreach (GUI chr in chars)
            {
                chr.Dispose();
            }
            chars.Clear();

            float x = 0;
            float y = 0;
            foreach (char c in text) {
                int ind = maping.IndexOf(c);
                if (ind >= 0)
                {
                    float tx = (ind % 16) / 16f;
                    float ty = (ind / 16) / 16f;
                    chars.Add(new GUIPlane(
                        position + new Vector2(x, y) * fontSize,
                        new Vector2(fontSize),
                        mat,
                        new Vector2(tx, ty), new Vector2(tx + 1 / 16f, ty + 1 / 16f)));
                }
                if (c == '\n')
                {
                    y -= 3;
                    x = 0;
                }
                else
                {
                    x += 1.5f;
                }

            }
        }

        public override void Render(Component camera, Component playerPos, IEnumerable<Light> lights) {
            foreach(GUI chr in chars) {
                chr.Render(camera, playerPos, lights);
            }
        }

        public new void Dispose()
        {
            foreach (GUI chr in chars)
            {
                chr.Dispose();
            }
            base.Dispose();
        }
    }
}
