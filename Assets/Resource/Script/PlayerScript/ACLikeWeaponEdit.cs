using UnityEngine;
using System.Collections;
 
public class ACLikeWeaponEdit : MonoBehaviour {
	//[実装案検討中.]
	//ACLikeController,ACLikeCamera,UnitStatus_hogeとの併用を前提.
	//Player用のWeponEdit.
	//ACLikeCameraのrayTarget,rayTargetPosから射出角度(銃身の向き)を決める.
	
	//-MainCameraの情報取得用-.
	public ACLikeCamera mainCamera;
	//-攻撃位置指定用のオブジェクト情報取得用-
	public GameObject AtkTargetPosObjcet;
	//--.
	//-TargetObject-[標的登録に使用].
	public string targetTag = "Enemy";//ターゲット探索用のタグ,デフォルトは"Enemy".
	public GameObject target;
	//-BulletObject-.
	public GameObject bullet;
	//-LockOnTime-
	public float lockOnTime = 1.0f;//ロックオンに要する時間(暫定基本1秒)
	//-BarrageCoolTime-.
	public float fireRate = 0.5f;//何秒に一度発射するか.
	public float timeCounter = 0.0f;//fireRateの時間になるまで前フレームからの経過時間を加算,保存する変数.
	//-TriggerSwitch-[起動/停止].
	public bool isTriggerOn = false;
	//-firePos-(武器オブジェクトからの相対定期な発射位置).
	[Vector3Field]
	public Vector3 firePos;
	//-baseAngle-.
	[Vector3Field]
	public Vector3 baseAngle;
	//-RandomAngle-(集弾率の調整,近距離ばら撒き系の武器(マシンガンなど)を作る際に).
	[Vector3Field]
	public Vector3 randAngle;
	private Vector3 randAngleAnswer;
	public bool randomShotX = false;
	public bool randomShotY = false;
	public bool randomShotZ = false;
	//-Way-(nWay弾,ショットガン,スラッグガンを作る際の弾と弾の各軸の弾数n).
	[Vector3Field]
	public Vector3 way = Vector3.one;//整数型で入力する.
	private Vector3 wayTotalAngle;
	//-SpaceAngle-(nWay弾,ショットガン,スラッグガンを作る際の弾と弾の各軸の角度間隔).
	[Vector3Field]
	public Vector3 spaceAngle;

	//--設置型武器(ターレット,セントリーガン)など特殊兵装用の変数群--.
	//-ShotType-.
	public bool isAimingShot = false;
	public bool isAimingShotOnlyX = false;//X方向のみの自機狙い.
	public bool isAimingShotOnlyY = false;//Y方向のみの自機狙い.
	public bool isAimingShotTargetFoot = false;//Targetの足元を直接狙う[2013/08/27追加].
	//aimingTargetFootRandom[2013/08/27追加]
	[Vector3Field]
	public Vector3 targetFootRandPos;//Targetの足元を狙う際の振れ幅
	//-ShotSpin-(砲身の向きに関係なく発射方向が回転する場合に).
	public bool isShotAxisRotationX = false;
	public bool isShotAxisRotationY = false;
	public bool isShotAxisRotationZ = false;
	[Vector3Field]
	public Vector3 addShotAxisRotation;//回転角度.
	//-SelfDestroy-[ターレット,セントリーガンなど設置型の武器の際使用].
	public bool isSentryGun = false;//自機が装備しない,設置型の武器であるか否か.
	public bool isSelfDestroy = false;
	public float destroyTime = 9.0f;

	// Use this for initialization
	void Start () {
		//-標的の更新-.
		TargetUpdate();
		//-Tag"MainCamera"のACLikeCameraコンポーネントを取得-.
		if(targetTag == "Enemy" && isSentryGun == false){//自機兵装(targetTag"Enemy"かつ,セントリーガンではない)であれば.
			mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<ACLikeCamera>();//MainCameraのコンポーネント取得用.
		}
		if(isSelfDestroy == true){//時間経過で消滅する場合(ターレット,ビット,設置型武器等).
			Destroy(this.gameObject, destroyTime);
		}
	}
	
