using UnityEngine;
using System.Collections;

public class WingBehavior : MonoBehaviour {
    public float rotationSpeed = 1000.0f;
    public GameObject parrot = null;

    private Vector3 rotation;
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(Vector3.forward*rotationSpeed, Space.Self);
        if(parrot != null)
        {
            rotation = transform.rotation.eulerAngles;
            rotation.x = parrot.transform.rotation.eulerAngles.x;
            rotation.y = parrot.transform.rotation.eulerAngles.y;
        }
	}
}
