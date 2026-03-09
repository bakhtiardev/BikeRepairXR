using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot editor script — wires LevelCompletedCanvas into Wheel_B's WheelTwoHandGrab.
/// Delete this file after running.
/// </summary>
public static class WireupLevelCompletedCanvas
{
    [MenuItem("Tools/Wireup Level Completed Canvas Reference")]
    public static void Wireup()
    {
        // Find Wheel_B anywhere in the hierarchy — include inactive objects/components
        WheelTwoHandGrab[] all = Object.FindObjectsByType<WheelTwoHandGrab>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (all.Length == 0)
        {
            EditorUtility.DisplayDialog("Error", "No WheelTwoHandGrab found in scene!", "OK");
            return;
        }

        // GameObject.Find doesn't find inactive objects — search all scene roots instead
        GameObject levelCanvas = null;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "LevelCompletedCanvas") { levelCanvas = root; break; }
        }
        if (levelCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "LevelCompletedCanvas not found in scene!", "OK");
            return;
        }

        foreach (var grab in all)
        {
            Undo.RecordObject(grab, "Wire LevelCompletedCanvas");
            grab.levelCompletedCanvas = levelCanvas;
            EditorUtility.SetDirty(grab);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            all[0].gameObject.scene);

        Debug.Log($"[Wireup] levelCompletedCanvas wired on {all.Length} WheelTwoHandGrab component(s).");
        EditorUtility.DisplayDialog("Done",
            $"Wired LevelCompletedCanvas into {all.Length} WheelTwoHandGrab component(s).\n\nYou can now save the scene.",
            "OK");
    }
}
