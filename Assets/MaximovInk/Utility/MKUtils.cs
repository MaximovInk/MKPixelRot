using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MaximovInk
{
    public static class MKUtils
    {
        public static void Invoke(this MonoBehaviour mb, Action f, float delay)
        {
            mb.StartCoroutine(InvokeRoutine(f, delay));
        }

        private static IEnumerator InvokeRoutine(System.Action f, float delay)
        {
            yield return new WaitForSeconds(delay);

            f();
        }

        private static IEnumerator InvokeRoutine(this MonoBehaviour mb, System.Action f, float delay)
        {
            yield return mb.StartCoroutine(WaitForRealTime(delay));

            f();
        }

        public static void InvokeAfterFrame(this MonoBehaviour mb, Action f)
        {
            mb.StartCoroutine(InvokeAfterFrameRoutine(f));
        }

        private static IEnumerator InvokeAfterFrameRoutine(System.Action f)
        {
            yield return new WaitForFixedUpdate();
            f();
        }

        public static void DestroyAllChildren(Transform target)
        {
            if (Application.isPlaying)
            {
                for (var i = target.childCount - 1; i >= 0; i--)
                {
                    UnityEngine.Object.Destroy(target.GetChild(i).gameObject);
                }
                return;
            }
            for (var i = target.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(target.GetChild(i).gameObject);
            }

        }

        public static IEnumerator WaitForRealTime(float delay)
        {
            while (true)
            {
                float pauseEndTime = Time.realtimeSinceStartup + delay;
                while (Time.realtimeSinceStartup < pauseEndTime)
                {
                    yield return 0;
                }
                break;
            }
        }

        /// <summary>
        /// Returns random index of a infinite probability distribution array
        /// </summary>
        /// <param name="weights"> weights</param>
        /// <returns></returns>
        public static int GetRandomWeightedIndex(float[] weights)
        {
            if (weights == null || weights.Length == 0) return -1;

            float weight;
            float sum = 0;
            int i;
            for (i = 0; i < weights.Length; i++)
            {
                weight = weights[i];

                if (float.IsPositiveInfinity(weight))
                {
                    return i;
                }
                else if (weight >= 0f && !float.IsNaN(weight))
                {
                    sum += weights[i];
                }
            }

            float r = Random.value;
            float s = 0f;

            for (i = 0; i < weights.Length; i++)
            {
                weight = weights[i];
                if (float.IsNaN(weight) || weight <= 0f) continue;

                s += weight / sum;
                if (s >= r) return i;
            }

            return -1;
        }


        /// <summary>
        /// Returns random index of a finite probability distribution array
        /// </summary>
        /// <param name="prob"></param>
        /// <returns></returns>
        public static int PickOne(float[] prob)
        {
            var index = 0;
            var r = Random.value;

            while (r > 0)
            {
                r -= prob[index];
                index++;
            }
            index--;

            index = Mathf.Clamp(index, 0, prob.Length);

            return index;
        }

        //https://forum.unity.com/threads/generating-sprites-dynamically-from-png-or-jpeg-files-in-c.343735/
        public static Sprite LoadNewSprite(string filePath, float pixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {

            // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

            var spriteTexture = LoadTexture(filePath);
            var newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), pixelsPerUnit, 0, spriteType);

            return newSprite;
        }

        public static Sprite ConvertTextureToSprite(Texture2D texture, float pixelsPerUnit = 100.0f, SpriteMeshType spriteType = SpriteMeshType.Tight)
        {
            // Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

            var newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), pixelsPerUnit, 0, spriteType);

            return newSprite;
        }

        public static Texture2D LoadTexture(string filePath)
        {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            if (File.Exists(filePath))
            {
                var fileData = File.ReadAllBytes(filePath);
                var tex2D = new Texture2D(2, 2);
                if (tex2D.LoadImage(fileData))           // Load the imagedata into the texture (size is set automatically)
                    return tex2D;                 // If data = readable -> return texture
            }
            return null;                     // Return null if load failed
        }

        public static string KiloFormat(this int num)
        {
            var mult = 1;

            if (num < 0)
            {
                num *= -1;
                mult = -1;
            }

            if (num >= 100000000)
                return (num / 1000000D * mult).ToString("0,#M");

            if (num >= 1000000)
                return (num / 1000000D * mult).ToString("0.##M");

            if (num >= 100000)
                return (num / 1000D * mult).ToString("0,#K");

            if (num >= 10000)
                return (num / 1000D * mult).ToString("0.##") + "K";

            return (num * mult).ToString("#,0");
        }
    }
}
