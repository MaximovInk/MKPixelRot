using MaximovInk;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
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

        GUILayout.Space(10);

        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label($"Sprite angle {sprite.Angle}");
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (sprite.Renderer != null)
        {
            sprite.Renderer.color = EditorGUILayout.ColorField("Color", sprite.Renderer.color);
            sprite.Renderer.sharedMaterial = (Material)EditorGUILayout.ObjectField("Material", sprite.Renderer.sharedMaterial, typeof(Material), false);

            var sortingLayerNames = GetSortingLayerNames();

            int index = Mathf.Max(0, Array.IndexOf(sortingLayerNames, sprite.Renderer.sortingLayerName));
            index = EditorGUILayout.Popup("Layer", index, sortingLayerNames);
            sprite.Renderer.sortingLayerName = sortingLayerNames[index];

            sprite.Renderer.sortingOrder = EditorGUILayout.IntField("Order", sprite.Renderer.sortingOrder);

        }

        GUILayout.Space(10);

        base.OnInspectorGUI();
    }


    public string[] GetSortingLayerNames()
    {
        var internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        var sortingLayers = (string[])sortingLayersProperty.GetValue(null, new object[0]);
        return sortingLayers;
    }

    private void ProgressBar(float val, string label)
    {
        Rect r = EditorGUILayout.BeginVertical();
        EditorGUI.ProgressBar(r, val, label);
        GUILayout.Space(18);
        EditorGUILayout.EndVertical();
    }

    /*
     TODO: Make spriteRenderer hidden, show only parent object with script
      private void OnSceneGUI()
    {
        var sceneView = SceneView.currentDrawingSceneView;
        var objectList = FindObjectsOfType<SpriteRenderer>().ToList();

        switch (Event.current.type)
        {
            case EventType.MouseEnterWindow:
                objectList.ForEach(x => x.hideFlags = x.hideFlags & ~HideFlags.HideInHierarchy);
                break;
            case EventType.MouseLeaveWindow:
                objectList.ForEach(x => x.hideFlags = x.hideFlags | HideFlags.HideInHierarchy);
                break;
        }

        if(Selection.activeGameObject != null)
        Debug.Log(Selection.activeGameObject.name);

        var sr = Selection.activeGameObject.GetComponent<SpriteRenderer>();

        if (sr.gameObject.name.Contains("PixelRot"))
        {
            Selection.activeGameObject = sr.transform.parent.gameObject;
        }
    }*/
}
