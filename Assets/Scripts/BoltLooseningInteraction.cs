using UnityEngine;
using TMPro;

/// <summary>
/// Manages the bolt-loosening step of removing the back wheel.
///
/// INTERACTION FLOW
///   1. Player grabs the AllenWrench (grip / side trigger via WrenchProximityGrab).
///   2. Player brings the wrench tip into the yellow zone visible around the wheel axle.
///   3. While the wrench is inside the zone (grip still held = tool stabilised),
///      pressing the INDEX (front) trigger counts as one turn.
///   4. After <see cref="turnsRequired"/> presses the bolt is marked loose.
///   5. The counter <see cref="counterText"/> feeds back progress to the player.
///   6. On completion, <see cref="wheelGrab"/> is enabled so the wheel can be removed.
///
/// SCENE SETUP
///   1. Create an empty child GameObject of the back wheel at the axle center, e.g. "BoltZone".
///   2. Attach THIS script to "BoltZone".
///   3. Assign the AllenWrench 1 GameObject's <see cref="WrenchProximityGrab"/> to <see cref="wrench"/>.
///   4. Assign the <see cref="WheelTwoHandGrab"/> component on the back wheel to <see cref="wheelGrab"/>.
///   5. Create a World-Space Canvas near the wheel with a TextMeshProUGUI and assign it to <see cref="counterText"/>.
///   6. Adjust <see cref="zoneRadius"/> and <see cref="wrenchTipOffset"/> to fit your model scale.
/// </summary>
public class BoltLooseningInteraction : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("The WrenchProximityGrab component on AllenWrench 1.")]
    public WrenchProximityGrab wrench;

    [Tooltip("WheelTwoHandGrab on the back wheel — will be enabled once bolt is loose.")]
    public WheelTwoHandGrab wheelGrab;

    [Tooltip("World-space TextMeshProUGUI that shows turn count to the player.")]
    public TextMeshProUGUI counterText;

    [Header("Zone Settings")]
    [Tooltip("Radius of the wrench-engagement sphere drawn around this GameObject (the axle center).")]
    public float zoneRadius = 0.15f;

    [Tooltip("Distance (metres) along the wrench's local forward axis from its root to the tip. " +
             "Used when WrenchProximityGrab.wrenchTip is not assigned.")]
    public float wrenchTipOffset = 0.06f;

    [Header("Bolt Settings")]
    public int turnsRequired = 10;

    // ── State ─────────────────────────────────────────────────────────────────
    private int  _turns       = 0;
    private bool _boltLoosened = false;

    // ── Unity ─────────────────────────────────────────────────────────────────
    private void Start()
    {
        // Wheel grab is locked until bolt is fully loose
        if (wheelGrab != null)
        {
            wheelGrab.enabled = false;

            // If a previous play session left the wheel un-parented (ISDK GrabFreeTransformer
            // can un-parent its target), restore it under the bike root so snap-back works.
            if (wheelGrab.transform.parent == null)
            {
                Transform bikeRoot = transform.parent; // BoltZone → Wheel_B → Bike; here we're on BoltZone
                if (bikeRoot != null)
                {
                    Vector3    wp = wheelGrab.transform.position;
                    Quaternion wr = wheelGrab.transform.rotation;
                    wheelGrab.transform.SetParent(bikeRoot, worldPositionStays: false);
                    wheelGrab.transform.position = wp;
                    wheelGrab.transform.rotation = wr;
                    Debug.Log("[BoltLooseningInteraction] Wheel_B was a root — re-parented under " + bikeRoot.name);
                }
            }
        }

        // Re-activate the counter canvas in case a previous play session saved it inactive
        // (WheelTwoHandGrab hides it on detach — this ensures a clean start every time).
        if (counterText != null && !counterText.canvas.gameObject.activeSelf)
            counterText.canvas.gameObject.SetActive(true);

        UpdateCounterText();
    }

    private void Update()
    {
        if (_boltLoosened) return;

        // Wrench must be in hand
        if (wrench == null || !wrench.IsHeld) return;

        // Compute wrench tip world position
        Vector3 tipPos = (wrench.wrenchTip != null)
            ? wrench.wrenchTip.position
            : wrench.transform.position + wrench.transform.forward * wrenchTipOffset;

        // Must be inside the zone sphere
        if (Vector3.Distance(tipPos, transform.position) > zoneRadius) return;

        // Count index trigger down-events on the hand that holds the wrench.
        // The grip (side trigger) is already held — that IS the stabilisation.
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, wrench.HoldingController))
        {
            _turns++;
            UpdateCounterText();

            if (_turns >= turnsRequired)
                LooseBolt();
        }
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    private void LooseBolt()
    {
        _boltLoosened = true;

        if (counterText != null)
            counterText.text = "Bolt loose!\nGrab wheel with both hands.";

        if (wheelGrab != null)
            wheelGrab.enabled = true;

        Debug.Log("[BoltLooseningInteraction] Bolt fully loosened — wheel grab enabled.");
    }

    private void UpdateCounterText()
    {
        if (counterText != null)
            counterText.text = $"Turns: {_turns} / {turnsRequired}";
    }

    // ── Editor Gizmo ──────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _boltLoosened ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
    }
}
