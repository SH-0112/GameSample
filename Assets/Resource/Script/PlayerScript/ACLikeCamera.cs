using UnityEngine;
using System.Collections;

public class ACLikeCamera : MonoBehaviour {
	//使用方法：カメラオブジェクトにぶち込むタイプのCameraMoveScript[2013/10/05作成].
	//
	//[ToDo].
	//水平状態よりも下を見下ろした時に,プレイヤーを画面内に収めるようカメラそのものが上に移動するようにする.
	//追従移動用---------------------------.
	public Transform target;//プレイヤーを登録するための変数.
	//-移動関連-.
	public float distance = 3.0f;
	public float width = 1.0f;//カメラの横の寄せ幅(視界確保用).
	private float widthL;//カメラを左に寄せる際に使う値.
	private float widthR;//カメラを右に寄せる際に使う値.
	public float height = 1.0f;//高さ.
	public float speed = 8.0f;//追従速度(デフォルトは8.0f).
	//-回転関連-.	
	public float rotationSpeed = 5.0f;//回転速度.
	public float rotationY = 0.0f;//.
	public float rotationYMin = -60.0f;//縦回転下限.
	public float rotationYMax = 60.0f;//縦回転上限.
	//-Raycast関連-.
	public Ray ray;
	public RaycastHit hitInfo;
	public float rayDistance = 1000.0f;//Rayの有効射程.
	public GameObject rayTarget;	//RayがGameObjectぶつかった際[誘導弾,ロックオンなどの処理を作る際に].
	public Vector3 rayTargetPos;	//Rayがぶつかった座標を保持[通常弾頭の射出角度(銃身の向き)を決める処理を作る際に].
	private Vector3 windowCenter;	//画面中央座標の保管用.
	

	// Use this for initialization
	void Start () {
		//マウスカーソルを非表示(書き出した時に適用される).
		Cursor.visible = false;
        //マウスカーソルを画面外へ出ないように.
#if UNITY_4_6
        Screen.lockCursor = true;
#else
        if (Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;//標準モード
        }
        else
        {
            //Cursor.lockState = CursorLockMode.Confined;//ウィンドウ内にとどめる
            Cursor.lockState = CursorLockMode.Locked;//中央にロック
        }
#endif
        //カメラの左右寄せに使う値の処理.
        widthL = -width;
		widthR = width;
		//Raycast関連.
		windowCenter = new Vector3(Screen.width/2, Screen.height/2, 0);//画面中央の座標意をセット.
	}
	
	// Update is called once per frame
	void Update () {
		//--カメラからのRaycast--[武装オブジェクトから発射した際の目標方向指定に使用予定].
		
		//画面中央へRayを飛ばす.
		ray = Camera.main.ScreenPointToRay(windowCenter);
		
		//ナニカにぶつかったら,gameObject取得.
		if(Physics.Raycast(ray, out hitInfo,rayDistance)){
			rayTarget = hitInfo.collider.gameObject;
			rayTargetPos = hitInfo.point;
			//Scene画面でのデバッグ用のRay描画(ゲーム画面では描画されない).
			Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * rayDistance, Color.green);
		}else{//何もぶつかっていなければ.
			rayTarget = null;//nullを入れておく.
			rayTargetPos = transform.TransformDirection(Vector3.forward) * rayDistance;//Rayの限界射程の座標を入力.
		}
		//--カメラ操作--.
		//-追従移動-.
		if(0 > Input.GetAxis("Horizontal")){//または左が入力されていれば.
			//視界確保のため,カメラを右へ寄せる.
			width = widthR;
		}else if(Input.GetAxis("Horizontal") > 0){//そうでなければ(右が入力されて入れば).
			//視界確保のため,カメラを左へ寄せる.
			width = widthL;
		}	
		//プレイヤーの現時位置と比べて相対的な移動先を算出.
		Vector3 wantedPosition;//カメラの移動先を保持する変数.
		//縦回転の上限値付近での異常挙動を回避するため+1して条件とする(以降の計算に影響無し).
		if(rotationYMax+1 >= transform.eulerAngles.x && transform.eulerAngles.x >= 0){//縦回転上限~0度の間であれば.
			wantedPosition = target.TransformPoint(0.0f + width,
				0.0f + height + Mathf.Sin(transform.eulerAngles.x * Mathf.Deg2Rad) * distance,//Math.Deg2Rad[度数法を弧度法へ変換する].
				0.0f - Mathf.Cos(transform.eulerAngles.x * Mathf.Deg2Rad) * distance);
		}else{
			//カメラ自身の傾きから移動量の補正を算出,適用.
			float heightDistance = distance - distance * 1/2 * (360.0f - transform.eulerAngles.x) / -rotationYMin;
			wantedPosition = target.TransformPoint(0.0f + width,
				0.0f + height + Mathf.Sin(transform.eulerAngles.x * Mathf.Deg2Rad) * heightDistance,//Math.Deg2Rad[度数法を弧度法へ変換する].
				0.0f - Mathf.Cos(transform.eulerAngles.x * Mathf.Deg2Rad) * distance);	
		}
		//実際に移動させる.
		transform.position = Vector3.Lerp (transform.position, wantedPosition, Time.deltaTime * speed);//現在位置から徐々に変数へ入れた位置へ移動する.
		
		//-旋回-:[Y軸回転]はtargetのローカルの向きに合わせる,[X軸回転]はマウスの[縦(Y)方向]移動量で変化させる.
		//rotationY += Input.GetAxis("Mouse Y") * rotationSpeed;//回転量の計算.
        rotationY += Input.GetAxis("Vertical2") * rotationSpeed;
		rotationY = Mathf.Clamp (rotationY, rotationYMin, rotationYMax);//回転量の制限.
		transform.localEulerAngles = new Vector3(-rotationY, target.localEulerAngles.y, 0.0f);
	}
}
