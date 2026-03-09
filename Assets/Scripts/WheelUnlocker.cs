using TMPro;
using UnityEngine;
using System.Collections;

public class WheelUnlocker : MonoBehaviour
{
    [Header("Unlock Settings")]
    public int requiredTurns = 5;

    [Header("References")]
    public Rigidbody wheelRigidbody;
    public MonoBehaviour grabbableScript;

    [Header("UI")]
    public GameObject unlockCounterCanvas;
    public TMP_Text counterText;

    [Header("State")]
    public int currentTurns = 0;
    public bool wrenchInPlace = false;
    public bool wheelUnlocked = false;

    private WrenchTool activeWrench;

    private void Start()
    {
        LockWheel();
        UpdateCounterUI();

        if (unlockCounterCanvas != null)
            unlockCounterCanvas.SetActive(false);
    }

    public void SetWrenchInPlace(bool inPlace, WrenchTool wrench)
    {
        wrenchInPlace = inPlace;

        if (inPlace)
            activeWrench = wrench;
        else if (activeWrench == wrench)
            activeWrench = null;

        if (!wheelUnlocked && unlockCounterCanvas != null)
            unlockCounterCanvas.SetActive(inPlace);
    }

    public void RegisterTurn()
    {
        Debug.Log("RegisterTurn called | unlocked=" + wheelUnlocked +
              " | wrenchInPlace=" + wrenchInPlace +
              " | activeWrench=" + (activeWrench != null) +
              " | isHeld=" + (activeWrench != null && activeWrench.isHeld) +
              " | currentTurns=" + currentTurns + "/" + requiredTurns);

        if (wheelUnlocked)
        {
            Debug.Log("Turn ignored: wheel already unlocked");
            return;
        }

        if (!wrenchInPlace)
        {
            Debug.Log("Turn ignored: wrench not in place");
            return;
        }

        if (activeWrench == null || !activeWrench.isHeld)
        {
            Debug.Log("Turn ignored: wrench not held");
            return;
        }

        currentTurns++;
        Debug.Log("Wheel turn count: " + currentTurns + " / " + requiredTurns);

        UpdateCounterUI();

        if (currentTurns >= requiredTurns)
        {
            UnlockWheel();
        }
    }

    private void UpdateCounterUI()
    {
        if (counterText != null)
        {
            if (counterText != null)
                counterText.text = "Loosen axle\n" + currentTurns + " / " + requiredTurns;
        }
    }

    private void LockWheel()
    {
        if (wheelRigidbody != null)
        {
            wheelRigidbody.linearVelocity = Vector3.zero;
            wheelRigidbody.angularVelocity = Vector3.zero;
            wheelRigidbody.isKinematic = true;
            wheelRigidbody.useGravity = false;
        }

        if (grabbableScript != null)
        {
            grabbableScript.enabled = false;
        }
    }

    private void UnlockWheel()
    {
        wheelUnlocked = true;
        Debug.Log("Front wheel unlocked. You can now remove it.");

        if (wheelRigidbody != null)
        {
            wheelRigidbody.linearVelocity = Vector3.zero;
            wheelRigidbody.angularVelocity = Vector3.zero;
            wheelRigidbody.isKinematic = true;
            wheelRigidbody.useGravity = false;
        }

        if (grabbableScript != null)
        {
            grabbableScript.enabled = true;
        }

        if (counterText != null)
        {
            counterText.text = "Wheel unlocked.\nYou can grab it now.";
        }

        if (unlockCounterCanvas != null)
            StartCoroutine(HideCanvasAfterDelay());
    }

    private IEnumerator HideCanvasAfterDelay()
    {
        // canvas still visible for a couple of seconds; then it is disabled
        yield return new WaitForSeconds(10f);

        if (unlockCounterCanvas != null)
            unlockCounterCanvas.SetActive(false);
    }
}