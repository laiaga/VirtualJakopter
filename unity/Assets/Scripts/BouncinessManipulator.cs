
using UnityEngine;
using System.Collections;

public class BouncinessManipulator : MonoBehaviour {

    public float bounciness = 0.2f;

	// Use this for initialization
	void Start () {
        Collider coll = GetComponent<Collider>();
        coll.material.bounciness = bounciness;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
