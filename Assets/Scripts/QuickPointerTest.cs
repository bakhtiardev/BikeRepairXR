using UnityEngine;
using UnityEngine.EventSystems;

public class QuickPointerTest : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"CLICK PROBE: {name} pointerId={eventData.pointerId}");
    }
}
