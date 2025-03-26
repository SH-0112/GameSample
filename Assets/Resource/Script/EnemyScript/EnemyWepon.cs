using UnityEngine;
using System.Collections;

public class EnemyWepon : MonoBehaviour {
	
	//Level--------------------------------
	public bool easy = true;
	public bool normal = false;
	public bool hard = false;
	public bool insanity = false;
	//ShotType-----------------------------
	public bool aimingShot = false;
	public bool aimingShotOnlyX = false;
	//ShotSpin------------------------------
	public bool shotAxisRotationX = false;
	public float AddAxisRotationX = 0.0f;
	public bool shotAxisRotationY = false;
	public float AddAxisRotationY = 0.0f;
	public bool shotAxisRotationZ = false;
	public float AddAxisRotationZ = 0.0f;
	//BulletControl&LevelControl------------
	//firePos
	public float firePosX = 0.0f;
	public float firePosY = 0.0f;
	public float firePosZ = 0.0f;
	//baseAngle
	public float baseAngleX = 0.0f;
	public float baseAngleY = 0.0f;
	public float baseAngleZ = 0.0f;
	//RandomAngle
	public bool randomShotX = false;
	public float randAngleX = 0.0f;
	private float randAngleAnswerX = 0.0f;
	public bool randomShotY = false;
	public float randAngleY = 0.0f;
	private float randAngleAnswerY = 0.0f;
	public bool randomShotZ = false;
	public float randAngleZ = 0.0f;
	private float randAngleAnswerZ = 0.0f;
	//SpaceAngle
	private float spaceAngleX = 0.0f;
	public float spaceAngleX_E = 0.0f;
	public float spaceAngleX_N = 0.0f;
	public float spaceAngleX_H = 0.0f;
	public float spaceAngleX_I = 0.0f;
	private float spaceAngleY = 0.0f;
	public float spaceAngleY_E = 0.0f;
	public float spaceAngleY_N = 0.0f;
	public float spaceAngleY_H = 0.0f;
	public float spaceAngleY_I = 0.0f;
	private float spaceAngleZ = 0.0f;
	public float spaceAngleZ_E = 0.0f;
	public float spaceAngleZ_N = 0.0f;
	public float spaceAngleZ_H = 0.0f;
	public float spaceAngleZ_I = 0.0f;
	/*public float speed;*/
	//Way
	private int wayX = 1;
	public int wayX_E = 1;
	public int wayX_N = 1;
	public int wayX_H = 1;
	public int wayX_I = 1;
	private int wayY = 1;
	public int wayY_E = 1;
	public int wayY_N = 1;
	public int wayY_H = 1;
	public int wayY_I = 1;
	private int wayZ = 1;
	public int wayZ_E = 1;
	public int wayZ_N = 1;
	public int wayZ_H = 1;
	public int wayZ_I = 1;
	private float wayTotalAngleX;
	private float wayTotalAngleY;
	private float wayTotalAngleZ;
	//ActionCoolTime----------------------
	public int oneSetActionFrame = 120;
	public int actionFrameRate = 10;
	public int actionLimitFrame = 80;
	public float nowActionFrame = 0.0f;
	//BarrageCoolTime---------------------
	public float timeCounter = 0.0f;
	public float frameCounter = 0.0f;
	//BulletObject------------------------
	public GameObject bullet;
	//Target(Player)Object----------------
	private GameObject target;
	
	

	// Use this for initialization
	void Start () {
		
		target = GameObject.FindWithTag("Player");
		LevelChenge();	
	}
	
