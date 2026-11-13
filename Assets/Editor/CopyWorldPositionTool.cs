#if UNITY_EDITOR
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class CopyWorldPositionTool
{
    [MenuItem("CONTEXT/Transform/Copy World Position Only")]
    private static void CopyWorldPosition(MenuCommand command)
    {
        Transform target = command.context as Transform;

        if (target == null)
            return;

        Vector3 pos = target.position;

        EditorGUIUtility.systemCopyBuffer =
            $"{pos.x.ToString(CultureInfo.InvariantCulture)}, " +
            $"{pos.y.ToString(CultureInfo.InvariantCulture)}, " +
            $"{pos.z.ToString(CultureInfo.InvariantCulture)}";

        Debug.Log($"Copied World Position: {EditorGUIUtility.systemCopyBuffer}");
    }

    [MenuItem("CONTEXT/Transform/Paste World Position Only")]
    private static void PasteWorldPosition(MenuCommand command)
    {
        Transform target = command.context as Transform;

        if (target == null)
            return;

        string text = EditorGUIUtility.systemCopyBuffer;

        if (!TryParseVector3(text, out Vector3 pos))
        {
            Debug.LogWarning($"Clipboard does not contain a valid Vector3 position: {text}");
            return;
        }

        Undo.RecordObject(target, "Paste World Position");

        target.position = pos;

        EditorUtility.SetDirty(target);

        PrefabUtility.RecordPrefabInstancePropertyModifications(target);

        Debug.Log($"Pasted World Position to {target.name}: {pos}");
    }

    private static bool TryParseVector3(string text, out Vector3 result)
    {
        result = Vector3.zero;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        // "(1, 2, 3)", "1, 2, 3", "Vector3(1, 2, 3)" 같은 형식 대응
        MatchCollection matches = Regex.Matches(
            text,
            @"[-+]?\d*\.?\d+(?:[eE][-+]?\d+)?"
        );

        if (matches.Count < 3)
            return false;

        bool parsedX = float.TryParse(
            matches[0].Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float x
        );

        bool parsedY = float.TryParse(
            matches[1].Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float y
        );

        bool parsedZ = float.TryParse(
            matches[2].Value,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float z
        );

        if (!parsedX || !parsedY || !parsedZ)
            return false;

        result = new Vector3(x, y, z);
        return true;
    }
}
#endif