using UnityEngine;
using System.Collections.Generic;

using System;

using System.Threading;





namespace MaximovInk
{
    [ExecuteAlways]
    [SerializeField]
    public partial class MKPixelRotSprite : MonoBehaviour
    {
        [SerializeField] private Sprite _sprite;

        [SerializeField] private float _fps = 12f;

        [Range(1,180)]
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

        private Thread _realtimeThread;

        private void Update()
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
        }

        private float CalculateAngle()
        {
            return Mathf.Round(transform.rotation.eulerAngles.z / _angleStep) * _angleStep;
        }

        private bool CheckRotationChanged()
        {
            if (_lastRotation != transform.rotation)
            {
                _target.transform.position = transform.position;
                _target.transform.rotation = Quaternion.identity;
                _lastRotation = transform.rotation;

                return true;
            }

            return false;
        }

        [Header("Debug (do not changing)")]
        [SerializeField] private float _angle;

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

                _target = renderer;

                Rotate();
            }

            var source = _sprite.texture;

            ValidateTexture(source);

        }

        private void OnValidate()
        {
            Rotate();
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

            if (!skip)
            {
                textureData = MKTextureUtilites.Scale2x(textureData);
                scaledSize *= 2;

                textureData = MKTextureUtilites.Scale2x(textureData);
                scaledSize *= 2;

                textureData = MKTextureUtilites.Scale2x(textureData);
                scaledSize *= 2;

                if (angle == 90)
                {
                    MKTextureUtilites.Rotate90(textureData);
                }
                else if (angle == 180)
                {
                    MKTextureUtilites.Rotate90(textureData);
                    MKTextureUtilites.Rotate90(textureData);
                }
                else if (angle == 270)
                {
                    MKTextureUtilites.Rotate90(textureData);
                    MKTextureUtilites.Rotate90(textureData);
                    MKTextureUtilites.Rotate90(textureData);

                }
                else

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

        public bool IsBusy()
        {
            return _cacheThread != null && _cacheThread.IsAlive;

        }
    }
}