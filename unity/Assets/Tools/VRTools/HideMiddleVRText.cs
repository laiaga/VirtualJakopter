using UnityEngine;
using System.Collections;

public class HideMiddleVRText : MonoBehaviour {


    GUIText middleVRGUIText;

	// Use this for initialization
	void Start () {
	    if (VRTools.IsClient())
        {
            GameObject GuiTextGameObject = GameObject.Find("__");
            if (GuiTextGameObject != null)
                middleVRGUIText = GuiTextGameObject.GetComponent<GUIText>();
            if (middleVRGUIText != null)
                middleVRGUIText.enabled = false;
        }
	}

    public void Update()
    {
        if (VRTools.GetInstance().GetKeyDown(KeyCode.P))
        {
            ToggleGUITexts();
        }

    }

    void ToggleGUITexts()
    {
        if (middleVRGUIText != null)
            middleVRGUIText.enabled = !middleVRGUIText.enabled;
    }

}
