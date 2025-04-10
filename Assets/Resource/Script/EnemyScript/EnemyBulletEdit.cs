using UnityEngine;
using System.Collections;

public class EnemyBulletEdit : MonoBehaviour {
	//更新:2013/03/06
	//更新:2013/05/17 isDestriyTimeがfalseでもDestroyされてしまうバグを修正
	//TargetObject
	private GameObject target;
	//EffectObject
	public GameObject muzzleFlashEffect;//マズルフラッシュエフェクト[2013/08/22追加]
	public GameObject impactEffect;//インパクト(着弾)エフェクト[2013/08/22追加]
	public GameObject explosionEffect;
	//CommonParameter(弾ごとに共通する変数)
	public float bulletPower = 10.0f;
	public float moveSpeed = 10.0f;
	public bool isDestructible = true;//近接攻撃の当たり判定の場合はfalseにすることで判定の消失を防ぐ
	public float destroyTime = 6.0f;
	//UniqueElements-----------------
	//Homing(誘導弾)--
	public bool homing = false;				//弾が誘導弾か否か
	public float rotateSpeed = 15.0f;		//プレイヤーの方を向く速さ
	public float searchStartDelay = 1.0f;	//プレイヤーのサーチ(誘導開始までの時間)
	public float searchInterval = 0.1f;		//プレイヤーとの向きの更新間隔(誘導中の角度更新間隔)
	private float nextSearch = 0.0f;		//次に角度を更新する時間を計算するための変数	
	public float searchEndTime = 3.5f;		//誘導の終了時間
	//Penetration(貫通弾予定地)--
	public bool penetration = false;//弾が貫通弾か否か
	//BezirLaser(ベジェ曲線弾：へにょりレーザーなど)	--[2013/08/22追加]
	public bool isBezierBullet= false;			//ベジェ曲線弾か否か
	public bool isBezierRandomImpact = false;	//着弾点をランダムにするか否か(falseであれば生成時のターゲット座標へ向かう)
	public bool isAntiAirBezierRandomBullet = false;	//対空ランダムを行うベジェ曲線弾であるかないか(trueであれば,着弾点をランダムにした際の処理を切り替える)
	public Bezier myBezier;
	public float controlP1RandomRadious = 64f;	//制御点P1をランダム決定する際の半径
	public float controlP2RandomRadious = 32f;	//制御点P2をランダム決定する際の半径
	public float controlP3RandomRadious = 16f;	//制御点P3をランダム決定する際の半径
	private Vector3 controlP0FirstPos;			//制御点P0(初期位置)
	private Vector3 controlP3TargetPos;			//制御点P3(着弾点)
	private float controlP0ToP3Dist;			//初期位置から着弾点までの距離
	public float bezierClacTime = 0.0f;
	//-BezirImpactMarker(ベジェ曲線弾用着弾点マーカー)-
	public bool isGenerateBezierImpactMarker;
	public GameObject bezierImpactMarker;
	
	// Use this for initialization
	void Start () {
		//-誘導弾のデータ入力---.
		//誘導標的(gameobject)の登録
		target = GameObject.FindWithTag("Player");
		//誘導終了時間が弾消滅時間より長かったら修正
		if(searchEndTime>destroyTime){
			searchEndTime = destroyTime;
		}
		//HomingTimeClac(誘導時間関連の計算)
		searchStartDelay = Time.time+searchStartDelay;
		searchEndTime = Time.time+searchEndTime;
		
		//-ベジェ曲線弾のデータ入力---.[2013/08/22追加]
		//着弾点データ入力
		controlP0FirstPos = this.gameObject.transform.position;//生成時の初期位置を登録
		if(isBezierRandomImpact == true && isAntiAirBezierRandomBullet == false){//trueであれば着弾地点をランダムに.
			Vector2 vec2 = Random.insideUnitCircle * controlP3RandomRadious;
			controlP3TargetPos = new Vector3(this.gameObject.transform.position.x+vec2.x, 0.0f, this.gameObject.transform.position.z+vec2.y);
		}else if(isBezierRandomImpact == true && isAntiAirBezierRandomBullet == true){//対空ランダム弾であれば
			controlP3TargetPos = Random.insideUnitSphere * controlP3RandomRadious + target.transform.position;
		}else{//そうでなければ,生成時のプレイヤー座標へ.
			controlP3TargetPos = target.transform.position;
		}
		//着弾地点までの距離算出
		controlP0ToP3Dist = Vector3.Distance(controlP3TargetPos, controlP0FirstPos);
		//制御点データ入力
		myBezier = new Bezier(controlP0FirstPos,//P0
			Random.insideUnitSphere * controlP1RandomRadious,//P1
			Random.insideUnitSphere * controlP2RandomRadious,//P2
			controlP3TargetPos);//P3
		
		//-共通データ入力---.
		//DestroyTimeSet(isDestructible==trueであれば消滅時間を設定)
		if(isDestructible == true){
			Destroy(gameObject, destroyTime);	
		}
		//-マズルフラッシュエフェクト生成-
		GenerateMuzzleFlashEffect();
		//-ベジェ曲線弾の着弾点マーカー生成-
		GenerateBezierImpactMarker();
	}
	
	// Update is called once per frame
	void Update () {
		BulletControl();
	}
	//BulletControl---------
	void BulletControl(){
		HomingBullet ();
		PenetrationBullet ();
		BezierBullet();
		BulletMove();
	}
	
