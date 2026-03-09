using UnityEngine;
using UnityEngine.InputSystem;

public class WrenchTool : MonoBehaviour
{
    [Header("Input")]
    public Key turnKey = Key.Backspace;

    [Header("Hold Detection")]
    public bool isHeld = false;

    private WheelUnlocker currentWheel;

    private void Update()
    {
        bool pressed =
            OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch) ||
            OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch);

        if (pressed)
        {
            Debug.Log("TURN BUTTON PRESSED (A/X)");
        }

        if (isHeld && currentWheel != null && pressed)
        {
            Debug.Log("Registering turn...");
            currentWheel.RegisterTurn();
        }
    }

    public void SetHeld(bool held)
    {
        isHeld = held;
        Debug.Log("Wrench isHeld = " + isHeld);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Wrench entered trigger: " + other.name);

        WheelUnlocker wheel = other.GetComponentInParent<WheelUnlocker>();
        if (wheel != null)
        {
            currentWheel = wheel;
            wheel.SetWrenchInPlace(true, this);
            Debug.Log("Wrench is in place");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Wrench exited trigger: " + other.name);

        WheelUnlocker wheel = other.GetComponentInParent<WheelUnlocker>();
        if (wheel != null && wheel == currentWheel)
        {
            wheel.SetWrenchInPlace(false, this);
            currentWheel = null;
        }
    }
}