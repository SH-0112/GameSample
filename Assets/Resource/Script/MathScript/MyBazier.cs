using UnityEngine;
using System.Collections;

public class MyBazier : MonoBehaviour {
	
	public Bezier myBezier;
	//controlP1 = this gameObject position
	public float controlP2RandomRadious = 64f;
	public float controlP3RandomRadious = 32f;
	public float controlP4RandomRadious = 16f;
	public GameObject controlP4Target;
	private float t = 0f;
	
	// Use this for initialization
	void Start () {
		myBezier = new Bezier(this.gameObject.transform.position/*new Vector3( 0f, 250f, 0f )*/, Random.insideUnitSphere * controlP2RandomRadious/*50f*/, Random.insideUnitSphere * controlP3RandomRadious/*10f*/, Random.insideUnitCircle * controlP4RandomRadious + new Vector2(this.gameObject.transform.position.x, this.gameObject.transform.position.z)/*new Vector3( this.gameObject.transform.position.x+Random.Range(-5,5), 0f, this.gameObject.transform.position.z+Random.Range(-100,100) )*/ );
	}


	
	// Update is called once per frame
	void Update () {
		Vector3 vec = myBezier.GetPointAtTime( t );
		transform.position = vec;
		t += Time.deltaTime/2f/*0.01f*/;
		if( t > 1f ){
			t = 0f;
		}
	}
}
