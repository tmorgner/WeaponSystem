using System;
using UnityEngine;

namespace UnityTools
{
  public class TextureImage
  {
    int width;
    int height;
    Color[] color;

    public TextureImage(int width, int height)
    {
      this.width = width;
      this.height = height;
      this.color = new Color[width * height];
    }

    public void SetPixel(int x, int y, Color c)
    {
      this.color[x + y * width] = c;
    }

    public void Apply(Texture2D t)
    {
      if (color.Length != (t.width * t.height))
      {
        throw new ArgumentException();
      }

      t.SetPixels(color);
    }
  }
}
