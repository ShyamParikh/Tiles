using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class HierarchySectionHeader
{
    static HierarchySectionHeader()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
    }

    static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.EntityIdToObject(instanceID) as GameObject;

        if (gameObject != null && gameObject.name.StartsWith("1.", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.darkRed);

            EditorGUI.LabelField(selectionRect, gameObject.name.Replace("1.", "").ToUpperInvariant(), new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }

        else if (gameObject != null && gameObject.name.StartsWith("2.", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.darkGreen);

            EditorGUI.LabelField(selectionRect, gameObject.name.Replace("2.", "").ToUpperInvariant(), new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }

        else if (gameObject != null && gameObject.name.StartsWith("3.", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.darkBlue);

            EditorGUI.LabelField(selectionRect, gameObject.name.Replace("3.", "").ToUpperInvariant(), new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }

        else if (gameObject != null && gameObject.name.StartsWith("4.", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.grey);

            EditorGUI.LabelField(selectionRect, gameObject.name.Replace("4.", "").ToUpperInvariant(), new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }

        else if (gameObject != null && gameObject.name.StartsWith("5.", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.black);

            EditorGUI.LabelField(selectionRect, gameObject.name.Replace("5.", "").ToUpperInvariant(), new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }

        else if (gameObject != null && gameObject.name.StartsWith("6.", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.darkOrange);

            EditorGUI.LabelField(selectionRect, gameObject.name.Replace("6.", "").ToUpperInvariant(), new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }

        else if (gameObject != null && gameObject.name.StartsWith("7.", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.darkMagenta);

            EditorGUI.LabelField(selectionRect, gameObject.name.Replace("7.", "").ToUpperInvariant(), new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }

        else if (gameObject != null && gameObject.name.StartsWith("8.", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.darkCyan);

            EditorGUI.LabelField(selectionRect, gameObject.name.Replace("8.", "").ToUpperInvariant(), new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            });
        }
    }
}