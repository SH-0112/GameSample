using UnityEngine;
using System.Collections;

public class ParticleAutoDestructer : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		ParticleDestroyCheck();
	}
	
	void ParticleDestroyCheck(){//Loop設定がされていないパーティクルが再生を終了していた場合デストロイする.
		if(GetComponent<ParticleSystem>().IsAlive() == false){
			Destroy(gameObject);
		}
	}
}