	// Update is called once per frame
	void Update () {
		BaseAngleUpdate();//親の回転に合わせて自身の射出角度を修正.
		TargetUpdate();//標的の更新.
		timeCounter += Time.deltaTime;//前フレームからの経過時間を加算.
		//ACLikeCameraから取得した座標の方向へ向かせる.
		if(targetTag == "Enemy" && isSentryGun == false){//自機兵装(targetTag"Enemy"かつ,セントリーガンではない)であれば.
			//transform.LookAt(mainCamera.rayTargetPos);
			AtkTargetPosObjcet.transform.position = mainCamera.rayTargetPos + Vector3.up;//offset分upする(FinalIK仕様)
		}
		if(isTriggerOn == true){
			if(timeCounter > fireRate){//前フレームから加算してカウントした経過時間が実行時間を超えていたら.
				BarrageControl();//弾道の計算~生成を行って.
				timeCounter = 0;//最後にカウンターを初期化.
				
			}
			
		}
		
		//[ToDo]セントリーガン用の処理.
	}
	//-射出する弾幕、弾道の計算のメイン&生成-.
	void BarrageControl(){
		//BarrageClac.
		wayTotalAngle.x = spaceAngle.x * (int)(way.x);
		wayTotalAngle.y = spaceAngle.y * (int)(way.y);
		wayTotalAngle.z = spaceAngle.z * (int)(way.z);
		/*
		wayTotalAngleX = spaceAngleX*(wayX-1);
		wayTotalAngleY = spaceAngleY*(wayY-1);
		wayTotalAngleZ = spaceAngleZ*(wayZ-1);
		*/
		
		//BarrageSpread.
		for(int i = 0; i<(int)(way.x); i++){
			for(int j = 0; j<(int)(way.y); j++){
				for(int k = 0; k<(int)(way.z); k++){
					AimingShotClac();//Aiming or OtherShot.
					ShotAxisRotationClac();//Rotation or NotRotation.
					RandomShotAngleClac();//RandomShot or NotRandom.
					//↓BarrageSpredClacMain.
					Quaternion barrageAngle = Quaternion.Euler(baseAngle.y+randAngleAnswer.y-(wayTotalAngle.y/2)+spaceAngle.y*j, baseAngle.x+randAngleAnswer.x-(wayTotalAngle.x/2)+spaceAngle.x*i, baseAngle.z+randAngleAnswer.z-(wayTotalAngle.z/2)+spaceAngle.z*k);
					if(isAimingShotTargetFoot == true){//ターゲットの足元を直接狙う弾であれば[2013/08/27追加].
						Instantiate(bullet, target.transform.position 
						+ new Vector3(Random.Range(-targetFootRandPos.x, targetFootRandPos.x),
						Random.Range(-targetFootRandPos.y, targetFootRandPos.y),
						Random.Range(-targetFootRandPos.z, targetFootRandPos.z)), barrageAngle);
					}else{//そうでなければ.
						//Debug.Log("BulletInstantiate");
						Instantiate(bullet,transform.TransformPoint(firePos.x,firePos.y,firePos.z),barrageAngle);
					}
				}
			}
		}
	}
	
