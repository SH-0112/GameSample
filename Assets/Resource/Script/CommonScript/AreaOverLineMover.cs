using UnityEngine;
using System.Collections;

public class AreaOverLineMover : MonoBehaviour {
	
	//エリアオーバーラインをプレイヤーの高度にあわせるスクリプト.
	public GameObject target;
	public string targetTag = "Player";//デフォルトは"Player".
	
	// Use this for initialization
	void Start () {
		target = GameObject.FindWithTag(targetTag);//タグに指定されているGameObjectを取得.
	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(transform.position.x, target.transform.position.y, transform.position.z);
	}
}
