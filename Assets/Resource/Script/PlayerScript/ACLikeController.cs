using UnityEngine;
using System.Collections;
using System;

public class ACLikeController : MonoBehaviour {
	//[2013/10/05作成].
	//[構築予定仕様].
	//[CharacterController]を使用していることを前提とする.
	//プレイヤーはカメラの注視点方向を向く[Y軸のみ].
	//武装の照準はカメラの注視店方向を向く[軸制限無し].
	//[WASD]で前後左右移動,斜め移動時は移動速度を√2で割る.
	//[スペースキー(継続入力)]でブースト使用,入力方向へ加速,ニュートラルの場合は上方向へ加速(上昇).
	//[ToDo:実装予定]----.
	//[ジャンプ]:[接地状態]から[ニュートラル]でブースト(押下)を受け付けた際の短距離高速上昇.
	//[クイックブースト(サイドステップ)]:[方向キー+ブースト(押下)]を受け付けた際の短距離高加速.
	//[壁蹴り(ブーストドライブ)]:壁などに接触した状態での短距離上方加速(優先度:低).

	//-MainCameraの情報取得用-.
	private ACLikeCamera mainCamera;
	//-Animator取得用-
	private Animator animator;
	//-武装の登録用-.
	public ACLikeWeaponEdit[] mainWeaponEdit;
	public ACLikeWeaponEdit[] subWeaponEdit;
	//-各種エフェクト登録用-.
	public GameObject boostEffect;
	//-通常移動-.
	public float moveSpeed = 10.0f;				//移動速度.
	public Vector3 moveDirection = Vector3.zero;//移動方向.
	public float rotateSpeed = 3.0f;			//旋回速度.
	public Vector3 rotateDirection = Vector3.zero;//旋回方向.
	//-ブースト関連-.
	public float boostEnergy = 0.0f;
	public float boostEnergyMax = 100.0f;
	public Vector3 boostDirection = Vector3.zero;//ブースト方向.
	public float boostSpeed = 0.0f;
	public float boostSpeedMax = 5.0f;
	public float quickBoostSpeed = 0.0f;
	public float quickBoostSpeedMax = 64.0f;
    private bool quickBoostUsed = false;
	//-消費EN-.
	public float boostNeedEN = 1.0f;		//ブースト移動で1秒あたりに消費するエネルギー.
	public float quickBoostNeedEN = 1.5f;	//クイックブースト(ジャンプと共用)1回で消費するエネルギー.
	public float gravity = 9.8f;			//適用する重力. 
	
	//-CharacterController.Moveにおいてコリジョンのどの面が衝突しているか(上面(above),側面(sides),底面(below))-.
	public CollisionFlags collisionFlags;
	private CharacterController controller;
	
	// Use this for initialization
	void Start () {
		//-Tag"MainCamera"のACLikeCameraコンポーネントを取得-.
		mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<ACLikeCamera>();//MainCameraのコンポーネント取得用.
		animator = GetComponent<Animator>();//Animator取得
		controller = gameObject.GetComponent<CharacterController>();//コンポーネントにあるキャラコンを取得.
		boostEnergy = boostEnergyMax;//開始時にブーストゲージをMAXまで充填.
	}
	
	// Update is called once per frame
	void Update () {
		AnimationControl();
		ActionControl();
        //XInputだとLT,RTが3rdAxis扱いで-1~1でまとめられちゃってるからどうしようねこれ…
        //LT:9thAxis,RT:10thAxisで0-1で入力取れる
        Debug.Log("Input.GetAxis(Jump) = "+Input.GetAxis("Jump"));
	
	}
	
	//アニメーションの管理.
	void AnimationControl(){

		// モーションを切り替える

		//ブレンドツリーを使用
		animator.SetFloat("Blend_X", Input.GetAxis("Horizontal"));
		animator.SetFloat("Blend_Z", Input.GetAxis("Vertical"));
		animator.SetBool("OnGround", IsGrounded());
		if (!IsGrounded())
		{
			animator.SetFloat("Jump", Input.GetAxis("Jump"));
		}

	}
	//操作の管理.
	void ActionControl(){
		//-武装の起動/停止命令-.
		ApplyFireControl();
		//-ブーストエフェクトのOn/Off-.
		ApplyBoostEffectControl();
		//-擬似重力の適用-.
		ApplyGravity();
		//-前後左右移動-.
		ApplyMove();
		//-ブースト移動処理-.
		ApplyJumpBoost();
		ApplyQuickBoost();
		ApplyBoost();
		//-ロックオン移動処理-
		ApplyLockOn();
	}

