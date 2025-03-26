using UnityEngine;
using System.Collections;

public class BuildingBreaker : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnCollisionEnter(Collision collisionInfo){
		if(collisionInfo.gameObject.tag == "building1" || collisionInfo.gameObject.tag == "building2" 
				|| collisionInfo.gameObject.tag == "building3" || collisionInfo.gameObject.tag == "building4"){
				//BuildingBreakDowner.csを張り付けたオブジェクト(タグbuilding1~4)を破壊する(崩壊させる処理を実行).
				collisionInfo.gameObject.SendMessage("ApplyBreakDown", "DontRequireReceiver");
		}
	}
}
