using UnityEngine;
using System.Collections;

public class PlayerBulletEdit : MonoBehaviour {
	//更新:2013/04/02
	//2013/04/04:bool型変数名を修正,通常ヒットエフェクトを追加
	//TargetObject
	private GameObject target;
	//EffectObject
	public GameObject hitEffect;//爆発以外の当たった際のエフェクト
	public GameObject explosionEffect;//誘導弾用のエフェクト
	//CommonParameter(弾ごとに共通する変数)
	public float bulletPower = 10.0f;
	public float moveSpeed = 10.0f;
	public bool isGenerateHitEffect = false;//当たった際にエフェクトを再生する場合はtrue
	public bool isDestructible = true;//近接攻撃の当たり判定の場合はfalseにすることで判定の消失を防ぐ
	public float destroyTime = 6.0f;
	//UniqueElements-----------------
	//Homing(誘導弾)--
	public bool isHoming = false;//弾が誘導弾か否か
	public float rotateSpeed = 15.0f;//エネミーの方を向く速さ
	public float searchStartDelay = 1.0f;//エネミーのサーチ(誘導開始までの時間)
	public float searchInterval = 0.1f;//エネミーとの向きの更新間隔(誘導中の角度更新間隔)
	private float nextSearch = 0.0f;//次に角度を更新する時間を計算するための変数	
	public float searchEndTime = 3.5f;//誘導の終了時間
	//Penetration(貫通弾予定地)--
	public bool isPenetration = false;//弾が貫通弾か否か
	
	// Use this for initialization
	void Start () {
		//誘導標的(gameobject)の登録
		GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
		float dist = 16777215f;//最短距離にいる敵を探索するのに使用するためfloat型で正確に取りうる最大値を入力(符号1bit,指数部8bit,仮数部23bit)
		foreach(GameObject enemy in enemys){//foreachで回して
			if(dist > SearchDistance(enemy)){//現在のdistの値よりも近い位置に敵がいたら
				dist = SearchDistance(enemy);//distの値を更新
				target = enemy;//targetへ該当する敵のGameObjectを代入
			}	
		}
		//誘導終了時間が弾消滅時間より長かったら修正
		if(searchEndTime>destroyTime){
			searchEndTime = destroyTime;
		}
		//HomingTimeClac(誘導時間関連の計算)
		searchStartDelay = Time.time+searchStartDelay;
		searchEndTime = Time.time+searchEndTime;
		//DestroyTimeSet(isDestructible==trueであれば消滅時間を設定)
		if(isDestructible == true){
			Destroy(gameObject, destroyTime);	
		}
	}
	
	// Update is called once per frame
	void Update () {
		BulletControl();
	}
	//SerchDistance---------対象のオブジェクトとの距離を調べて返す
	float SearchDistance(GameObject argObject){//対象のオブジェクトとの距離を調べて返す
		float distance = Vector3.Distance(transform.position, argObject.transform.position);
		return distance;
	}
	
	//BulletControl---------
	void BulletControl(){
		HomingBullet ();
		PenetrationBullet ();
		BulletMove();
	}
	
	//BulletMove-----------------------------------------------------------
	void BulletMove(){
		transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime,Space.Self);
	}
	
	//HomingBullet(誘導弾)--------------------------------------------------
	void HomingBullet(){
		if(isHoming == true){
			HomingStart();
		}
		return;
	}
	//HomingStart
	void HomingStart(){//発射直後から誘導開始まで
		//serchStartDelay エネミーのサーチ(角度修正)を行わない射出直後の時間
		if(Time.time>searchStartDelay){//現在時間がサーチ開始時間を過ぎていれば
			HomingSearchMove();//誘導開始
		}
		return;
	}
	//HomingSerchMove 
	void HomingSearchMove(){//誘導開始から誘導終了まで,一定間隔でプレイヤーとの角度の修正を行う(プレイヤーの方を向く)
		if(target != null &&(Time.time > nextSearch)&&(Time.time < searchEndTime)){//targetがnullではなく,現在時間が角度修正時間を過ぎていて、かつサーチ終了時間前であれば
			nextSearch = Time.time + searchInterval;//次の角度修正時間を設定→角度の修正を行う
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position), Time.deltaTime * rotateSpeed);
		}
		return;//サーチ終了時間を過ぎていれば直進する(特に処理無し)
	}
	
	void HomingEndMove(){//誘導終了後の処理(未定)
		//誘導を打ち切ってしまえば,勝手に直進するので現状では処理は無い
	}
	
	//PenetrationBullet(貫通弾予定地)---------------------------------------------------
	void PenetrationBullet(){
		if(isPenetration == true){
			
		}
		return;
	}
	
	void OnCollisionEnter(Collision collisionInfo){
		if(collisionInfo.gameObject.tag == "Enemy"){//Enemyタグであれば
			SendDamage(collisionInfo.gameObject);//Damageの送信を行い
			Destroy(gameObject,0.0f);//自分自身をデストロイ
		}
		if(collisionInfo.gameObject.tag =="Player"|| collisionInfo.gameObject.tag == "Bullet" ||collisionInfo.gameObject.tag == "EnemyBullet"){
			return;//Plyer,Bullet,EnemyBulletタグであればreturn
		}
		if(isPenetration == true){
			return;//貫通弾であればreturn
		}
		if(isGenerateHitEffect == true){
			GenerateHitEffect();//通常ヒットエフェクトを発生させる
		}
		if(isHoming == true){
			GenerateExprosionEffect();//誘導弾であれば着弾時のエフェクトを発生させる
		}
		if(isDestructible == true){//破壊可能な弾であれば
			Destroy(gameObject,0.0f);//自分自身をデストロイ
		}
	}

	
	void OnDestroy(){//オブジェクトが破棄される直前に呼び出される変数
		/*if(homing == true){
			GenerateExprosionEffect();
		}*/		
	}
	
	void GenerateHitEffect(){//通常のヒットエフェクトを生成する
		Instantiate(hitEffect, transform.position, transform.rotation);
	}
	
	void GenerateExprosionEffect(){//爆発エフェクトを生成する
		Instantiate(explosionEffect,transform.position,transform.rotation);
	}
	
	void SendDamage(GameObject argObject){//ダメージをエネミー側へ送信するための関数(予定地)
		//ApplyDamage→EnemyStatusKeeper
		argObject.SendMessage("ApplyDamage", bulletPower);
	}
}
