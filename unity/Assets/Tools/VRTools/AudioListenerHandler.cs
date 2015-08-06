using UnityEngine;
using System.Collections;

public class AudioListenerHandler : MonoBehaviour {

    /// <summary>
    /// Disable audio listener component on all cameras (recommended)
    /// </summary>
    public bool DisableAllAudioListeners = true;

    VRManagerScript vrManager;
    GameObject headNode;

    void Awake()
    {
        vrManager = FindObjectOfType<VRManagerScript>();
        if (vrManager.TemplateCamera != null)
        {
            AudioListener audioListener = vrManager.TemplateCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = false;
                Debug.Log("[MVRTools] Disabeling AudioListener component on template camera : " + vrManager.TemplateCamera.name, this);
            }
        }
        if (DisableAllAudioListeners)
        {
            foreach (AudioListener audioListener in FindObjectsOfType<AudioListener>())
            {
                if (audioListener != null)
                {
                    audioListener.enabled = false;
                    Debug.Log("[MVRTools] Disabeling AudioListener component on game object : " + audioListener.name, this);
                }
            }
        }
    }

	// Use this for initialization
	void Start () {
        headNode = GameObject.Find("HeadNode");
        if (headNode != null)
        {
            if (headNode.GetComponent<AudioListener>() == null)
            {
                headNode.AddComponent<AudioListener>();
                Debug.Log("[MVRTools] Adding AudioListener component to HeadNode", this);
            }
            headNode.GetComponent<AudioListener>().enabled = true;
            Debug.Log("[MVRTools] Activating AudioListener component on HeadNode", this);
        }
	}

}
