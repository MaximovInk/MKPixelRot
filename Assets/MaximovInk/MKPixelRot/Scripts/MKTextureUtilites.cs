
using System;
using System.Threading;
using UnityEngine;

namespace MaximovInk
{
    public static class MKTextureUtilites
    {/// <summary>
     /// Use "Scale2x" algorithm to produce new texture from inputTexture.
     /// </summary>
     /// <param name='inputTexture'>
     /// Input texture.
     /// </param>
        public static Texture2D Scale2x(Texture2D inputTexture, bool apply = true)
        {
            Texture2D returnTexture = new Texture2D(inputTexture.width * 2, inputTexture.height * 2);

            // Every pixel from input texture produces 4 output pixels, for more details check out http://scale2x.sourceforge.net/algorithm.html
            int y = 0;
            while (y < inputTexture.height)
            {
                int x = 0;
                while (x < inputTexture.width)
                {
                    Color colorB = inputTexture.GetPixel(x, y - 1);
                    Color colorH = inputTexture.GetPixel(x, y + 1);
                    Color colorD = inputTexture.GetPixel(x - 1, y);
                    Color colorF = inputTexture.GetPixel(x + 1, y);

                    Color colorE = inputTexture.GetPixel(x, y);

                    if (!AreColorsSame(colorB, colorH) && !AreColorsSame(colorD, colorF))
                    {
                        returnTexture.SetPixel(2 * x, 2 * y, AreColorsSame(colorD, colorB) ? colorD : colorE);
                        returnTexture.SetPixel(2 * x + 1, 2 * y, AreColorsSame(colorB, colorF) ? colorF : colorE);
                        returnTexture.SetPixel(2 * x, 2 * y + 1, AreColorsSame(colorD, colorH) ? colorD : colorE);
                        returnTexture.SetPixel(2 * x + 1, 2 * y + 1, AreColorsSame(colorH, colorF) ? colorF : colorE);
                    }

                    else
                    {
                        returnTexture.SetPixel(2 * x, 2 * y, colorE);
                        returnTexture.SetPixel(2 * x + 1, 2 * y, colorE);
                        returnTexture.SetPixel(2 * x, 2 * y + 1, colorE);
                        returnTexture.SetPixel(2 * x + 1, 2 * y + 1, colorE);
                    }

                    x++;
                }
                y++;

            }


            returnTexture.filterMode = inputTexture.filterMode;
            if (apply)
                returnTexture.Apply(false, false);

            return returnTexture;
        }

        //https://discussions.unity.com/t/rotate-an-image-by-modifying-texture2d-getpixels32-array/102125/4
        public static void Rotate(Texture2D originTexture, float angle, bool apply = true)
        {
            int oldX;
            int oldY;
            int width = originTexture.width;
            int height = originTexture.height;

            Color32[] originPixels = originTexture.GetPixels32();
            Color32[] transformedPixels = originTexture.GetPixels32();
            float phi = Mathf.Deg2Rad * angle;

            for (int newY = 0; newY < height; newY++)
            {
                for (int newX = 0; newX < width; newX++)
                {
                    transformedPixels[newY * width + newX] = new Color32(0, 0, 0, 0);
                    int newXNormToCenter = newX - width / 2;
                    int newYNormToCenter = newY - height / 2;
                    oldX = (int)(Mathf.Cos(phi) * newXNormToCenter + Mathf.Sin(phi) * newYNormToCenter + width / 2);
                    oldY = (int)(-Mathf.Sin(phi) * newXNormToCenter + Mathf.Cos(phi) * newYNormToCenter + height / 2);
                    bool InsideImageBounds = (oldX > -1) && (oldX < width) && (oldY > -1) && (oldY < height);

                    if (InsideImageBounds)
                    {
                        transformedPixels[newY * width + newX] = originPixels[oldY * width + oldX];
                    }
                }
            }

            originTexture.SetPixels32(transformedPixels);

            if (apply)
                originTexture.Apply();
        }

        public static void Rotate90(Texture2D originTexture, bool apply = true)
        {
            int oldX;
            int oldY;
            int width = originTexture.width;
            int height = originTexture.height;

            Color32[] originPixels = originTexture.GetPixels32();
            Color32[] transformedPixels = originTexture.GetPixels32();

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    int newRow = col;
                    int newCol = height - (row + 1);

                    transformedPixels[newRow * width + newCol] = originPixels[row * width + col];
                
                }
            }

            originTexture.SetPixels32(transformedPixels);

