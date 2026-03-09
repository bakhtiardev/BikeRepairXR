using UnityEngine;

public class InteractionBikeScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OVRSceneManager scneMng = FindObjectOfType<OVRSceneManager>();

        if (scneMng != null)
        {
            scneMng.RequestSceneCapture();
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
