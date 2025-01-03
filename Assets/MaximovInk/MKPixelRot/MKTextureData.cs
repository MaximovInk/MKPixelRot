using System.Runtime.CompilerServices;
using UnityEngine;

namespace MaximovInk
{
    [System.Serializable]
    public struct MKTextureData
    {
        public Color32[] Data;
        public int Width;
        public int Height;
        public int Length => Data.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndex2D(int x, int y)
        {
            return x + y * Width;
        }

        public Color32 GetUnsafe(int x, int y)
        {
            return Data[GetIndex2D(x, y)];
        }

        public Color32 Get( int x, int y)
        {
            x = Mathf.Clamp(x, 0, Width - 1);
            y = Mathf.Clamp(y, 0, Height - 1);

            return Data[GetIndex2D(x, y)];
        }

        public void SetUnsafe( int x, int y, Color32 data)
        {
            Data[GetIndex2D(x, y)] = data;
        }

        public void Set( int x, int y, Color32 data)
        {
            x = Mathf.Clamp(x, 0, Width - 1);
            y = Mathf.Clamp(y, 0, Height - 1);

            Data[GetIndex2D(x, y)] = data;
        }

        public MKTextureData(int w, int h)
        {
            Width = w;
            Height = h;

            Data = new Color32[w * h];
        }

    }
}
