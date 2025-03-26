using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ActiveAreaData{
	public Vector3 mapSize = new Vector3(500.0f, 0.0f, 500.0f);
	public Vector3 activeAreaMax = new Vector3(100.0f, 0.0f, 100.0f);//移動可能範囲の上限(座標は決め打ちで).
	public Vector3 activeAreaMin = new Vector3(-100.0f, 0.0f, -100.0f);//移動可能範囲の下限(座標は決め打ちで).
	public bool isAutoEntry = false;//移動可能範囲を自動計算(初期座標からの相対的な立方体で設定)するか否か.
	public Vector3 autoEntryAreaScope = new Vector3(200.0f, 0.0f, 200.0f);//移動可能範囲を自動計算する際の各方向の範囲
	
}
[System.Serializable]
public class RandomMoveData{
	//Inspector上に表示する必要が無く,かつprotectedにせず参照したいのでinternalに
	internal Vector3 goalPos;		//ランダム移動の到着地点登録用.
	public float moveDist = 30.0f;	//ランダム移動で必ず移動する距離.
	public float timeLimit = 3f;	//ランダム移動を打ち切るまでの時間(秒).
	internal float finTime;			//ランダム移動終了時間登録用.
	internal bool isMoveNow = false;//ランダム移動中か否か.
}

//このクラスを使用するには継承を行わなくてはならない(戒め).
//継承後はBase部分をユニークな単語に置き換える.
[System.Serializable]
public abstract class EnemyAI_Base : MonoBehaviour {
	/*陸上ユニット,航空ユニットで分けられるようにしたい所ネー. */
	
	//-ステータス関連-.
	//移動速度,範囲などに関連するステータス.
	public float rotateSpeed = 5.0f;//旋回の速さ.
	public float moveSpeed = 15.0f;//移動速度.
	public ActiveAreaData activeAreaData;
	public RandomMoveData randomMoveData;
	//HPなどユニットの死亡判定,状態遷移などに使用するステータス.
	//--継承後のクラスで用途に合うUnitStatusを記述する--
	//-メカニム関連-.
	public Animator animator;
	//-Ray-.
	public RaycastHit hitInfo;	
	public LineRenderer searchLine;
	//-Target(Player)Object-.
	protected GameObject target;
	
	// Use this for initialization
	//--処理があれば,必ず継承クラスでoverrideしてこの基底クラスのStartを呼ぶ→base.Start()--.
	public virtual void Start () {//Startを仮想関数化しておく.
		//Targetのセットアップ.
		target = GameObject.FindWithTag("Player");
		//UnitStatusのセットアップ.
		//--継承後のクラスで用途に合うUnitStatusをGetComponent<hoge>()する--
		//Aninmatorのセットアップ.
		animator = GetComponent<Animator>();
		//PlayerSerchLine Setup.
		searchLine = GetComponent<LineRenderer>();
        searchLine.positionCount = 2;
	}
	
