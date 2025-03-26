using UnityEngine;
using System.Collections;

public class UnitStatus_Rollvincher : UnitStatus_Base {
	
	//-Rollvincher専用のステータス-.
	public float barrierMaxHp = 250f;//バリア用HP最大値(再展開後この値まで回復).
	public float barrierNowHp = 0f;
	
	// Use this for initialization
	public override void Start () {
		base.Start();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
		barrierNowHp = barrierMaxHp;
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
		base.ApplyDeath(deadDestroyDelay);
	}
	//バリアへ回復処理.
	public void ApplyBarrierHeal(float argHealPoint){
		barrierNowHp += argHealPoint;
		//上限値突破時の修正.
		if(barrierNowHp > barrierMaxHp){
			barrierNowHp = barrierMaxHp;
		}
	}
	//バリアへのダメージ処理.
	public void ApplyBarrierDamage(float argDamage){
		barrierNowHp -= argDamage;				//現在バリアHPから減算.
		//下限値突破時の修正.
		if(0 > barrierNowHp){
			barrierNowHp = 0;
		}
	}
}
