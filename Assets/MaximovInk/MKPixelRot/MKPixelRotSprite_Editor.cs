using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk
{
#if UNITY_EDITOR
    using UnityEditor;
#endif

    public partial class MKPixelRotSprite
    {
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


#endif

        private void ValidateTexture(Texture2D texture)
        {
            if (!texture.isReadable)
            {
#if UNITY_EDITOR
                SetTextureReadable(AssetDatabase.GetAssetPath(texture));
#else
                Debug.LogError($"Texture is not readable! {texture.name}");
                return;
#endif
            }
        }

        private void RepaintScene()
        {

#if UNITY_EDITOR
            if (_target != null)
            {
                _target.gameObject.SetActive(false);
                _target.gameObject.SetActive(true);
            }


            SceneView.RepaintAll();
#endif


        }
        

    }
}
