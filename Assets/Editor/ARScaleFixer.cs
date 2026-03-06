using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ARScaleFixer : EditorWindow
{
    [MenuItem("Tools/AR Scale Fixer/Print Bounds")]
    static void PrintBounds()
    {
        string[] names = {
            "Low-Poly Bicycle # 5 - gravity variant",
            "Models/interior",
            "Models/bed1",
            "Models/bed2",
            "Models/glass_table",
            "Models/little_glass_table",
            "Models/tv1",
            "Models/printer",
            "Models/screen",
            "Models/tumba_fur",
            "Models/sek1",
            "Models/sek2",
            "Models/sek3",
            "Models/sek4",
            "Models/speaker"
        };

        foreach (var path in names)
        {
            GameObject go = GameObject.Find(path);
            if (go == null) { Debug.Log($"NOT FOUND: {path}"); continue; }

            Bounds b = GetWorldBounds(go);
            Debug.Log($"[BOUNDS] {path} | size: {b.size.x:F3} x {b.size.y:F3} x {b.size.z:F3} | center: {b.center}");
        }
    }

    static Bounds GetWorldBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    // Target real-world sizes in meters
    static readonly Dictionary<string, Vector3> targetSizes = new Dictionary<string, Vector3>
    {
        { "Low-Poly Bicycle # 5 - gravity variant", new Vector3(1.8f, 1.1f, 0.6f) },
        { "Models/bed1",           new Vector3(2.0f, 0.55f, 1.4f) },
        { "Models/bed2",           new Vector3(2.0f, 0.55f, 1.4f) },
        { "Models/glass_table",    new Vector3(0.6f, 0.55f, 0.6f) },
        { "Models/little_glass_table", new Vector3(0.45f, 0.5f, 0.45f) },
        { "Models/tumba_fur",      new Vector3(0.5f, 0.5f, 0.5f) },
        { "Models/tv1",            new Vector3(1.2f, 0.7f, 0.08f) },
        { "Models/printer",        new Vector3(0.42f, 0.2f, 0.35f) },
        { "Models/screen",         new Vector3(0.55f, 0.45f, 0.07f) },
        { "Models/sek1",           new Vector3(0.8f, 0.8f, 0.8f) },
        { "Models/sek2",           new Vector3(0.8f, 0.8f, 0.8f) },
        { "Models/sek3",           new Vector3(0.8f, 0.8f, 0.8f) },
        { "Models/sek4",           new Vector3(0.8f, 0.8f, 0.8f) },
        { "Models/speaker",        new Vector3(0.25f, 0.45f, 0.25f) },
    };

    [MenuItem("Tools/AR Scale Fixer/Apply Realistic AR Scales")]
    static void ApplyARScales()
    {
        // First fix the room interior to represent a real room ~6x3x6m
        // interior scale is ~16 - we need to know mesh native size
        // The child Plane001 has scale 0.0254 suggesting inches. Interior is likely in inches too.
        // 16 * native_size_in_model_units = world_meters
        // We'll scale it so the room fits ~6x3x6 meters

        // Fix interior room shell - target: ~6m wide, 3m tall, 6m deep
        // Current scale 15.93, 17.85, 15.93 → keep proportions, just target 3m height
        // height ratio: 3.0 / 17.85 = 0.168
        // But actually the current scene works visually - let's measure & fit properly.

        // Step 1: Fix the Models group to be a proper scale
        // All furniture is positioned relative to Models group at scale 1
        // The room interior uses scale ~16 which inflates the mesh to room size
        // If interior mesh = 1 inch native, scale 16 = 16 inches ≈ 0.4m - too small
        // If interior mesh = 1 foot native (0.3048m), scale 16 * 0.3048 ≈ 4.9m - reasonable for room!
        // So room interior: X≈5m, Y≈5.4m(too tall), Z≈5m. Target: X=6, Y=3, Z=6
        // New scale: 16/17.85 * 3.0 ceiling target: scaleY = 15.93 * (3.0/5.4) ≈ 8.85

        // Let's measure by printing and then computing scale factor
        // For now apply known-good values based on analysis:

        foreach (var kvp in targetSizes)
        {
            GameObject go = GameObject.Find(kvp.Key);
            if (go == null) { Debug.LogWarning($"NOT FOUND: {kvp.Key}"); continue; }

            Bounds b = GetWorldBounds(go);
            if (b.size == Vector3.zero) { Debug.LogWarning($"Zero bounds: {kvp.Key}"); continue; }

            Vector3 target = kvp.Value;
            Vector3 currentScale = go.transform.localScale;
            Vector3 currentSize = b.size;

            // Use the largest axis to determine dominant scale factor
            float currentMaxSize = Mathf.Max(currentSize.x, currentSize.y, currentSize.z);
            float targetMaxSize = Mathf.Max(target.x, target.y, target.z);
            float scaleFactor = targetMaxSize / currentMaxSize;

            Vector3 newScale = currentScale * scaleFactor;
            go.transform.localScale = newScale;

            Debug.Log($"[SCALED] {kvp.Key}: {currentSize.x:F3}x{currentSize.y:F3}x{currentSize.z:F3} → target {target.x}x{target.y}x{target.z} | factor: {scaleFactor:F4} | newScale: {newScale}");
        }

        // Fix interior room: keep as-is (it defines the room shell, already scene-proportioned)
        // Just adjust Y to bring ceiling to realistic 2.8m
        FixInteriorRoom();

        EditorUtility.SetDirty(GameObject.Find("Models"));
        EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("[AR Scale Fixer] Done! Save the scene to persist.");
    }

    static void FixInteriorRoom()
    {
        GameObject interior = GameObject.Find("Models/interior");
        if (interior == null) return;

        Bounds b = GetWorldBounds(interior);
        Debug.Log($"[INTERIOR] current world size: {b.size}");

        // Target a room that's ~6m wide, 2.8m tall, 6m deep
        Vector3 currentScale = interior.transform.localScale;
        float targetHeight = 2.8f;
        float scaleFactor = targetHeight / b.size.y;
        interior.transform.localScale = currentScale * scaleFactor;

        Debug.Log($"[INTERIOR] Rescaled by {scaleFactor:F4}. New scale: {interior.transform.localScale}");
    }
}
