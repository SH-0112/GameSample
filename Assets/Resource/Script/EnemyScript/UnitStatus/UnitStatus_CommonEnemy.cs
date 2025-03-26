using UnityEngine;
using System.Collections;

public class UnitStatus_CommonEnemy : UnitStatus_Base {

	// Use this for initialization
	public override void Start () {
		base.Start();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
		base.ApplyDeath(deadDestroyDelay);
	}
}