	// Update is called once per frame
	void Update () {
				
		timeCounter += Time.deltaTime;
		frameCounter = Time.frameCount;
		nowActionFrame = frameCounter%oneSetActionFrame/*timeCounter%2*/;
		if(nowActionFrame%actionFrameRate == 0 && nowActionFrame<actionLimitFrame/*1*/ ){
			BarrageControl();
		}
				
	}
	//Level Chenge
	void LevelChenge(){
		if((easy == true)){
			spaceAngleX = spaceAngleX_E;
			spaceAngleY = spaceAngleY_E;
			spaceAngleZ = spaceAngleZ_E;
			wayX = wayX_E;
			wayY = wayY_E;
			wayZ = wayZ_E;			
		}
		else if((normal == true)){
			spaceAngleX = spaceAngleX_N;
			spaceAngleY = spaceAngleY_N;
			spaceAngleZ = spaceAngleZ_N;
			wayX = wayX_N;
			wayY = wayY_N;
			wayZ = wayZ_N;
		}
		else if((hard == true)){
			spaceAngleX = spaceAngleX_H;
			spaceAngleY = spaceAngleY_H;
			spaceAngleZ = spaceAngleZ_H;
			wayX = wayX_H;
			wayY = wayY_H;
			wayZ = wayZ_H;
		}
		else if((insanity == true)){
			spaceAngleX = spaceAngleX_I;
			spaceAngleY = spaceAngleY_I;
			spaceAngleZ = spaceAngleZ_I;
			wayX = wayX_I;
			wayY = wayY_I;
			wayZ = wayZ_I;
		}		
	}
	
	void BarrageControl(){
		//BarrageClac
		wayTotalAngleX = spaceAngleX*(wayX-1);
		wayTotalAngleY = spaceAngleY*(wayY-1);
		wayTotalAngleZ = spaceAngleZ*(wayZ-1);
		
		//BarrageSpread
		for(int i = 0; i<wayX; i++){
			for(int j = 0; j<wayY; j++){
				for(int k = 0; k<wayZ; k++){
					AimingShotClac();//Aiming or OtherShot
					ShotAxisRotationClac();//Rotation or NotRotation
					RandomShotAngleClac();//RandomShot or NotRandom
					//↓BarrageSpredClacMain
					Quaternion barrageAngle = Quaternion.Euler(baseAngleY+randAngleAnswerY-(wayTotalAngleY/2)+spaceAngleY*j, baseAngleX+randAngleAnswerX-(wayTotalAngleX/2)+spaceAngleX*i, baseAngleZ+randAngleAnswerZ-(wayTotalAngleZ/2)+spaceAngleZ*k);
					Instantiate(bullet,transform.TransformPoint(firePosX,firePosY,firePosZ),barrageAngle);
				}
			}
		}		
	}
	
	//AimingShotClac
	void AimingShotClac(){
		if(aimingShot == true){//AimingShot
			Quaternion baseAngle = Quaternion.LookRotation(target.transform.position-transform.position);
			baseAngleX = /*baseAngle.y;*/baseAngle.eulerAngles.y;
			baseAngleY = /*baseAngle.x;*/baseAngle.eulerAngles.x;
			//baseAngleZ = /*baseAngle.z;*/baseAngle.eulerAngles.z;							
		}
		if((aimingShotOnlyX == true)&&(aimingShot == false)){
			Quaternion baseAngle = Quaternion.LookRotation(target.transform.position-transform.position);
			baseAngleX = /*baseAngle.y;*/baseAngle.eulerAngles.y;														
		}
	}
	
	//shotAxisRotationClac
	void ShotAxisRotationClac(){
		if(shotAxisRotationX == true){
			baseAngleX += AddAxisRotationX*Time.time;//.frameCount;
		}
		if(shotAxisRotationY == true){
			baseAngleY += AddAxisRotationY*Time.time;//.frameCount;
		}
		if(shotAxisRotationZ == true){
			baseAngleZ += AddAxisRotationZ*Time.time;//.frameCount;
		}		
	}
	//randomShotAngleClac
	void RandomShotAngleClac(){
		if(randomShotX == true){
			randAngleAnswerX = Random.Range(-randAngleX/2.0f,randAngleX/2.0f);
		}
		if(randomShotY == true){
			randAngleAnswerY = Random.Range(-randAngleY/2.0f,randAngleY/2.0f);
		}
		if(randomShotZ == true){
			randAngleAnswerZ = Random.Range(-randAngleZ/2.0f,randAngleZ/2.0f);
		}
	}
	
	//SendBulletSpeed
	void SendBulletSpeed(){
		
	}
	
	//ApplyLevelChange
	void ApplyLevelChange(){
		
	}
	
}