	//BulletMove-----------------------------------------------------------
	void BulletMove(){
		transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime,Space.Self);
	}
	
	//HomingBullet(誘導弾)--------------------------------------------------
	#region 
	void HomingBullet(){
		if(homing == true){
			HomingStart();
		}
		return;
	}
	//HomingStart
	void HomingStart(){//発射直後から誘導開始まで
		//serchStartDelay プレイヤーのサーチ(角度修正)を行わない射出直後の時間
		if(Time.time>searchStartDelay){//現在時間がサーチ開始時間を過ぎていれば
			HomingSearchMove();//誘導開始
		}
		return;
	}
	//HomingSerchMove 
	void HomingSearchMove(){//誘導開始から誘導終了まで,一定間隔でプレイヤーとの角度の修正を行う(プレイヤーの方を向く)
		if((Time.time > nextSearch)&&(Time.time < searchEndTime)){//現在時間が角度修正時間を過ぎていて、かつサーチ終了時間前であれば
			nextSearch = Time.time + searchInterval;//次の角度修正時間を設定→角度の修正を行う
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target.transform.position - transform.position), Time.deltaTime * rotateSpeed);
		}
		return;//サーチ終了時間を過ぎていれば直進する(特に処理無し)
	}
	
	void HomingEndMove(){//誘導終了後の処理(未定)
		//誘導を打ち切ってしまえば,勝手に直進するので現状では処理は無い
	}
	#endregion
	//PenetrationBullet(貫通弾予定地)----------------------------------------------
	#region
	void PenetrationBullet(){
		if(penetration == true){
			
		}
		return;
	}
	#endregion
	//BezierBullet(ベジェ曲線弾)[2013/08/22追加]---------------------------------------------------
	void BezierBullet(){
		if(isBezierBullet == true){
			BezierMoveRotation();
		}
	}
	void BezierMoveRotation(){
		if(bezierClacTime > 1.0f){
			return;
		}else{
			Vector3 vec = myBezier.GetPointAtTime( bezierClacTime );
			//transform.position = vec;
			transform.LookAt(vec);
			//transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(vec - transform.position), rotateSpeed*Time.deltaTime);
			//発射から着弾までにかける時間は直線時と同じとする
			bezierClacTime += moveSpeed * Time.deltaTime/controlP0ToP3Dist/*0.001f*/;//t=0~100%,速度*経過時間/距離=経過時間あたりの進行度
		}
	}
	//Unityオーバーライド関数関連---------------------------------------------------
	void OnCollisionEnter(Collision collisionInfo){
		if(collisionInfo.gameObject.tag == "Player"){
			//Debug.Log("Player Hit Collision!");
			SendPlayerDamage(collisionInfo);
			GenerateImpactEffect();//着弾エフェクト生成
			if (isDestructible == true){
				GenerateImpactEffect();//着弾エフェクト生成[2013/08/22追加]
				Destroy(gameObject,0.0f);
			}
		}
		if(collisionInfo.gameObject.tag =="Enemy"||collisionInfo.gameObject.tag == "EnemyBullet"){
			return;
		}
		if(penetration == true){
			return;
		}
		if(homing == true){
			GenerateExprosionEffect();
		}
		if(isDestructible == true){
			GenerateImpactEffect();//着弾エフェクト生成[2013/08/22追加]
			Destroy(gameObject,0.0f);
		}
	}
	/*void OnToriggerEnter(Collider colliderInfo){
		if(colliderInfo.gameObject.tag =="Enemy"||colliderInfo.gameObject.tag == "EnemyBullet"){
			return;
		}
		if(penetration == true){
			return;
		}
		if(homing == true){
			GenerateExprosionEffect();
		}
		if(isDestructible == true){
			Destroy(gameObject,0.0f);
		}
	}*/
	
	void OnDestroy(){//オブジェクトが破棄される直前に呼び出される変数
		/*if(homing == true){
			GenerateExprosionEffect();
		}*/		
	}
	//エフェクト生成関連---
	void GenerateMuzzleFlashEffect(){//マズルフラッシュエフェクトを生成する[2013/08/22追加]
		if(muzzleFlashEffect == true){//登録されていれば
			Instantiate(muzzleFlashEffect, transform.position,transform.rotation);
		}
	}
	void GenerateImpactEffect(){//着弾エフェクトを生成する[2013/08/22追加]
		if(impactEffect == true){//登録されていれば
			Instantiate(impactEffect, transform.position, transform.rotation);
		}
	}
	void GenerateExprosionEffect(){//爆発エフェクトを生成する
		Instantiate(explosionEffect,transform.position,transform.rotation);
	}
	//マーカー生成関連---
	void GenerateBezierImpactMarker(){//ベジェ曲線弾の着弾点マーカーを生成する
		if(isGenerateBezierImpactMarker == true){
			Instantiate(bezierImpactMarker, controlP3TargetPos, transform.rotation);
		}
	}
	//---
	void SendPlayerDamage(Collision argCollisionInfo){//ダメージをプレイヤー側へ送信するための関数
		if(GameObject.FindWithTag("GameController") != null){
			GameObject.FindWithTag("GameController").SendMessage("ApplyPlayerDamage", bulletPower, SendMessageOptions.DontRequireReceiver);//エラーチェックを行わない.
		}else{
			argCollisionInfo.gameObject.SendMessage("ApplyDamage", bulletPower, SendMessageOptions.DontRequireReceiver);//UnitStatusを継承した場合用,エラーチェックを行わない.
		}
		
	}
}
