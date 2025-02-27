﻿using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Unity.VisualScripting.Member;
using static UnityEngine.UI.Image;

namespace MaximovInk
{
    public static class MKTextureUtilites
    {
        public static MKTextureData Trim(MKTextureData textureData)
        {
            var minX = textureData.Width-1;
            var minY = textureData.Height-1;
            var maxX = 0;
            var maxY = 0;


            for (int ix = 0; ix < textureData.Width; ix++)
            {
                for (int iy = 0; iy < textureData.Height; iy++)
                {
                    var pixel = textureData.Get(ix, iy);

                    if(pixel.a == 0) continue;

                    minX = Math.Min(minX, ix);
                    minY = Math.Min(minY, iy);

                    maxX = Math.Max(maxX, ix);
                    maxY = Math.Max(maxY, iy);
                }
            }

            var sizeX = maxX - minX+1;
            var sizeY = maxY - minY+1;

            if (sizeX < 1 || sizeY < 1)
            {
                Debug.Log("Failed to trim texture!");
                return textureData;
            }

            var newTexture = new MKTextureData(sizeX,sizeY);

            for (int ix = 0; ix < sizeX; ix++)
            {
                for (int iy = 0; iy < sizeY; iy++)
                {
                    var pixel = textureData.Get(minX + ix, minY + iy);

                    newTexture.Set(ix,iy, pixel);
                }
            }

            return newTexture;
        }

        public static int GetSize(float w, float h)
        {
            var maxS = Math.Max(w, h);
            return (int)(maxS * 1.3f);
        }

        public static MKTextureData GetSpriteDataForRot(Sprite sprite)
        {
            var source = sprite.texture;

            var originPx = sprite.rect.min;
            var pixelSize = sprite.rect.size;

            var textureData = new MKTextureData((int)pixelSize.x,(int) pixelSize.y);

            for (var i = 0; i < textureData.Length; i++)
            {
                textureData.Data[i] = Color.clear;
            }

            var sourceData = source.GetPixels32();

            for (var i = 0; i < pixelSize.x; i++)
            {
                for (var j = 0; j < pixelSize.y; j++)
                {
                    var sX = (int)(originPx.x + i);
                    var sY = (int)(originPx.y + j);

                    var pixel = GetUnsafe(sourceData, sX, sY, source.width);

                    textureData.SetUnsafe(i, j, pixel);
                }
            }

            return textureData;
        }

        public static MKTextureData ResizeUpCanvas(MKTextureData textureData, int newSize)
        {
            var offset = new Vector2(newSize / 2 - textureData.Width / 2, newSize / 2 - textureData.Height / 2);

            var result = new MKTextureData(newSize, newSize);

            for (var sX = 0; sX < textureData.Width; sX++)
            {
                for (var sY = 0; sY < textureData.Height; sY++)
                {
                    var dX = (int)(sX + offset.x);
                    var dY = (int)(sY + offset.y);

                    var pixel = textureData.GetUnsafe(sX, sY);

                    result.Set(dX, dY, pixel);
                }
            }

            return result;
        }

        public static MKTextureData GetSpriteDataForRot(Sprite sprite, out int size)
        {
            var source = sprite.texture;

            var originPx = sprite.rect.min;
            var pixelSize = sprite.rect.size;

            size = GetSize(pixelSize.x, pixelSize.y);
            var textureData = new MKTextureData(size, size);

            var offset = new Vector2(size / 2 - pixelSize.x / 2, size / 2 - pixelSize.y / 2);

            for (var i = 0; i < textureData.Length; i++)
            {
                textureData.Data[i] = Color.clear;
            }

            var sourceData = source.GetPixels32();

            for (var i = 0; i < pixelSize.x; i++)
            {
                for (var j = 0; j < pixelSize.y; j++)
                {
                    var sX = (int)(originPx.x + i);
                    var sY = (int)(originPx.y + j);
                    var dX = (int)(i + offset.x);
                    var dY = (int)(j + offset.y);

                    var pixel = GetUnsafe(sourceData, sX, sY, source.width);

                    textureData.SetUnsafe(dX, dY, pixel);
                }
            }

            return textureData;
        }

        public static MKTextureData Scale2x(MKTextureData textureData)
        {
            var w = textureData.Width;
            var h = textureData.Height;

            int newW = w * 2;
            int newH = h * 2;

            MKTextureData result = new MKTextureData(newW, newH);

            int y = 0;
            while (y < h)
            {
                int x = 0;
                while (x < w)
                {
                    Color colorB = textureData.Get(x, y - 1);
                    Color colorH = textureData.Get(x, y + 1);
                    Color colorD = textureData.Get(x - 1, y);
                    Color colorF = textureData.Get(x + 1, y);

                    Color colorE = textureData.Get(x, y);

                    if (!AreColorsSame(colorB, colorH) && !AreColorsSame(colorD, colorF))
                    {
                        result.SetUnsafe(2 * x, 2 * y, AreColorsSame(colorD, colorB) ? colorD : colorE);
                        result.SetUnsafe(2 * x + 1, 2 * y, AreColorsSame(colorB, colorF) ? colorF : colorE);
                        result.SetUnsafe(2 * x, 2 * y + 1, AreColorsSame(colorD, colorH) ? colorD : colorE);
                        result.SetUnsafe(2 * x + 1, 2 * y + 1, AreColorsSame(colorH, colorF) ? colorF : colorE);
                    }

                    else
                    {
                        result.Set(2 * x, 2 * y, colorE);
                        result.Set(2 * x + 1, 2 * y, colorE);
                        result.Set(2 * x, 2 * y + 1, colorE);
                        result.Set(2 * x + 1, 2 * y + 1, colorE);
                    }

                    x++;
                }
                y++;

            }

            return result;
        }