	//AimingShotClac--自機狙いの計算.
	void AimingShotClac(){
		if(isAimingShot == true){//AimingShot
			Quaternion baseAngle = Quaternion.LookRotation(target.transform.position-transform.position);
			transform.rotation = baseAngle;
			//baseAngleX = baseAngle.eulerAngles.y;
			//baseAngleY = baseAngle.eulerAngles.x;
			//baseAngleZ = /*baseAngle.z;*/baseAngle.eulerAngles.z;							
		}
		if((isAimingShotOnlyX == true)&&(isAimingShot == false)){
			Quaternion baseAngle = Quaternion.LookRotation(target.transform.position-transform.position);
			transform.rotation = Quaternion.Euler(transform.rotation.x, baseAngle.eulerAngles.y, transform.rotation.z);
			//baseAngleX = baseAngle.eulerAngles.y;														
		}
		if((isAimingShotOnlyY == true)&&(isAimingShot == false)){
			Quaternion baseAngle = Quaternion.LookRotation(target.transform.position-transform.position);
			transform.rotation = Quaternion.Euler(baseAngle.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
			//baseAngleX = baseAngle.eulerAngles.y;														
		}
	}
	//-shotAxisRotationClac--回転弾幕の計算.
	void ShotAxisRotationClac(){
		if(isShotAxisRotationX == true){
			baseAngle.x += addShotAxisRotation.x * Time.deltaTime;//.frameCount;
		}
		if(isShotAxisRotationY == true){
			baseAngle.y += addShotAxisRotation.y * Time.deltaTime;//.frameCount;
		}
		if(isShotAxisRotationZ == true){
			baseAngle.z += addShotAxisRotation.z * Time.deltaTime;//.frameCount;
		}		
	}
	//-randomShotAngleClac--射出するのブレの計算.
	void RandomShotAngleClac(){
		if(randomShotX == true){
			randAngleAnswer.x = Random.Range(-randAngle.x/2.0f,randAngle.x/2.0f);
		}
		if(randomShotY == true){
			randAngleAnswer.y = Random.Range(-randAngle.y/2.0f,randAngle.y/2.0f);
		}
		if(randomShotZ == true){
			randAngleAnswer.z = Random.Range(-randAngle.z/2.0f,randAngle.z/2.0f);
		}
	}
	//-BaseAngleUpdate--自身の角度の更新.
	void BaseAngleUpdate(){
		baseAngle.x = transform.localRotation.eulerAngles.y+transform.root.rotation.eulerAngles.y;
		baseAngle.y = transform.localRotation.eulerAngles.x+transform.root.rotation.eulerAngles.x;
		baseAngle.z = transform.localRotation.eulerAngles.z+transform.root.rotation.eulerAngles.z;
		if (targetTag == "Enemy" && isSentryGun == false)
		{//自機兵装(targetTag"Enemy"かつ,セントリーガンではない)であれば(FinalIK仕様)
			baseAngle.x = transform.rotation.eulerAngles.y;
			baseAngle.y = transform.rotation.eulerAngles.x;
			baseAngle.z = transform.rotation.eulerAngles.z;
		}
	}
	//-TargetUpdate--標的の更新.
	void TargetUpdate(){
		if(isAimingShot==true || isAimingShotOnlyX==true || isAimingShotOnlyY==true){
			//誘導標的(gameobject)の登録.
			GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
			float dist = 16777215f;//最短距離にいるターゲットを探索するのに使用するためfloat型で正確に取りうる最大値を入力(符号1bit,指数部8bit,仮数部23bit).
			foreach(GameObject newTarget in targets){//foreachで回して.
				if(dist > SearchDistance(newTarget)){//現在のdistの値よりも近い位置に敵がいたら.
					dist = SearchDistance(newTarget);//distの値を更新.
					target = newTarget;//targetへ該当する敵のGameObjectを代入.
				}	
			}
		}
	}
	//-SerchDistance---------対象のオブジェクトとの距離を調べて返す.
	float SearchDistance(GameObject argObject){//対象のオブジェクトとの距離を調べて返す.
		float distance = Vector3.Distance(transform.position, argObject.transform.position);
		return distance;
	}
	
	//-ApplyIsToriggerOn--武装の起動.
	public void ApplyIsToriggerOn(){
		isTriggerOn = true;
	}
	//-ApplyIsToriggerOff--武装の停止.
	public void ApplyIsToriggerOff(){
		isTriggerOn = false;
	}
	//-ロックオン所要時間の取得-
	public float GetLockOnTime()
	{
		return lockOnTime;
	}
}
