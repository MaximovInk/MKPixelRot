using System.Collections;
using System.Threading;
using UnityEngine;

namespace MaximovInk
{
    public partial class MKPixelRotSprite
    {
        [System.Serializable]
        public struct MKPixelRotCacheData
        {
            public int SheetWidth;
            public int SheetHeight;

            public MKTextureData Input;
            public MKTextureData Output;

            public int SpriteCount;

            public int SpriteSize;
        }


        [Header("Cached info")]
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private Texture2D _rotationSheet;

        private bool _cacheIsDirty;

        private MKPixelRotCacheData _cacheData;

        private Thread _cacheThread;

        public float CachingState => _cachingState;
        private float _cachingState;

        private void UpdateCached()
        {
            if (_cacheIsDirty)
            {
                _cacheIsDirty = false;

                _rotationSheet = MakeTexture(_cacheData.SheetWidth, _cacheData.SheetHeight);
                _rotationSheet.SetPixels32(_cacheData.Output.Data);
                _rotationSheet.Apply();

                _sprites = new Sprite[_cacheData.SpriteCount];
                
                for (var i = 0; i < _cacheData.SpriteCount; i++)
                {
                    var angle = _angleStep * i;

                    _sprites[i] = 
                        MakeSprite(
                            _rotationSheet, i * _cacheData.SpriteSize, 
                            0,
                            _cacheData.SpriteSize, 
                            _cacheData.SpriteSize,
                            $"{_sprite.name}_cached {angle.ToString()}deg"
                            );
                }
    
                CachedRotate();

                RepaintScene();
         

            }
        }

        private void GenerateRotationSheetThread()
        {
            var size = _cacheData.SpriteSize;
            _previousStep = _angleStep;
            var textureData = _cacheData.Input;
            _cacheData.SpriteCount = (int)(360 / _angleStep);

            _cacheData.SheetWidth = _cacheData.SpriteCount * size;
            _cacheData.SheetHeight = size;

            var rotationSheetData = new MKTextureData(_cacheData.SpriteCount * size, size);
         

            for (int i = 0; i < rotationSheetData.Length; i++)
            {
                rotationSheetData.Data[i] = Color.clear;
            }

            for (var i = 0; i < _cacheData.SpriteCount; i++)
            {
                var angle = _angleStep * i;
                var texture1 = GetRotate(textureData, size, angle);

                MKTextureUtilites.InsertToTexture(
                    rotationSheetData,
                    texture1,
                    i * size,
                    0,
                    size,
                    size,
                    _cacheData.SheetWidth,
                    _cacheData.SheetHeight
                );

                _cachingState = (i/(float)_cacheData.SpriteCount);
            }

            _cacheData.Output = rotationSheetData;
            _cacheIsDirty = true;
        }

        private IEnumerator ThisWillBeExecutedOnTheMainThread()
        {
            Debug.Log("This is executed from the main thread");
            yield return null;
        }

        public void GenerateRotationSheet()
        {
            if (_cacheThread != null && _cacheThread.IsAlive)
            {
                _cacheThread.Abort();
                _cacheThread = null;
            }

            if (_sprite == null)
            {
                Debug.LogError("Sprite is null!");
                return;
            }

          
            _cachingState = 0;
            _cacheData.Input = MKTextureUtilites.GetSpriteDataForRot(_sprite, out var size);
            _cacheData.SpriteSize = size;
            _cacheThread = new Thread(GenerateRotationSheetThread);
            _cacheThread.Start();

        }

        private void CachedRotate()
        {
            if (_sprites == null) return;

            var index = (int)(_angle / 360f * _sprites.Length);

            index = Mathf.Clamp(index, 0, _sprites.Length - 1);

            if(_sprites.Length == 0)
            {
                return;
            }

            _target.sprite = _sprites[index];
        }
    }
}