	// Update is called once per frame
	//--処理があれば,必ず継承クラスでoverrideしてこの基底クラスのUpdateを呼ぶ→base.Update()--.
	public virtual void Update () {//Updateを仮想関数化しておく.
	
	}
	//-[純粋仮想関数]アニメーション管理部-.
	public abstract void AnimationControl();
	//-[純粋仮想関数]行動パターン管理部-.
	public abstract void ActionPatternControl();
	//-[純粋仮想関数]それぞれの行動パターン毎の記述-.
	public abstract void SystemCombatMode();
	public abstract void SystemScanMode();
	public abstract void SystemIdlingMode();
	//-[純粋仮想関数]武装オブジェクトへのアクセス-.
	public abstract void SendWeponAttackOrder();
	public abstract void SendWeponStopOrder();
	//-思考パターンを組み立てるパーツ-.
	//対象との間に障害物があるかないかを調べる(情報はhitInfoに格納される).
	protected void SearchRay(GameObject argTarget){//※この機能を使用する場合はLineRenderが必要.
		Vector3 thisUnitPos = new Vector3(transform.position.x, transform.position.y-5.0f, transform.position.z);
		Physics.Linecast(thisUnitPos,argTarget.transform.position, out hitInfo);	
		searchLine.SetPosition(0,thisUnitPos);
		searchLine.SetPosition(1,hitInfo.point);
	}
	//対象との距離を調べて返す.
	protected float SearchDistance(GameObject argTarget){
		float distance = Vector3.Distance(transform.position, argTarget.transform.position);
		return distance;
	}
	//-行動パターンを組み立てるパーツ-.
	//対象の方向を向く旋回.
	protected void ApplyTargetTurn(GameObject argTarget){
		Vector3 targetPos = new Vector3(argTarget.transform.position.x, transform.position.y, argTarget.transform.position.z);//y軸の座標を同じにすることで謎のもぐりこみを回避.
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPos - transform.position), Time.deltaTime *rotateSpeed);
	}
	//対象指定簡易追跡移動.
	protected void ApplyMoveChase(GameObject argTarget){
		if(SearchDistance(argTarget) < 20.0f){
			return;
		}
		Vector3 targetPos = new Vector3(argTarget.transform.position.x, transform.position.y, argTarget.transform.position.z);//y軸の座標を同じにすることで謎のもぐりこみを回避
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetPos - transform.position), Time.deltaTime *rotateSpeed);
		transform.Translate(Vector3.forward*moveSpeed*Time.deltaTime,Space.Self);
		//※移動範囲の制限.
		ApplyActiveAreaClamp();
	}
	//簡易ランダム移動.
	protected void ApplyMoveRandom(float argDist){
		if(randomMoveData.isMoveNow == false){//移動前であれば目標地点を決定.
			for(int i=0; i<1000; i++){
				float angle = Random.Range(-180,180)*Mathf.PI/180.0f;
				float xPos = argDist*Mathf.Cos(angle);
				float zPos = argDist*Mathf.Sin(angle);
				randomMoveData.goalPos = new Vector3(transform.position.x + xPos, transform.position.y, transform.position.z+zPos);
				//for文の回る試行回数内で移動可能範囲内の目標地点が出れば.
				if(activeAreaData.activeAreaMax.x > randomMoveData.goalPos.x && randomMoveData.goalPos.x > activeAreaData.activeAreaMin.x
					&& activeAreaData.activeAreaMax.z > randomMoveData.goalPos.z && randomMoveData.goalPos.z > activeAreaData.activeAreaMin.z){
					randomMoveData.finTime = Time.time + randomMoveData.timeLimit;//ランダム移動の終了時間を指定.
					randomMoveData.isMoveNow = true;//ランダム移動実行中に切り替える.
					return;
				}
			}
		}
		if(randomMoveData.isMoveNow == true){//ランダム移動中であれば目標地点へ向かって旋回.
			//パターンA[旋回・移動完全分離型].
			/*if(transform.rotation != Quaternion.LookRotation(randomMoveData.goalPos - transform.position)){
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(randomMoveData.goalPos - transform.position), Time.deltaTime *rotateSpeed);
			}
			else{//旋回が完了していれば移動.
				transform.Translate(Vector3.forward*moveSpeed*Time.deltaTime,Space.Self);
				//※移動範囲の制限.
				ApplyActiveAreaClamp();
				//ランダム移動を打ち切る条件.
				if(transform.position == randomMoveData.goalPos || Time.time > randomMoveData.timeLimit){
					randomMoveData.isMoveNow = false;//目標地点に到着又は時間経過で,次のランダム移動を受け付ける状態へ移行.
				}
			}*/
			//パターンB[旋回・移動同時型].
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(randomMoveData.goalPos - transform.position), Time.deltaTime *rotateSpeed);
			transform.Translate(Vector3.forward*moveSpeed*Time.deltaTime,Space.Self);
			//※移動範囲の制限.
			ApplyActiveAreaClamp();
			//ランダム移動を打ち切る条件.
			if(transform.position == randomMoveData.goalPos || Time.time > randomMoveData.timeLimit){
				randomMoveData.isMoveNow = false;//目標地点に到着又は時間経過で,次のランダム移動を受け付ける状態へ移行.
			}
		}	
	}
	//移動時の移動可能エリア制限.
	protected void ApplyActiveAreaClamp(){
		//※移動範囲の制限.
		transform.position = new Vector3 (
			Mathf.Clamp(transform.position.x, activeAreaData.activeAreaMin.x, activeAreaData.activeAreaMax.x),
			Mathf.Clamp(transform.position.y, 0.0f, transform.position.y),
			Mathf.Clamp(transform.position.z, activeAreaData.activeAreaMin.z, activeAreaData.activeAreaMax.z));
	}
	//-経路探索移動-.
	//経路探索移動は継承後のクラスで用途に合う~PathSearchMove~をGetComponent<hoge>()して使用する.
}
