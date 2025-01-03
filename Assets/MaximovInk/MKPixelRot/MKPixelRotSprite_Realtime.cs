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
        private Thread _realtimeThread;

        private void RealtimeRotateThread(int size)
        {
            if (TrimSource)
            {
                _realtimeData.Input = MKTextureUtilites.Trim(_realtimeData.Input);
                size = MKTextureUtilites.GetSize(
                    _realtimeData.Input.Width, 
                    _realtimeData.Input.Height);
                _realtimeData.Input = MKTextureUtilites.ResizeUpCanvas(_realtimeData.Input, size);
            }

            var textureData = GetRotate(_realtimeData.Input, size, _angle);

            _realtimeData.SpriteSize = size;
            _realtimeData.Output = textureData;
            _realtimeIsDirty = true;
        }

        private void RealtimeRotate()
        {
            if (_realtimeIsDirty) return;

            if (_realtimeThread is { IsAlive: true }) return;

            ValidateTexture(_sprite.texture);

            _realtimeData.Input = MKTextureUtilites.GetSpriteDataForRot(_sprite, out var size);

            _realtimeThread = new Thread(() => RealtimeRotateThread(size));
            _realtimeThread.Start();

        }

        private void UpdateRealtime()
        {
            if (!_realtimeIsDirty) return;

            _realtimeIsDirty = false;

            _finalTex = MakeTexture(_realtimeData.Output.Width, _realtimeData.Output.Height);
            _finalTex.SetPixels32(_realtimeData.Output.Data);
            _finalTex.Apply();

            _finalSprite = MakeSprite(_finalTex, 0,0, _finalTex.width, _finalTex.height, $"{_sprite.name}_realtime {_angle}deg", _angle);

            _target.sprite = _finalSprite;


        }
    }
}
