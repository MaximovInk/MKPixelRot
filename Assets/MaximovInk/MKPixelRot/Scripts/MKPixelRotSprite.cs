using UnityEngine;
using System;

namespace MaximovInk
{
    [ExecuteAlways]
    [SerializeField]
    public partial class MKPixelRotSprite : MonoBehaviour
    {
        public bool IsBusy() => _cacheThread != null && _cacheThread.IsAlive;
        public float Angle => _angle;

        [SerializeField] private Sprite _sprite;

        [SerializeField] private float _fps = 12f;

        [Range(1,180)]
        [SerializeField] private float _angleStep = 45f;

        [Tooltip("Use cache for better perfomance!")]
        [SerializeField] private bool _realtime;
        private Texture2D _finalTex;
        private Sprite _finalSprite;

        public SpriteRenderer Renderer => _target;
        [HideInInspector, SerializeField]
        private SpriteRenderer _target;

        private void Awake()
        {
            Validate();
        }

        private float _lastAngle;
        private Quaternion _lastRotation;
        private float _previousStep = 0;
        private float _fpsTimer = 0f;

        private bool _invokeRotate;

        private void LateUpdate()
        {
            Validate();

            if (!Application.isPlaying)
            {
                var sr = GetComponent<SpriteRenderer>();

                if(sr != null)
                {
                    _sprite = sr.sprite;

                    DestroyImmediate(sr);
                }
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

            UpdateCached();
            UpdateRealtime();

            if (_invokeRotate)
            {
                _invokeRotate = false;
                Rotate();
            }
        }

        private float CalculateAngle()
        {
            var value = Mathf.Round(transform.rotation.eulerAngles.z / _angleStep) * _angleStep; ;

            while(value > 360)
            {
                value -= 360;
            }

            return value;
        }

        private bool CheckRotationChanged()
        {
            if (_target == null) return false;

            if (_lastRotation != transform.rotation)
            {
                _target.transform.position = transform.position;
                _target.transform.rotation = Quaternion.identity;
                _lastRotation = transform.rotation;

                return true;
            }

            return false;
        }

        private float _angle;

        public void Rotate()
        {
            if (_target == null) return;

            if (_realtime)
            {
                RealtimeRotate();
            }
            else
            {
                CachedRotate();
            }
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
                //renderer.gameObject.hideFlags = HideFlags.HideInHierarchy;

                _target = renderer;

                Rotate();
            }

            var source = _sprite.texture;

            ValidateTexture(source);

        }

        private void OnValidate()
        {
            _invokeRotate = true;
        }

        private MKTextureData GetRotate(MKTextureData textureData, int size, float angle)
        {
            int originalSize = size;
            int scaledSize = size;

            bool skip = false;

            if (angle == 360 || angle == 0)
            {
                var result = new MKTextureData(textureData.Width, textureData.Height);

                for (int i = 0; i < textureData.Length; i++)
                {
                    result.Data[i] = textureData.Data[i];
                }

                textureData = result;

                skip = true;
            }

            if (angle == 90)
            {
                MKTextureUtilites.Rotate90(textureData);
                skip = true;
            }
            else if (angle == 180)
            {
                MKTextureUtilites.Rotate90(textureData);
                MKTextureUtilites.Rotate90(textureData);
                skip = true;
            }
            else if (angle == 270)
            {
                MKTextureUtilites.Rotate90(textureData);
                MKTextureUtilites.Rotate90(textureData);
                MKTextureUtilites.Rotate90(textureData);
                skip = true;

            }
            
            if (!skip)
            {
                textureData = MKTextureUtilites.Scale2x(textureData);
                scaledSize *= 2;

                textureData = MKTextureUtilites.Scale2x(textureData);
                scaledSize *= 2;

                textureData = MKTextureUtilites.Scale2x(textureData);
                scaledSize *= 2;

                MKTextureUtilites.Rotate(textureData, angle);

                textureData = MKTextureUtilites.ScaleDown(textureData, originalSize, originalSize);
            }

            scaledSize = originalSize;

            return textureData;
        }
   
        public void StopAllThreads()
        {
            if(_realtimeThread != null)
            {
                _realtimeThread.Abort();
                _realtimeThread = null;
            }

            if (_cacheThread != null)
            {
                _cacheThread.Abort();
                _cacheThread = null;
            }
        }

        private Texture2D MakeTexture(int width, int height)
        {
            var texture = new Texture2D(width, height);

            texture.alphaIsTransparency = true;
            texture.filterMode = FilterMode.Point;
            //texture.wrapMode = TextureWrapMode.Mirror;

            return texture;
        }
   
        private Sprite MakeSprite(Texture2D texture, int x, int y, int w, int h, string name)
        {
            var sprite = Sprite.Create(
                   texture,
                   new Rect(x, y, w, h),
                   new Vector2(0.5f, 0.5f),
                       _sprite.pixelsPerUnit);
            sprite.name = name;

            return sprite;
        }
    }
}