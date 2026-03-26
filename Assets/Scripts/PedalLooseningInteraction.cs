using UnityEngine;
using TMPro;

/// <summary>
/// Manages the pedal-loosening step using a small wrench.
///
/// INTERACTION FLOW
///   1. Player grabs the small wrench (grip trigger via WrenchProximityGrab).
///   2. Player brings the wrench tip into the yellow zone visible around the pedal axle.
///   3. While the wrench is inside the zone (grip still held = tool stabilised),
///      pressing the INDEX (front) trigger counts as one turn.
///   4. After <see cref="turnsRequired"/> presses the pedal bolt is marked loose.
///   5. On completion, <see cref="pedalGrab"/> is enabled so the pedal can be removed.
///
/// SCENE SETUP
///   1. Create an empty child GameObject of the pedal at the axle centre, e.g. "PedalBoltZone_L".
///   2. Attach THIS script to that child.
///   3. For the LEFT pedal leave this component ENABLED at scene start.
///      For the RIGHT pedal leave it DISABLED — PedalLevelController enables it automatically.
///   4. Assign the small wrench's WrenchProximityGrab to <see cref="wrench"/>.
///   5. Assign the PedalGrab component on this pedal to <see cref="pedalGrab"/>.
///   6. Create a World-Space Canvas near the pedal with a TextMeshProUGUI and assign it to
///      <see cref="counterText"/>.
///   7. Adjust <see cref="zoneRadius"/> and <see cref="wrenchTipOffset"/> to fit your model.
/// </summary>
public class PedalLooseningInteraction : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("The WrenchProximityGrab component on the small wrench.")]
    public WrenchProximityGrab wrench;

    [Tooltip("PedalGrab on this pedal — will be enabled once the pedal bolt is loose.")]
    public PedalGrab pedalGrab;

    [Tooltip("World-space TextMeshProUGUI that shows turn count to the player.")]
    public TextMeshProUGUI counterText;

    [Tooltip("The Canvas GameObject that hosts counterText — assigned directly so it can be toggled even when inactive.")]
    public GameObject counterCanvasObject;

    [Header("Zone Settings")]
    [Tooltip("Radius (metres) of the wrench-engagement sphere drawn around this GameObject (the pedal axle centre).")]
    public float zoneRadius = 0.12f;

    [Tooltip("Distance (metres) along the wrench's local forward axis from its root to the tip. " +
             "Used when WrenchProximityGrab.wrenchTip is not assigned.")]
    public float wrenchTipOffset = 0.06f;

    [Header("Bolt Settings")]
    public int turnsRequired = 8;

    [Header("Pedal Side")]
    [Tooltip("Which pedal this zone controls.")]
    public bool isLeftPedal = true;

    // ── State ─────────────────────────────────────────────────────────────────
    private int  _turns        = 0;
    private bool _boltLoosened = false;

    // ── Unity ─────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Awake runs even for disabled components, so this guarantees pedalGrab
        // starts disabled regardless of its Inspector state (important for the
        // right pedal whose PedalLooseningInteraction begins disabled).
        if (pedalGrab != null)
            pedalGrab.enabled = false;
    }

    private void Start()
    {
        if (pedalGrab != null)
            pedalGrab.enabled = false;

        UpdateCounterText();
    }

    private void OnEnable()
    {
        // Reset state every time we become active (right pedal re-enabled by controller).
        _turns        = 0;
        _boltLoosened = false;

        if (pedalGrab != null)
            pedalGrab.enabled = false;

        UpdateCounterText();
    }

    private void Update()
    {
        if (_boltLoosened) return;

        if (wrench == null) return;

        // Compute wrench tip world position.
        Vector3 tipPos = (wrench.wrenchTip != null)
            ? wrench.wrenchTip.position
            : wrench.transform.position + wrench.transform.forward * wrenchTipOffset;

        // Tip must be inside the engagement zone.
        if (Vector3.Distance(tipPos, transform.position) > zoneRadius) return;

        // Match bolt logic first. If HoldingController is missing (None),
        // fall back to active-controller trigger detection.
        bool indexDown;
        OVRInput.Controller holdingController = wrench.HoldingController;
        if (wrench.IsHeld && holdingController != OVRInput.Controller.None)
            indexDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, holdingController);
        else
            indexDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);

        if (indexDown)
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

        string pedalSide = isLeftPedal ? "LEFT" : "RIGHT";
        if (counterText != null)
            counterText.text = $"{pedalSide} Pedal Bolt loose!\nGrab the pedal and\ncarry it to the carpet.";

        if (pedalGrab != null)
            pedalGrab.enabled = true;

        Debug.Log("[PedalLooseningInteraction] Pedal bolt loosened — pedal grab enabled.");
    }

    private void UpdateCounterText()
    {
        string pedalSide = isLeftPedal ? "LEFT" : "RIGHT";
        if (counterText != null)
            counterText.text = $"{pedalSide} Pedal\nTurns: {_turns} / {turnsRequired}";
    }

    // ── Editor Gizmo ──────────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _boltLoosened ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
    }
}