        public static void Rotate(MKTextureData textureData, float angle)
        {
            var width = textureData.Width;
            var height = textureData.Height;

            MKTextureData transformedPixels = new MKTextureData(textureData.Width, textureData.Height);

            var phi = Mathf.Deg2Rad * angle;

            for (var newY = 0; newY < height; newY++)
            {
                for (var newX = 0; newX < width; newX++)
                {
                    transformedPixels.Set(newX, newY, new Color32(0, 0, 0, 0));
                    var newXNormToCenter = newX - width / 2;
                    var newYNormToCenter = newY - height / 2;
                    var oldX = (int)(Mathf.Cos(phi) * newXNormToCenter + Mathf.Sin(phi) * newYNormToCenter + width / 2);
                    var oldY = (int)(-Mathf.Sin(phi) * newXNormToCenter + Mathf.Cos(phi) * newYNormToCenter + height / 2);
                    var insideImageBounds = (oldX > -1) && (oldX < width) && (oldY > -1) && (oldY < height);

                    if (!insideImageBounds) continue;

                    var pixel = textureData.GetUnsafe(oldX, oldY);
                    transformedPixels.SetUnsafe(newX, newY, pixel);
                }
            }

            for (var i = 0; i < transformedPixels.Length; i++)
            {
                textureData.Data[i] = transformedPixels.Data[i];
            }
        }

        public static void Rotate90(MKTextureData textureData)
        {
            var width = textureData.Width;
            var height = textureData.Height;

            MKTextureData transformedPixels = new MKTextureData(textureData.Width, textureData.Height);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int newRow = col;
                    int newCol = height - (row + 1);

                    var pixel = textureData.GetUnsafe(col, row);
                    transformedPixels.SetUnsafe(newCol, newRow, pixel);
                }
            }

            for (int i = 0; i < transformedPixels.Length; i++)
            {
                textureData.Data[i] = transformedPixels.Data[i];
            }
        }

        public static MKTextureData ScaleDown(MKTextureData textureData, int newWidth, int newHeight, float blend)
        {
            MKTextureData transformedPixels = new MKTextureData(newWidth, newHeight);

            var oldWidth = textureData.Width;
            var oldHeight = textureData.Height;

            var stepX = (int)(oldWidth / (float)newWidth);
            var stepY = (int)(oldHeight / (float)newHeight);

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    var srcX = (int)(stepX * x);
                    var srcY = (int)(stepY * y);

                    if (srcX >= oldWidth || srcY >= oldHeight) continue;

                    var pixel = textureData.GetUnsafe(srcX, srcY);

                    for (int ix = 0; ix < stepX; ix++)
                    {
                        for (int iy = 0; iy < stepY; iy++)
                        {
                            pixel = Color.Lerp(pixel, textureData.Get(srcX + ix, srcY + iy), blend);
                        }
                    }

                    transformedPixels.SetUnsafe(x, y, pixel);
                }

            }

            return transformedPixels;
        }
     
        public static void InsertToTexture(MKTextureData canvas, MKTextureData textureData, int xOffset, int yOffset)
         {
             for (int i = 0; i < textureData.Width; i++)
             {
                 for (int j = 0; j < textureData.Height; j++)
                 {
                     var pixel = textureData.GetUnsafe(i, j);

                     canvas.SetUnsafe(i + xOffset, j + yOffset, pixel);
                 }
             }
         }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool AreColorsSame(Color aColor, Color bColor)
        {
            return Mathf.Approximately(aColor.r, bColor.r) &&
                Mathf.Approximately(aColor.g, bColor.g) &&
                Mathf.Approximately(aColor.b, bColor.b) &&
                Mathf.Approximately(aColor.a, bColor.a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetIndex2D(int x, int y, int width)
        {
            return x + y * width;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 GetUnsafe(Color32[] textureData, int x, int y, int width)
        {
            return textureData[GetIndex2D(x, y, width)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color32 Get(Color32[] textureData, int x, int y, int width, int height)
        {
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);


            return textureData[GetIndex2D(x, y, width)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetUnsafe(Color32[] textureData, int x, int y, int width, Color32 data)
        {
            textureData[GetIndex2D(x, y, width)] = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(Color32[] textureData, int x, int y, int width, int height, Color32 data)
        {
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);

            textureData[GetIndex2D(x, y, width)] = data;
        }

    }
}
