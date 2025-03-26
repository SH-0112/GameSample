using UnityEngine;
using System.Collections;

public class OrbitalRotation_Z : MonoBehaviour {

	public float angularVelocity = 45;
		
	// Update is called once per frame
	void Update () {
		//transform.RotateAround(transform.forward,Mathf.Deg2Rad*angularVelocity*Time.deltaTime);
		transform.Rotate(transform.forward, Mathf.Deg2Rad*angularVelocity*Time.deltaTime);
	}
}
