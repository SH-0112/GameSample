using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent (typeof(Rigidbody))]
public class MeleeAttackHit : MonoBehaviour
{
	public GameObject hitObject;
	
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Player") && other.CompareTag("Enemy"))
		{
			Instantiate(hitObject, transform.position, transform.rotation);
		}		
	}
}
