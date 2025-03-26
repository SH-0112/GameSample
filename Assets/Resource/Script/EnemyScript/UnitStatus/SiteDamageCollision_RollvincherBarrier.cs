using UnityEngine;
using System.Collections;

public class SiteDamageCollision_RollvincherBarrier : SiteDamageCollision_Base {
		
	public UnitStatus_Rollvincher unitStatusRollvincher;	
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	//Rollvincherのバリア剥がし用の判定へダメージを送る.
	public override void ApplyDamage(float argDamage){
		unitStatusRollvincher.ApplyBarrierDamage(argDamage-(argDamage*(damageCutPercent/100.0f)));
	}
}
