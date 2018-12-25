using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (Input.GetKey("left"))
        {
            Camera.main.transform.RotateAround(Vector3.zero, Vector3.up, 50 * Time.deltaTime);
        }
        else if (Input.GetKey("right"))
        {
            Camera.main.transform.RotateAround(Vector3.zero, Vector3.up, -50 * Time.deltaTime);
        }
    }
}
