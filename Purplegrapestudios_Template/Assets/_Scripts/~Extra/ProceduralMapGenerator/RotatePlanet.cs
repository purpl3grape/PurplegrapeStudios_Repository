using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePlanet : MonoBehaviour {

	// Use this for initialization
	void Start () {
        transform.Rotate(0, 0, 0);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        transform.Rotate(0, Time.fixedDeltaTime * 10, 0);
    }
}
