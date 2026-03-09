using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One-shot editor setup script — run via the menu then delete this file.
/// </summary>
public static class LevelCompletedCanvasSetup
{
    [MenuItem("Tools/Setup Level Completed Canvas")]
    public static void Setup()
    {
        // ── Canvas ────────────────────────────────────────────────────────────
        GameObject canvasGO = GameObject.Find("LevelCompletedCanvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("LevelCompletedCanvas");
            Undo.RegisterCreatedObjectUndo(canvasGO, "Create LevelCompletedCanvas");
        }

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        if (canvas == null) canvas = Undo.AddComponent<Canvas>(canvasGO);
        canvas.renderMode = RenderMode.WorldSpace;

        if (canvasGO.GetComponent<CanvasScaler>() == null)
            Undo.AddComponent<CanvasScaler>(canvasGO);
        if (canvasGO.GetComponent<GraphicRaycaster>() == null)
            Undo.AddComponent<GraphicRaycaster>(canvasGO);

        // Size & position — place 1.5 m above Carpet_Interact (or origin)
        RectTransform rt = canvasGO.GetComponent<RectTransform>();
        rt.sizeDelta   = new Vector2(800, 250);
        rt.localScale  = new Vector3(0.002f, 0.002f, 0.002f);

        GameObject carpet = GameObject.Find("Carpet_Interact");
        if (carpet != null)
        {
            rt.position = carpet.transform.position + new Vector3(0f, 1.6f, 0.3f);
            rt.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            rt.position = new Vector3(0f, 1.6f, 0f);
        }

        // ── Background panel ──────────────────────────────────────────────────
        GameObject panelGO = canvasGO.transform.Find("Panel")?.gameObject;
        if (panelGO == null)
        {
            panelGO = new GameObject("Panel");
            Undo.RegisterCreatedObjectUndo(panelGO, "Create Panel");
            panelGO.transform.SetParent(canvasGO.transform, false);
        }

        Image panelImg = panelGO.GetComponent<Image>();
        if (panelImg == null) panelImg = Undo.AddComponent<Image>(panelGO);
        panelImg.color = new Color(0f, 0f, 0f, 0.82f);

        RectTransform panelRt = panelGO.GetComponent<RectTransform>();
        panelRt.anchorMin  = Vector2.zero;
        panelRt.anchorMax  = Vector2.one;
        panelRt.offsetMin  = Vector2.zero;
        panelRt.offsetMax  = Vector2.zero;

        // ── TextMeshPro label ─────────────────────────────────────────────────
        GameObject textGO = canvasGO.transform.Find("LevelCompletedText")?.gameObject;
        if (textGO == null)
        {
            textGO = new GameObject("LevelCompletedText");
            Undo.RegisterCreatedObjectUndo(textGO, "Create LevelCompletedText");
            textGO.transform.SetParent(canvasGO.transform, false);
        }

        TextMeshProUGUI tmp = textGO.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = Undo.AddComponent<TextMeshProUGUI>(textGO);
        tmp.text      = "Level Completed!\n\nPlease fill in the survey form before continuing.";
        tmp.fontSize  = 52;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;

        RectTransform textRt = textGO.GetComponent<RectTransform>();
        textRt.anchorMin  = Vector2.zero;
        textRt.anchorMax  = Vector2.one;
        textRt.offsetMin  = new Vector2(30, 20);
        textRt.offsetMax  = new Vector2(-30, -20);

        // ── Start inactive — script activates it at runtime ───────────────────
        canvasGO.SetActive(false);

        EditorUtility.SetDirty(canvasGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            canvasGO.scene);

        Debug.Log("[Setup] LevelCompletedCanvas configured and set inactive.");
        EditorUtility.DisplayDialog("Done",
            "LevelCompletedCanvas is ready.\n\nNow:\n" +
            "1. Select Wheel_B in the hierarchy.\n" +
            "2. On WheelTwoHandGrab, drag LevelCompletedCanvas into 'Level Completed Canvas'.",
            "OK");
    }
}
