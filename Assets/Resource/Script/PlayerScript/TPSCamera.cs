using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class TPSCamera : MonoBehaviour
{
	public Transform Target;
	public float DistanceToPlayerM = 2f;    // カメラとプレイヤーとの距離[m]
	public float SlideDistanceM = 0f;       // カメラを横にスライドさせる；プラスの時右へ，マイナスの時左へ[m]
	public float HeightM = 1.2f;            // 注視点の高さ[m]
	public float RotationSensitivity = 100f;// 感度

	void Start()
	{
		if (Target == null)
		{
			Debug.LogError("ターゲットが設定されていない");
			Application.Quit();
		}
	}

	void FixedUpdate()
	{
		float rotX = Input.GetAxis("Horizontal2") * Time.deltaTime * RotationSensitivity;
		float rotY = Input.GetAxis("Vertical2") * Time.deltaTime * RotationSensitivity;

		Vector3 lookAt = Target.position + Vector3.up * HeightM;

		// 回転
		transform.RotateAround(lookAt, Vector3.up, rotX);
		// カメラがプレイヤーの真上や真下にあるときにそれ以上回転させないようにする
		if (transform.forward.y > 0.9f && rotY < 0)
		{
			rotY = 0;
		}
		if (transform.forward.y < -0.9f && rotY > 0)
		{
			rotY = 0;
		}
		transform.RotateAround(lookAt, transform.right, rotY);

		// カメラとプレイヤーとの間の距離を調整
		transform.position = lookAt - transform.forward * DistanceToPlayerM;

		// 注視点の設定
		transform.LookAt(lookAt);

		// カメラを横にずらして中央を開ける

		transform.position = transform.position + transform.right * SlideDistanceM;
	}
}
