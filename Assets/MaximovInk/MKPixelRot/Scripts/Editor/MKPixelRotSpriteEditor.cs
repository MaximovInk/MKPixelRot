using MaximovInk;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MKPixelRotSprite))]
public class MKPixelRotSpriteEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var sprite  = target as MKPixelRotSprite;

        if(GUILayout.Button("Make cache"))
        {
            sprite.GenerateRotationSheet();
        }

        

        base.OnInspectorGUI();
    }
}
