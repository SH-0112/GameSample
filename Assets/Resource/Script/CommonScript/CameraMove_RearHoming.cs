using UnityEngine;
using System.Collections;

public class CameraMove_RearHoming : MonoBehaviour {
	//使用方法：カメラオブジェクトにぶち込むタイプのCameraMoveScript
	//追従移動用---------------------------
	public Transform target;//プレイヤーを登録するための変数
	public float distance = 8.0f;
	public float width = 0.0f;
	public float height = 3.0f;//高さ
	public float speed = 5.0f;//追従速度 3.0f:ばねカメラ(高速移動時遅れて追従する)　5.0f:デフォルト(ほぼ遅延なし)
	public bool smoothRotation = true;//滑らかにカメラの回転を行うか？　falseならカックカクでプレイヤーの方を向く
	public float rotationSpeed = 10.0f;//回転速度
	//任意操作用追加変数---------------------------
	public float xspeed = 100f;//横の回転速度
	public float yspeed = 100f;//縦の回転速度
	public float dy = 1.0f;//カメラの注視点の高さ
	public float dx = 0.0f;//カメラの注視点のx座標位置
	public float x = 0.0f;//private float x = 0.0f;//角度計算用
	public float y = 0.0f;//private float y = 0.0f;//角度計算用
	public float yMinLimit = -20.0f;//縦回転下限
	public float yMaxLimit = 80.0f;//縦回転上限
	
	void Start(){
		Vector3 angles = this.transform.eulerAngles;
		x = angles.y;
		y = angles.x;
	}
	
	void Update () {
		if(Input.GetMouseButton(1)){//マウス右ボタンが押されている際のカメラの任意操作(移動)
			SelfControlCamera();//任意操作カメラ
		}
		else{//それ以外の時のカメラ操作(移動)
			AoutRearHomingCamera();//背面自動追従カメラ
		}
		
	}
	
	//AoutRearHomingCamera()背面自動追従カメラ
	void AoutRearHomingCamera(){
		//カメラの追従移動
		Vector3 wantedPosition = target.TransformPoint(0.0f+width, 0.0f+height, 0.0f-distance);//プレイヤーの現在位置にy+height,z-distanceした変数を作る
		Vector3 targetPosition = new Vector3(target.position.x+dx, target.position.y+dy, target.position.z);//カメラの注視点を指定
		transform.position = Vector3.Lerp (transform.position, wantedPosition, Time.deltaTime * speed);//現在位置から徐々に変数へ入れた位置へ移動する
		//カメラの追従回転
		if (smoothRotation == true) {
			Quaternion wantedRotation = Quaternion.LookRotation(targetPosition - transform.position, target.up);
			transform.rotation = Quaternion.Slerp (transform.rotation, wantedRotation, Time.deltaTime * rotationSpeed);//現在の向きから徐々にプレイヤーの向きへと向く
		}
		else{
			transform.LookAt (targetPosition, target.up);
		}
	}
	
	//elfControlCamera()任意操作カメラ
	void SelfControlCamera(){
		x += Input.GetAxis ("Mouse X") * xspeed * 0.1f;
		y -= Input.GetAxis ("Mouse Y") * yspeed * 0.1f;
		
		y = ClampAngle(y, yMinLimit, yMaxLimit);
			
		Quaternion rotation = Quaternion.Euler (y, x, 0.0f);
		Vector3 position = rotation * new Vector3(0.0f, height, -distance) + target.position;
			
		this.transform.rotation = rotation;
		this.transform.position = position;
		//this.transform.position = Quaternion.Euler(y, x, 0.0f) * new Vector3(0.0f, height, -distance) + target.position;
	}
	
	static float ClampAngle(float angle, float min, float max){
		if(angle < -360.0f){
			angle += 360.0f;
		}
		if(angle > 360.0f){
			angle -= 360.0f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
