using UnityEngine;
using System.Collections;

public class EnemyAI_CommonEnemy : EnemyAI_Base {
	
	//-ステータス関連-.
	//HPなどユニットの死亡判定,状態遷移などに使用するステータス.
	public UnitStatus_CommonEnemy unitStatus;
	//-火器管制関連-.
	public FireControlSystem fcs;
	//-射程関連-.
	public float searchRange = 150.0f;
	public float mainWeaponRange = 150.0f;
	public float subWeaponRange = 50.0f;
	
	// Use this for initialization
	public override void Start () {
		base.Start();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
		//UnitStatusのセットアップ.
		unitStatus = GetComponent<UnitStatus_CommonEnemy>();
		//火器管制関連のセットアップ.
		fcs = GetComponent<FireControlSystem>();
		fcs.AllWeaponsDeactivation();//何か武装が起動していた際のための全停止.
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
		AnimationControl();
		ActionPatternControl();
	}
	
	//-継承後実装した純粋仮想関数群---.
	
	//-[純粋仮想関数]アニメーション管理部-.
	public override void AnimationControl(){
		
	}
	//-[純粋仮想関数]行動パターン管理部-.
	public override void ActionPatternControl(){
		if(searchRange > SearchDistance(target) && unitStatus.nowHp > 0){//索敵範囲内に入っていて,生存していれば戦闘モードへ移行.
			SystemCombatMode();
		}else{
			SystemIdlingMode();//そうでなければ,待機モード.
		}
		
	}
	//-[純粋仮想関数]それぞれの行動パターン毎の記述-.
	public override void SystemCombatMode(){
		ApplyTargetTurn(target);//targetの方向を向く.
		SendWeponAttackOrder();//武装オブジェクトの有効化.
	}
	public override void SystemScanMode(){
		
	}
	public override void SystemIdlingMode(){
		SendWeponStopOrder();//武装オブジェクトの無効化.
	}
	//-[純粋仮想関数]武装オブジェクトへのアクセス-.
	public override void SendWeponAttackOrder(){
		if(mainWeaponRange > SearchDistance(target)){//MainWeaponの射程内なら.
			fcs.WeaponsActivation(0);
		}
		if(subWeaponRange > SearchDistance(target)){
			fcs.WeaponsActivation(1);
		}
		//fcs.AllWeaponsActivation();//全武装の起動(※これを分ける必要がある場合は,固有のAIを作る).
	}
	public override void SendWeponStopOrder(){
		fcs.AllWeaponsDeactivation();//全武装の停止(※これを分ける必要がある場合は,固有のAIを作る).
	}
}
