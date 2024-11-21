using MaximovInk;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MKPixelRotSprite))]
public class MKPixelRotSpriteEditor : Editor
{

    private bool _invokeRepaint;

    public override void OnInspectorGUI()
    {
        var sprite  = target as MKPixelRotSprite;

        if(GUILayout.Button("Make cache"))
        {
            sprite.GenerateRotationSheet();
        }

        if (GUILayout.Button("Stop all"))
        {
            sprite.StopAllThreads();
        }

        if (sprite.IsBusy())
        {
            GUILayout.Space(10);
            ProgressBar(sprite.CachingState, "Generation..");
            GUILayout.Space(10);
            GUILayout.Space(10);

            if (sprite.CachingState < 0.9f) {
                Repaint();
               
            }
            _invokeRepaint = true;


            return;
        }

        if (_invokeRepaint)
        {
            _invokeRepaint = false;
            sprite.Rotate();
            SceneView.RepaintAll();
            EditorUtility.SetDirty(sprite);
            
        }


        base.OnInspectorGUI();
    }

    private void ProgressBar(float val, string label)
    {
        Rect r = EditorGUILayout.BeginVertical();
        EditorGUI.ProgressBar(r, val, label);
        GUILayout.Space(18);
        EditorGUILayout.EndVertical();
    }
}
