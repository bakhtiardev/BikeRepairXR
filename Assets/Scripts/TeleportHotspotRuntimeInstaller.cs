using Oculus.Interaction.Locomotion;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures every TeleportInteractable in the active scene has a TeleportHotspotHandler attached so
/// Meta's hotspot building block can actually move the player rig.
/// </summary>
public static class TeleportHotspotRuntimeInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterSceneLoad()
    {
        SceneManager.sceneLoaded += (_, __) => AttachHandlers();
        AttachHandlers();
    }

    private static void AttachHandlers()
    {
        var interactables = Object.FindObjectsByType<TeleportInteractable>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        foreach (var interactable in interactables)
        {
            if (!interactable.TryGetComponent(out TeleportHotspotHandler handler))
            {
                handler = interactable.gameObject.AddComponent<TeleportHotspotHandler>();
            }

            handler.hideFlags &= ~HideFlags.NotEditable;
        }
    }
}
