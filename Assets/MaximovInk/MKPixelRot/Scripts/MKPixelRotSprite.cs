using System;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace MaximovInk
{
    [ExecuteAlways]
    public partial class MKPixelRotSprite : MonoBehaviour
    {
        public bool IsBusy() => 
            _cacheThread is { IsAlive: true } ||
            _realtimeThread is { IsAlive: true };
        public int Angle => _angle;
        public SpriteRenderer Renderer => _target;
        public Transform RotationTransform => _rotationTransform;
        public bool TrimSource { get=> _trimSource; set=> _trimSource = value; }
        public Vector2 Pivot
        {
            get => _pivot;
            set => _pivot = value;
        }
        public float BlendDownScale
        {
            get=>_blendDownScale;
            set => _blendDownScale = value;
        }
        public bool IsRealtime
        {
            get => _realtime;
            set => _realtime = value;
        }
        public float AngleOffset
        {
            get => _angleOffset;
            set => _angleOffset = value;
        }

        [SerializeField] private Sprite _sprite;

        [SerializeField] private Vector2 _pivot = new(0.5f, 0.5f);
        [SerializeField] private Vector2 _rendererOffset = Vector2.zero;
        [SerializeField] private float _angleOffset;

        [SerializeField] private float _fps = 24f;

        [Range(1,180)]
        [SerializeField] private float _angleStep = 15f;

        [Tooltip("Use realtime mode for testing. Use cache for better performance!")]
        [SerializeField] private bool _realtime = true;

        [SerializeField] private bool _trimSource = true;

        [Range(0,1)]
        [SerializeField] private float _blendDownScale = 0.9f;

        [Header("Attached to this sprite")]
        [SerializeField]
        private SpriteRenderer _target;

        [SerializeField]
        private Transform _rotationTransform;


        private Texture2D _finalTex;
        private Sprite _finalSprite;


        private void Awake()
        {
            Validate();
        }

        private int _angle;
        private int _lastAngle;
        private Quaternion _lastRotation;
        private float _fpsTimer = 0f;

        private bool _invokeRotate;


        public void Unlink()
        {
            _target = null;
            _rotationTransform = null;
        }

        private void LateUpdate()
        {
            Validate();

            if (!Application.isPlaying)
            {
                TryToAttach();
            }

            if (_sprite == null) return;

            CheckRotationAndRotate();

            UpdateCached();
            UpdateRealtime();

            if (_invokeRotate)
            {
                _invokeRotate = false;
                Rotate();
            }

            var pos = transform.position;
            _target.transform.position = pos + (Vector3)_rendererOffset;

            _rotationTransform.transform.position =
                pos;
        }

        private void TryToAttach()
        {
            var sr = GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                _sprite = sr.sprite;

                DestroyImmediate(sr);
            }

        }

        private void CheckRotationAndRotate()
        {
            _fpsTimer += Time.deltaTime;

            if (CheckRotationChanged())
            {
                _angle = CalculateAngle();

                if (_fpsTimer > (1f / _fps) && _lastAngle != _angle)
                {
                    _fpsTimer = 0f;
                    _lastAngle = _angle;

                    Rotate();
                }
            }
        }

        private int CalculateAngle()
        {
            return CalculateAngle(transform.rotation.eulerAngles.z);
        }

        private int CalculateAngle(float angle)
        {
            var realAngle = _angleOffset + angle;

            var value = (int)(Mathf.Round(realAngle / _angleStep) * _angleStep);

            while (value > 360)
            {
                value -= 360;
            }
            return value;
        }

        private bool CheckRotationChanged()
        {
            if (_target == null) return false;

            if (_lastRotation == transform.rotation) return false;

            UpdateTransform();

            return true;

        }

        public void UpdateTransform()
        {
            if (_target == null) return;
            if(_rotationTransform == null) return;

            _target.transform.rotation= Quaternion.identity;

            _lastRotation = transform.rotation;

            _rotationTransform.rotation = 
                Quaternion.Euler(
                    0,
                    0, 
                    CalculateAngle(_lastRotation.eulerAngles.z));
        }

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


            if (_rotationTransform == null)
            {
                _rotationTransform = new GameObject($"{gameObject.name}_Transform_PixelRot").transform;
                _rotationTransform.transform.SetParent(MKPixelRotContainer.Instance.transform);
                _rotationTransform.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            }

            if (_target == null)
            {
                var spriteRenderer = new GameObject($"{gameObject.name}_Graphics_PixelRot").AddComponent<SpriteRenderer>();
                spriteRenderer.transform.SetParent(MKPixelRotContainer.Instance.transform);
                spriteRenderer.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                _target = spriteRenderer;
                
                if(_sprite != null)
                    Rotate();
            }

            if (_sprite == null) return;

            var source = _sprite.texture;

            ValidateTexture(source);

        }

        private void OnValidate()
        {
            _invokeRotate = true;
        }

        private MKTextureData GetCopy(MKTextureData textureData)
        {
            var result = new MKTextureData(textureData.Width, textureData.Height);

            for (int i = 0; i < textureData.Length; i++)
            {
                result.Data[i] = textureData.Data[i];
            }

            return result;
        }

        private MKTextureData GetRotate(MKTextureData textureData, int size, int angle)
        {
            var skip = false;

            switch (angle)
            {
                case 360:
                case 0:
                {
                    textureData = GetCopy(textureData);

                    skip = true;
                    break;
                }
                case 90:
                    textureData = GetCopy(textureData);
                    MKTextureUtilites.Rotate90(textureData);
                    skip = true;
                    break;
                case 180:
                    textureData = GetCopy(textureData);
                    MKTextureUtilites.Rotate90(textureData);
                    MKTextureUtilites.Rotate90(textureData);
                    skip = true;
                    break;
                case 270:
                    textureData = GetCopy(textureData);
                    MKTextureUtilites.Rotate90(textureData);
                    MKTextureUtilites.Rotate90(textureData);
                    MKTextureUtilites.Rotate90(textureData);
                    skip = true;
                    break;
            }

            if (skip) return textureData;


            textureData = MKTextureUtilites.Scale2x(textureData);

            textureData = MKTextureUtilites.Scale2x(textureData);

            textureData = MKTextureUtilites.Scale2x(textureData);

            MKTextureUtilites.Rotate(textureData, angle);

            textureData = MKTextureUtilites.ScaleDown(textureData, size, size, _blendDownScale);

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
            var texture = new Texture2D(width, height)
            {
                alphaIsTransparency = true,
                filterMode = FilterMode.Point
            };

            return texture;
        }
   
        private Sprite MakeSprite(Texture2D texture, int x, int y, int w, int h, string name, float angle)
        {
            Vector2 pivot = Quaternion.Euler(0,0, angle) * new Vector2(_pivot.x-0.5f, _pivot.y-0.5f)*2;

            pivot = new Vector2(pivot.x/2+0.5F, pivot.y/2+0.5f);

            var sprite = Sprite.Create(
                   texture,
                   new Rect(x, y, w, h),
                   pivot,
                       _sprite.pixelsPerUnit);
            sprite.name = name;

            return sprite;
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (Time.frameCount != 0 && Time.renderedFrameCount != 0)//not loading scene
                {
                    if (_target != null)
                        DestroyImmediate(_target.gameObject);

                    if(_rotationTransform != null) 
                        DestroyImmediate(_rotationTransform.gameObject);

                }
            }
            
#else
            {
                if (_target != null)
                    Destroy(_target.gameObject);

                if (_rotationTransform != null)
                    Destroy(_rotationTransform.gameObject);
            }
#endif
        }
    }
}