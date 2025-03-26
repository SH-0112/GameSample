using UnityEngine;
using System.Collections;

public class OrbitalRotation : MonoBehaviour {

	public float angularVelocity = 45.0f;
		
	// Update is called once per frame
	void Update () {
		//transform.RotateAround(transform.up,Mathf.Deg2Rad*angularVelocity*Time.deltaTime);
		transform.Rotate(transform.up, Mathf.Deg2Rad*angularVelocity*Time.deltaTime);
	}
}
