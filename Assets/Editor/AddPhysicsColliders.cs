using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

/// <summary>
/// Adds invisible physics colliders for the floor and tool table so dropped
/// objects (wrenches, wheel) don't fall through the garage geometry.
/// Run once via Tools > Add Physics Colliders, then delete this file.
/// </summary>
public static class AddPhysicsColliders
{
    [MenuItem("Tools/Add Physics Colliders (Floor + Table)")]
    public static void Add()
    {
        // ── Floor ──────────────────────────────────────────────────────────────
        // Carpet_Interact sits at Y=0.077, so the visual floor surface is ~0.
        // A large flat box covers the whole garage floor.
        // isTrigger = true so it never blocks the player's headset height;
        // FloorTrigger.cs handles selective reactions to wrenches and the detached wheel.
        CreateBox("PhysicsCollider_Floor",
            center:    new Vector3(2f, -0.025f, 2f),
            size:      new Vector3(20f, 0.05f, 20f),
            isTrigger: true);

        // ── Tool Table ─────────────────────────────────────────────────────────
        // AllenWrench 1 rests at Y=1.664 — table top is just below that (~1.62).
        // Sized to roughly cover a workshop bench surface.
        CreateBox("PhysicsCollider_Table",
            center:  new Vector3(2f, 1.595f, -3.14f),
            size:    new Vector3(2.5f, 0.05f, 1.2f));

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[AddPhysicsColliders] Floor and Table colliders added.");
        EditorUtility.DisplayDialog("Done",
            "Added:\n• PhysicsCollider_Floor\n• PhysicsCollider_Table\n\n" +
            "Both are invisible at runtime.\n" +
            "Adjust their size/position in the Scene view to match your garage exactly, then save the scene.",
            "OK");
    }

    private static void CreateBox(string name, Vector3 center, Vector3 size, bool isTrigger = false)
    {
        // Don't create duplicates if run twice
        if (GameObject.Find(name) != null)
        {
            Debug.Log($"[AddPhysicsColliders] '{name}' already exists — skipped.");
            return;
        }

        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
        go.transform.position = center;

        var col = Undo.AddComponent<BoxCollider>(go);
        col.size      = size;
        col.center    = Vector3.zero;
        col.isTrigger = isTrigger;
    }
}
