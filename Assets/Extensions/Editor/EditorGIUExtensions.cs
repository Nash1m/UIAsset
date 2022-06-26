using UnityEditor;
using UnityEngine;

public static class EditorGIUExtensions
{
    public static bool ImagedButton(string path, GUIStyle style = null, params GUILayoutOption[] options)
    {
        style ??= GUI.skin.button;

        var texture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(path);
        return GUILayout.Button(texture, style, options);
    }
}