            if (apply)
                originTexture.Apply();
        }


        /// <summary>
        /// Checks if the colors are the same.
        /// </summary>
        /// <returns>
        /// True if they are; otherwise false
        /// </returns>
        /// <param name='a'>
        /// First color.
        /// </param>
        /// <param name='b'>
        /// Second color.
        /// </param>
        private static bool AreColorsSame(Color aColor, Color bColor)
        {
            return Mathf.Approximately(aColor.r, bColor.r) &&
                Mathf.Approximately(aColor.g, bColor.g) &&
                Mathf.Approximately(aColor.b, bColor.b) &&
                Mathf.Approximately(aColor.a, bColor.a);
        }

        public static void ScaleDown(Texture2D texture, int newWidth, int newHeight, bool apply = true)
        {
            Color32[] originPixels = texture.GetPixels32();
            Color32[] transformedPixels = new Color32[newWidth * newHeight];

            var srcW = texture.width;
            var srcH = texture.height;

            var stepX = (int)(srcW / (float)newWidth);
            var stepY = (int)(srcH / (float)newHeight);


            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {



                    var srcX = (int)(stepX * x);
                    var srcY = (int)(stepY * y);

                    if (srcX >= srcW || srcY >= srcH) continue;

                    var pixel = originPixels[srcX + srcY * srcW];

                    for (int ix = 0; ix < stepX; ix++)
                    {
                        for(int iy = 0; iy < stepY; iy++)
                        {
                            pixel = Color.Lerp(pixel,originPixels[(srcX + ix) + (srcY+iy) * srcW],0.5f);
                        }
                    }


                    transformedPixels[x + newWidth * y] = pixel;

                    //transformedPixels[newY * width + newX] = originPixels[oldY * width + oldX];


                }

            }

            texture.Reinitialize(newWidth, newHeight);
            texture.SetPixels32(transformedPixels);
            if (apply)
            {
                texture.Apply();
            }
        }

        public static void InsertToTexture(Texture2D canvas, Texture2D source, int xOffset, int yOffset)
        {
            var pixels = source.GetPixels();

            for (int i = 0; i < source.width; i++)
            {
                for (int j = 0; j < source.height; j++)
                {
                    var pixel = pixels[i + j * source.width];

                    canvas.SetPixel(i + xOffset, j + yOffset, pixel);
                }
            }
        }
    
    }

    /// A unility class with functions to scale Texture2D Data.
    ///
    /// Scale is performed on the GPU using RTT, so it's blazing fast.
    /// Setting up and Getting back the texture data is the bottleneck.
    /// But Scaling itself costs only 1 draw call and 1 RTT State setup!
    /// WARNING: This script override the RTT Setup! (It sets a RTT!)  
    ///
    /// Note: This scaler does NOT support aspect ratio based scaling. You will have to do it yourself!
    /// It supports Alpha, but you will have to divide by alpha in your shaders,
    /// because of premultiplied alpha effect. Or you should use blend modes.
    public static class GPUTextureScaler
    {
        /// <summary>
        ///     Returns a scaled copy of given texture.
        /// </summary>
        /// <param name="tex">Source texure to scale</param>
        /// <param name="width">Destination texture width</param>
        /// <param name="height">Destination texture height</param>
        /// <param name="mode">Filtering mode</param>
        public static Texture2D Scaled(Texture2D src, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new(0, 0, width, height);
            _gpu_scale(src, width, height, mode);

            //Get rendered data back to a new texture
            Texture2D result = new(width, height, TextureFormat.ARGB32, true);
            result.Reinitialize(width, height);
            result.ReadPixels(texR, 0, 0, true);
            return result;
        }

        /// <summary>
        ///     Scales the texture data of the given texture.
        /// </summary>
        /// <param name="tex">Texure to scale</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="mode">Filtering mode</param>
        public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new(0, 0, width, height);
            _gpu_scale(tex, width, height, mode);

            // Update new texture
            tex.Reinitialize(width, height);
            tex.ReadPixels(texR, 0, 0, true);
            tex.Apply(true); //Remove this if you hate us applying textures for you :)
        }

        // Internal unility that renders the source texture into the RTT - the scaling method itself.
        private static void _gpu_scale(Texture2D src, int width, int height, FilterMode fmode)
        {
            //We need the source texture in VRAM because we render with it
            src.filterMode = fmode;
            src.Apply(true);

            //Using RTT for best quality and performance. Thanks, Unity 5
            RenderTexture rtt = new(width, height, 32);

            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(rtt);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
    }
}
