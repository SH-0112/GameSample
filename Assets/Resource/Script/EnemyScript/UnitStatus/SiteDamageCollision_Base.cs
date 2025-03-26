using UnityEngine;
using System.Collections;
	
//1.ダミーコリジョンオブジェクトに貼り付ける.
//2.登録されたUnitStatus~に処理済みダメージを渡す.
//3.モーション,座標移動に追従させる場合はRigidbodyのIsKinematicをチェック.
//要約:部位別に本体へダメージを送るクラス.
public class SiteDamageCollision_Base : MonoBehaviour {
	//試験的に建造[2013/09/06].
	
	public UnitStatus_Base unitStatus;//送り先のUnitSatus~を入れる(継承先の物でもおｋ).
	public float damageCutPercent = 0.0f;//n倍したい場合は負の値を入れる.
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public virtual void ApplyDamage(float argDamage){
		unitStatus.ApplyDamage(argDamage-(argDamage*(damageCutPercent/100.0f)));
	}
}
