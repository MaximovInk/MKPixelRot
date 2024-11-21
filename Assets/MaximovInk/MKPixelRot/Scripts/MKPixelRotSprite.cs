using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MaximovInk
{
    [ExecuteAlways]
    [SerializeField]
    public class MKPixelRotSprite : MonoBehaviour
    {
        [SerializeField] private Sprite _sprite;

        [SerializeField] private float _fps = 12f;

        [Range(15,180)]
        [SerializeField] private float _angleStep = 45f;

        [Tooltip("Use cache for better perfomance!")]
        [SerializeField] private bool _realtime;
        private Texture2D _finalTex;
        private Sprite _finalSprite;

        private SpriteRenderer _target;

        private void Awake()
        {
            Validate();
        }

        private float _lastAngle;
        private Quaternion _lastRotation;
        private float _previousStep = 0;
        private float _fpsTimer = 0f;

        private void Update()
        {
            if (!Application.isPlaying)
            {
                var sr = GetComponent<SpriteRenderer>();

                if(sr != null)
                {
                    _sprite = sr.sprite;

                    Validate();

                    DestroyImmediate(sr);
                }

                //return;
            }

            if (_sprite == null) return;

            _fpsTimer += Time.deltaTime;

            if (CheckRotationChanged())
            {
                _angle = CalculateAngle();

                if (_fpsTimer > (1f / _fps) && _lastAngle != _angle)
                {
                    _lastAngle = _angle;
                    Rotate();
                }
            }
        }

        private float CalculateAngle()
        {
            return Mathf.Round(transform.rotation.eulerAngles.z / _angleStep) * _angleStep;
        }

        private bool CheckRotationChanged()
        {
            if (_lastRotation != transform.rotation)
            {
                _target.transform.rotation = Quaternion.identity;
                _target.transform.position = transform.position;

                _lastRotation = transform.rotation;

                return true;
            }

            return false;
        } 

#if UNITY_EDITOR
        void SetTextureReadable(string AbsoluteFilePath)
    {
        string metadataPath = AbsoluteFilePath + ".meta";
        if (File.Exists(metadataPath))
        {
            List<string> newfile = new List<string>();

            string[] lines = File.ReadAllLines(metadataPath);
            foreach (string line in lines)
            {
                string newline = line;
                if (newline.Contains("isReadable: 0"))
                {
                    newline = newline.Replace("isReadable: 0", "isReadable: 1");
                }
                newfile.Add(newline);
            }

            File.WriteAllLines(metadataPath, newfile.ToArray());
            AssetDatabase.Refresh();

        }
    }

        private void ValidateTexture(Texture2D texture)
        {
            SetTextureReadable(AssetDatabase.GetAssetPath(texture));
        }
#endif

        [Header("Debug (do not changing)")]
        [SerializeField] private float _angle;
        [SerializeField] private Sprite[] _sprites;
        [SerializeField] private Texture2D _rotationSheet;
        
        public void GenerateRotationSheet()
        {

            _previousStep = _angleStep;

            var spriteCount = (int)(360 / _angleStep);

            var source = _sprite.texture;
            var ppu = _sprite.pixelsPerUnit;

            var spriteMin = _sprite.rect.min;
            var spriteSize = _sprite.rect.size;

            var maxSrcSize = Math.Max(spriteSize.x, spriteSize.y);
            var dstSpriteSize = (int)(maxSrcSize * 1.5f);

            _rotationSheet = new Texture2D((int)(spriteCount * dstSpriteSize), (int)(dstSpriteSize));

            _sprites = new Sprite[spriteCount];

            for (var i = 0; i < spriteCount; i++)
            {
                var angle = _angleStep * i;

                var texture1 = GetRotate(source, angle, dstSpriteSize, spriteSize, spriteMin);

                MKTextureUtilites.InsertToTexture(_rotationSheet, texture1, i * dstSpriteSize, 0);
            }

            _rotationSheet.alphaIsTransparency = source.alphaIsTransparency;
            _rotationSheet.filterMode = source.filterMode;

            _rotationSheet.Apply();

            for (var i = 0; i < spriteCount; i++)
            {
                var angle = _angleStep * i;

                _sprites[i] = Sprite.Create(_rotationSheet, new Rect(i * dstSpriteSize, 0, dstSpriteSize, dstSpriteSize), new Vector2(0.5f, 0.5f), _sprite.pixelsPerUnit);
                _sprites[i].name = $"Cached {angle.ToString()}deg";
            }



        }

        private object _lockObject = new();

        private void Rotate()
        {
            if (_realtime)
            {
                RealtimeRotate();
            }
            else
            {

                if (_sprites == null) return;

                var a = _angle;

                if (a == 360)
                {
                    a = 0;
                }

              

                var index = (int)(a / 360f * _sprites.Length);
                
                index = Mathf.Clamp(index, 0, _sprites.Length-1);


                _target.sprite = _sprites[index];
            }
        }

        private Texture2D GetRotate(Texture2D source, float angle, int size, Vector2 srcSize, Vector2 srcMin)
        {
            _finalTex = new Texture2D(size, size);

            var offset = new Vector2(_finalTex.width / 2 - srcSize.x / 2, _finalTex.height / 2 - srcSize.y / 2);

            for (int i = 0; i < _finalTex.width; i++)
            {
                for (int j = 0; j < _finalTex.height; j++)
                {
                    _finalTex.SetPixel(i, j, Color.clear);
                }
            }

            for (int i = 0; i < srcSize.x; i++)
            {
                for (int j = 0; j < srcSize.y; j++)
                {
                    var sX = (int)(srcMin.x + i);
                    var sY = (int)(srcMin.y + j);
                    var dX = (int)(i + offset.x);
                    var dY = (int)(j + offset.y);

                    var pixel = source.GetPixel(sX, sY);
                    _finalTex.SetPixel(dX, dY, pixel);
                }
            }

            if(angle == 0 || angle == 360)
            {
                _finalTex.alphaIsTransparency = source.alphaIsTransparency;
                _finalTex.filterMode = source.filterMode;

                _finalTex.Apply();

                return _finalTex;
            }
            if(angle == 90)
            {
                MKTextureUtilites.Rotate90(_finalTex, false);
                _finalTex.alphaIsTransparency = source.alphaIsTransparency;
                _finalTex.filterMode = source.filterMode;

                _finalTex.Apply();

                return _finalTex;

            }
            if (angle == 180)
            {
                MKTextureUtilites.Rotate90(_finalTex, false);
                MKTextureUtilites.Rotate90(_finalTex, false);

                _finalTex.alphaIsTransparency = source.alphaIsTransparency;
                _finalTex.filterMode = source.filterMode;

                _finalTex.Apply();

                return _finalTex;

            }
            if (angle == 270)
            {
                MKTextureUtilites.Rotate90(_finalTex, false);
                MKTextureUtilites.Rotate90(_finalTex, false);
                MKTextureUtilites.Rotate90(_finalTex, false);

                _finalTex.alphaIsTransparency = source.alphaIsTransparency;
                _finalTex.filterMode = source.filterMode;

                _finalTex.Apply();

                return _finalTex;

            }

            _finalTex = MKTextureUtilites.Scale2x(_finalTex, false);
            _finalTex = MKTextureUtilites.Scale2x(_finalTex, false);
            _finalTex = MKTextureUtilites.Scale2x(_finalTex, false);

            MKTextureUtilites.Rotate(_finalTex, angle, true);

            MKTextureUtilites.ScaleDown(_finalTex, size, size, true);

            _finalTex.alphaIsTransparency = source.alphaIsTransparency;
            _finalTex.filterMode = source.filterMode;

            _finalTex.Apply();

            return _finalTex;
        }

        private void RealtimeRotate()
        {
            var source = _sprite.texture;

            var ppu = _sprite.pixelsPerUnit;

            var originPx = _sprite.rect.min;
            var pixelSize = _sprite.rect.size;

            var maxS = Math.Max(pixelSize.x, pixelSize.y);
            var size = (int)(maxS * 1.5f);
            _finalTex = new Texture2D(size, size);
            var offset = new Vector2(_finalTex.width / 2 - pixelSize.x / 2, _finalTex.height / 2 - pixelSize.y / 2);

            for (int i = 0; i < _finalTex.width; i++)
            {
                for (int j = 0; j < _finalTex.height; j++)
                {
                    _finalTex.SetPixel(i, j, Color.clear);
                }
            }

            for (int i = 0; i < pixelSize.x; i++)
            {
                for (int j = 0; j < pixelSize.y; j++)
                {
                    var sX = (int)(originPx.x + i);
                    var sY = (int)(originPx.y + j);
                    var dX = (int)(i + offset.x);
                    var dY = (int)(j + offset.y);

                    var pixel = source.GetPixel(sX, sY);
                    _finalTex.SetPixel(dX, dY, pixel);
                }
            }



            _finalTex = MKTextureUtilites.Scale2x(_finalTex, false);
            _finalTex = MKTextureUtilites.Scale2x(_finalTex, false);
            _finalTex = MKTextureUtilites.Scale2x(_finalTex, false);

            MKTextureUtilites.Rotate(_finalTex, _angle, true);

            MKTextureUtilites.ScaleDown(_finalTex, size, size, true);

            _finalTex.alphaIsTransparency = source.alphaIsTransparency;
            _finalTex.filterMode = source.filterMode;

            _finalTex.Apply();

            _finalSprite = Sprite.Create(_finalTex, new Rect(0, 0, _finalTex.width, _finalTex.height), new Vector2(0.5f, 0.5f), _sprite.pixelsPerUnit);

            _finalSprite.name = $"Realtime: {_angle.ToString()}deg";

            _target.sprite = _finalSprite;
        }

        private void Validate()
        {
            if (_sprite == null) return;

            if (_target == null)
            {
                MKUtils.DestroyAllChildren(transform);

                var renderer = new GameObject($"{gameObject.name}_PixelRot").AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(transform);
                renderer.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                _target = renderer;
            }

            var source = _sprite.texture;

            if (!source.isReadable) {

#if UNITY_EDITOR
                ValidateTexture(source);
#else
                Debug.LogError($"Texture is not readable! {source.name}");
                return;
#endif
            }

            RealtimeRotate();
        }
    }
}