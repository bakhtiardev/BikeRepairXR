using System.Collections;
using UnityEngine;

public class WrenchCollisionHelper : MonoBehaviour
{
    private Collider[] wrenchColliders;

    private void Awake()
    {
        wrenchColliders = GetComponentsInChildren<Collider>();
    }

    public void IgnoreCollisionWithWheel(bool ignore)
    {
        GameObject wheel = GameObject.Find("Wheel_F");
        if (wheel == null) return;

        Collider[] wheelColliders = wheel.GetComponents<Collider>();

        foreach (var wrenchCol in wrenchColliders)
        {
            foreach (var wheelCol in wheelColliders)
            {
                if (wrenchCol != null && wheelCol != null)
                    Physics.IgnoreCollision(wrenchCol, wheelCol, ignore);
            }
        }
    }

    public IEnumerator ReenableWheelCollisionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        IgnoreCollisionWithWheel(false);
    }
}