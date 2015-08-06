using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour {
    public float lookSpeed = 5.0f;
    public float verticalRotation = 0.0f;
    public float horizontalRotation = 0.0f;
    public float verticalRange = 60.0f;

	// Use this for initialization
	void Start () {
        Cursor.visible = false;
    }
	
	// Update is called once per frame
	void Update () {

        horizontalRotation = Input.GetAxis("Mouse X") * lookSpeed;
        transform.Rotate(0, horizontalRotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * lookSpeed;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalRange, verticalRange);

        Camera.main.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
	}
}
