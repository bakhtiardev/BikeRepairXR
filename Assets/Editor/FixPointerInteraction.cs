using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class FixPointerInteraction : Editor
{
    [MenuItem("Tools/Fix Pointer Interaction")]
    public static void Fix()
    {
        GameObject rightController = GameObject.Find("RightControllerAnchor");
        if (rightController == null)
        {
            Debug.LogError("RightControllerAnchor not found");
            return;
        }

        if (rightController.GetComponent<SimpleLaserPointer>() == null)
        {
            rightController.AddComponent<SimpleLaserPointer>();
            var lr = rightController.GetComponent<LineRenderer>();
            if (lr == null) lr = rightController.AddComponent<LineRenderer>();
            lr.startWidth = 0.01f;
            lr.endWidth = 0.01f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.red;
            lr.endColor = Color.red;
        }

        GameObject eventSystemObj = GameObject.Find("EventSystem");
        if (eventSystemObj != null)
        {
            var inputSysModule = eventSystemObj.GetComponent("InputSystemUIInputModule");
            if (inputSysModule != null) DestroyImmediate(inputSysModule);
            var standaloneModule = eventSystemObj.GetComponent<StandaloneInputModule>();
            if (standaloneModule != null) DestroyImmediate(standaloneModule);

            System.Type ovrInputModuleType = null;
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                var types = asm.GetTypes();
                foreach(var t in types) {
                    if (t.Name == "OVRInputModule") {
                        ovrInputModuleType = t;
                        break;
                    }
                }
                if (ovrInputModuleType != null) break;
            }

            if (ovrInputModuleType != null)
            {
                Component ovrModule = eventSystemObj.GetComponent(ovrInputModuleType);
                if (ovrModule == null)
                {
                    ovrModule = eventSystemObj.AddComponent(ovrInputModuleType);
                }

                SerializedObject so = new SerializedObject(ovrModule);
                var rayProp = so.FindProperty("rayTransform");
                if (rayProp != null) {
                    rayProp.objectReferenceValue = rightController.transform;
                }
                
                var clickProp = so.FindProperty("joyPadClickButton");
                if (clickProp != null) {
                    // Try to map to IndexTrigger enum which is usually value 13 in OVRInput.Button but maybe different. Let's just set the int directly or search
                    // OVRInput.Button.PrimaryIndexTrigger is 1 << 13, but the enum used in OVRInputModule is usually OVRInput.Button which is mapped.
                    // Let's just do reflection to avoid missing types
                    System.Type ovrInputType = null;
                    foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies()) {
                        ovrInputType = asm.GetType("OVRInput") ?? asm.GetType("GlobalNamespace.OVRInput") ?? asm.GetTypes().FirstOrDefault(t => t.Name == "OVRInput");
                        if (ovrInputType != null) break;
                    }
                    if (ovrInputType != null) {
                        System.Type buttonType = ovrInputType.GetNestedType("Button");
                        if (buttonType != null) {
                            var val = System.Enum.Parse(buttonType, "PrimaryIndexTrigger");
                            clickProp.enumValueFlag = System.Convert.ToInt32(val);
                        }
                    }
                }
                so.ApplyModifiedProperties();
                
                Debug.Log("Successfully attached and configured OVRInputModule.");
            }
            else
            {
                Debug.LogError("OVRInputModule type still not found!");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
