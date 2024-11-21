using System.Threading;
using UnityEngine;

namespace MaximovInk
{
    public partial class MKPixelRotSprite
    {
        [System.Serializable]
        public struct MKPixelRotRealtimeData
        {
            public MKTextureData Input;
            public MKTextureData Output;

            public int SpriteSize;
        }

        private MKPixelRotRealtimeData _realtimeData;

        private bool _realtimeIsDirty;


        private void RealtimeRotateThread(int size)
        {
            var textureData = GetRotate(_realtimeData.Input, size, _angle);

            _realtimeData.SpriteSize = size;
            _realtimeData.Output = textureData;
            _realtimeIsDirty = true;
        }

        private void RealtimeRotate()
        {
            if (_realtimeThread == null || !_realtimeThread.IsAlive)
            {
                _realtimeData.Input = MKTextureUtilites.GetSpriteDataForRot(_sprite, out var size);

                _realtimeThread = new Thread(() => RealtimeRotateThread(size));
                _realtimeThread.Start();
            }

        }

        private void UpdateRealtime()
        {
            if (_realtimeIsDirty)
            {
                _realtimeIsDirty = false;

                _finalTex = new Texture2D(_realtimeData.SpriteSize, _realtimeData.SpriteSize);
                _finalTex.SetPixels32(_realtimeData.Output.Data);
                _finalTex.alphaIsTransparency = _sprite.texture.alphaIsTransparency;
                _finalTex.filterMode = _sprite.texture.filterMode;

                _finalTex.Apply();

                _finalSprite = Sprite.Create(_finalTex, new Rect(0, 0, _finalTex.width, _finalTex.height), new Vector2(0.5f, 0.5f), _sprite.pixelsPerUnit);

                _finalSprite.name = $"Realtime: {_angle.ToString()}deg";

                _target.sprite = _finalSprite;

            }
        }
    }
}
