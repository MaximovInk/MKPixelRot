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

            public int SpriteCountX;
            public int SpriteCountY;

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
            if (!_cacheIsDirty) return;

            _cacheIsDirty = false;

            _rotationSheet = MakeTexture(_cacheData.SheetWidth, _cacheData.SheetHeight);
            _rotationSheet.SetPixels32(_cacheData.Output.Data);
            _rotationSheet.Apply();

            _sprites = new Sprite[_cacheData.SpriteCount];

            int i = 0;

            for (int iy = 0; iy < _cacheData.SpriteCountY; iy++)
            {
                for (int ix = 0; ix < _cacheData.SpriteCountX; ix++)
                {
                    if (i >= _sprites.Length) break;


                    var t = i / ((float)_cacheData.SpriteCount) * 360f;
                    var angle = CalculateAngle(t);

                    _sprites[i] =
                        MakeSprite(
                            _rotationSheet, ix * _cacheData.SpriteSize, 
                            iy*_cacheData.SpriteSize,
                            _cacheData.SpriteSize,
                            _cacheData.SpriteSize,
                            $"{_sprite.name}_cached {angle.ToString()}deg",
                            angle
                        );


                    i++;
                }
            }

            CachedRotate();

            RepaintScene();
        }

        private object _lockObject = new();

        private void GenerateRotationSheetThread()
        {


            if (TrimSource)
            {
                _cacheData.Input = 
                    MKTextureUtilites.Trim(_cacheData.Input);
                _cacheData.SpriteSize = MKTextureUtilites.GetSize(
                    _cacheData.Input.Width,
                    _cacheData.Input.Height);

                _cacheData.Input = 
                    MKTextureUtilites.ResizeUpCanvas(_cacheData.Input, _cacheData.SpriteSize);
            }

            var size = _cacheData.SpriteSize;
            var textureData = _cacheData.Input;
            _cacheData.SpriteCount = (int)(360 / _angleStep);

            var xCount = Mathf.CeilToInt(Mathf.Sqrt(_cacheData.SpriteCount));
            var yCount = Mathf.CeilToInt(_cacheData.SpriteCount / (float)xCount);

            _cacheData.SpriteCountX = xCount;
            _cacheData.SpriteCountY = yCount;

            var rotationSheetData = new MKTextureData(xCount * size, yCount * size);

            _cacheData.SheetWidth = rotationSheetData.Width;
            _cacheData.SheetHeight = rotationSheetData.Height;

            for (var i = 0; i < rotationSheetData.Length; i++)
            {
                rotationSheetData.Data[i] = Color.clear;
            }

            int xCounter = 0;
            int yCounter = 0;

            for (var i = 0; i < _cacheData.SpriteCount; i++)
            {

                var t = i / ((float)_cacheData.SpriteCount) * 360f;

                var angle = CalculateAngle(t);

                var texture1 = GetRotate(textureData, size, angle);

                if (xCounter >= xCount)
                {
                    xCounter = 0;
                    yCounter++;
                }
               
                MKTextureUtilites.InsertToTexture(
                    rotationSheetData,
                    texture1,
                    xCounter * size,
                    yCounter*size);

                xCounter++;

                _cachingState = (i / (float)_cacheData.SpriteCount);
            }

            _cacheData.Output = rotationSheetData;
            _cacheIsDirty = true;
        }

        public void GenerateRotationSheet()
        {
            if (_cacheThread is { IsAlive: true })
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