	//-武装関連-.
	//火器管制.
	void ApplyFireControl(){
		if(Input.GetMouseButton(0)||Input.GetButton("Fire1")){//左クリックで主兵装を使用.
			foreach(ACLikeWeaponEdit mainWeaponEdits in mainWeaponEdit){
				mainWeaponEdits.ApplyIsToriggerOn();
			}
		}else{
			foreach(ACLikeWeaponEdit mainWeaponEdits in mainWeaponEdit){
				mainWeaponEdits.ApplyIsToriggerOff();
			}
		}
		if(Input.GetMouseButton(2)||Input.GetButton("Fire2")){//右クリックで副兵装を使用.
			foreach(ACLikeWeaponEdit subWeaponEdits in subWeaponEdit){
				subWeaponEdits.ApplyIsToriggerOn();
			}
		}else{
			foreach(ACLikeWeaponEdit subWeaponEdits in subWeaponEdit){
				subWeaponEdits.ApplyIsToriggerOff();
			}	
		}
	}
	
	//--移動関連--.
	//通常移動の適用.
	void ApplyMove(){
		//-旋回[マウスのX方向移動量を適用]-.
		//controller.transform.Rotate(0, Input.GetAxis("Mouse X") * rotateSpeed, 0);
        controller.transform.Rotate(0, Input.GetAxis("Horizontal2") * rotateSpeed, 0);
		//-移動[WASDを適用]-.
		//入力所得.
		moveDirection = new Vector3(Input.GetAxis("Horizontal"), moveDirection.y, Input.GetAxis("Vertical"));
		//入力を方向情報のみに限定する.
		moveDirection = transform.TransformDirection(moveDirection);
		//実際に移動へ適用.
		if(Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") != 0){//左右,前後の入力が同時に入っていれば.
			//√2で割った速度を適用する.
			//controller.Move(Vector3.right * Input.GetAxis("Horizontal") * moveSpeed / Mathf.Sqrt(2.0f) * Time.deltaTime);
			//controller.Move(Vector3.forward * Input.GetAxis("Vertical") * moveSpeed / Mathf.Sqrt(2.0f) * Time.deltaTime);
			collisionFlags = controller.Move(moveDirection * moveSpeed / Mathf.Sqrt(2.0f) * Time.deltaTime);
		}else{//そうでなければ通常の速度を適用する.
			//controller.Move(Vector3.right * Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime);
			//controller.Move(Vector3.forward * Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime);
			collisionFlags = controller.Move(moveDirection * moveSpeed * Time.deltaTime);
		}
	}
	//ブースト移動処理の適用.
	void ApplyBoost(){
		//-ブースト入力方向所得-.
		boostDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
		//-ブースト入力を方向情報のみに限定する-.
		boostDirection = transform.TransformDirection(boostDirection);
		//-ブースト起動時の詳細処理-.
		if((Input.GetButton("Jump") == true || Input.GetAxis("Jump") > 0) && boostEnergy > 0){//Jump = SpaceKye割り当て相当 & ブーストゲージが残っていれば.
			boostEnergy -= boostNeedEN * Time.deltaTime;//ブーストゲージを消費して.
			//-ブースト速度の上昇.
			ApplyBoostSpeedUp();
			//-実際のブースト移動.
			if(Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0){//左右,前後どちらへも入力が無ければ.
				//上向きに加速(上昇のみ,通常移動速度の追加補整/*と擬似重力を切る補正をかける*/).	
				
				//collisionFlags = controller.Move(Vector3.up * (moveSpeed + boostSpeed/* + gravity*/) * Time.deltaTime);
			
				}else if(Input.GetAxis("Horizontal") != 0 && Input.GetAxis("Vertical") != 0){//左右,前後の入力が同時に入っていれば.
				//√2で割った値で該当方向へ加速.
				collisionFlags = controller.Move(boostDirection * boostSpeed / Mathf.Sqrt(2.0f) * Time.deltaTime);
			}else{//そうでなければ1方向へ入力なので,
				//通常速度で該当方向へ加速.
				collisionFlags = controller.Move(boostDirection * boostSpeed * Time.deltaTime);;
			}
		}else if((Input.GetButton("Jump") == true || Input.GetAxis("Jump") > 0) && 0 >= boostEnergy){//ブーストの入力があっても,ブーストゲージが残っていなければ.
			ApplyBoostSpeedDown();//ブーストの速度を減速.
		}else if((Input.GetButton("Jump") != true || 0 >= Input.GetAxis("Jump")))
        {//ブーストの入力が無ければ.
			ApplyBoostSpeedDown();//ブーストの速度を減速.
			ApplyBoostEnergyCharge();//ブーストゲージを回復する.
		}
		//-接地している場合でもブーストゲージは回復させる-.
		if(IsGrounded() == true){
			ApplyBoostEnergyCharge();//ブーストゲージを回復する.
		}
		
	}
	//ブースト速度の加速.
	void ApplyBoostSpeedUp(){
		if(boostSpeedMax > boostSpeed){
			boostSpeed += (boostSpeedMax * Time.deltaTime);//加速する.
			if(boostSpeed > boostSpeedMax){//上限値以上になった場合は修正.
				boostSpeed = boostSpeedMax;
			}
		}
	}
	//ブースト速度の減速.
	void ApplyBoostSpeedDown(){
		boostSpeed -= (gravity * Time.deltaTime + Time.deltaTime);//ブーストの速度を減速.
		if(0 > boostSpeed){//0以下になった場合は修正.
			boostSpeed = 0;
		}
	}
	//ブーストゲージの回復.
	void ApplyBoostEnergyCharge(){
		boostEnergy += Time.deltaTime * boostEnergyMax/20;//ブーストゲージを回復する.
		if(boostEnergy > boostEnergyMax){//上限値以上になったら修正.
			boostEnergy = boostEnergyMax;
		}
	}
	//ジャンプ(短距離上方加速)の適用処理.
	void ApplyJumpBoost(){
		//入力方向の取得はboostDirectionを流用.
		if(IsGrounded() == true && Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0 &&
            (Input.GetButton("Jump") == true || Input.GetAxis("Jump") > 0) && boostEnergy > 0){//接地状態 && ニュートラル && ブースト入力(押下)有り && ブーストゲージ残量有.
			boostEnergy -= quickBoostNeedEN/* * Time.deltaTime*/;//この行動一回あたりに必要なエネルギーを消費して.
			//-瞬間的な加速を行う-.
			collisionFlags = controller.Move(Vector3.up * (moveSpeed + boostSpeedMax * 2/* + gravity*/) * Time.deltaTime);
		}
	}
	//クイックブースト(サイドステップ的な短距離加速)の適用処理.
	void ApplyQuickBoost(){
        //ここに処理を書きます.
        //入力方向の取得はboostDirectionを流用.
        if ((Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) &&
            (Input.GetButton("Jump") == true || Input.GetAxis("Jump") > 0) && boostEnergy > 0 && quickBoostUsed == false){//非ニュートラル && ブースト入力(押下)有り && ブーストゲージ残量有..
			boostEnergy -= quickBoostNeedEN/* * Time.deltaTime*/;//この行動一回あたりに必要なエネルギーを消費して.
			//-瞬間的な加速を行う-.
			quickBoostUsed = true;
			quickBoostSpeed = quickBoostSpeedMax;
			collisionFlags = controller.Move(boostDirection * (moveSpeed + quickBoostSpeed) * Time.deltaTime);
		}else{
			//そうで無ければ減速する.
			quickBoostUsed = false;
			quickBoostSpeed -= (gravity*Time.deltaTime + Time.deltaTime);
			if(0 > quickBoostSpeed){
				quickBoostSpeed = 0;
			}
		}
	}
	//ブーストエフェクトのON/OFF.
	void ApplyBoostEffectControl(){
		if((Input.GetButton("Jump") == true || Input.GetAxis("Jump") > 0) && boostEnergy > 0){
			boostEffect.SetActive(true);
		}else{
			boostEffect.SetActive(false);
		}
		
	}
	//-ロックオン関連処理-()
	private void ApplyLockOn()
	{
		//ToDo: ダメだったので書き直し
	}

	//擬似重力の適用.
	void ApplyGravity(){
		if(IsGrounded() != true || 0 >= boostEnergy){//接地しておらず,ブースト移動もしていない,またはブーストゲージが0であれば.
			//collisionFlags = controller.Move(Vector3.down * gravity * Time.deltaTime);
			moveDirection.y = -gravity * Time.deltaTime;
		}
	}
	//接地しているか否か.
	bool IsGrounded(){
		/*
		Debug.Log("collisionFlags = " + collisionFlags);
		Debug.Log("controller.isGrounded = " + controller.isGrounded);
		Debug.Log("(collisionFlags & CollisionFlags.CollidedBelow) != 0 " + (collisionFlags & CollisionFlags.CollidedBelow));
		*/
		return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
	}
	
}
