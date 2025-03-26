using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UnitStatus_Player : UnitStatus_Base {
	
	//-Player専用のステータス-.
	public ACLikeController acLikeController;//ブーストゲージ取得用.
	public GUISkin skin;
	
	// Use this for initialization
	public override void Start () {
		base.Start();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
		acLikeController = GetComponent<ACLikeController>();//同じGameObjectのACLikeControllerを取得.
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
		base.ApplyDeath(deadDestroyDelay);
		UpdateUI();
	}
	//GUIの描画.
	/*/コメントアウト中
	void OnGUI(){
		//-GUISkinの適用-.
		GUI.skin = skin;
		
		//-GUI本体の描画[とりあえず暫定で直討ち]-.
		//HP描画.
		GUILayout.BeginArea(new Rect(Screen.width/2 - 150, Screen.height/2 - 25, 100, 50));
			GUILayout.BeginVertical();
			GUILayout.Label("HP \n"+ nowHp);
			//GUILayout.Label("EN "+ Mathf.Round(acLikeController.boostEnergy/acLikeController.boostEnergyMax * 100.0f) + "%");
			//GUILayout.Box
			GUILayout.EndVertical();
		GUILayout.EndArea();
		//EN描画.
		GUILayout.BeginArea(new Rect(Screen.width/2 + 75, Screen.height/2 - 25, 100, 50));
			GUILayout.BeginVertical();
			//GUILayout.Label("HP "+ nowHp);
			GUILayout.Label("EN \n"+ Mathf.Round(acLikeController.boostEnergy/acLikeController.boostEnergyMax * 100.0f) + "%");
			//GUILayout.Box
			GUILayout.EndVertical();
		GUILayout.EndArea();
	}
	*/

	//--新規UI対応建造用--
	public Image gaugeImageHP;
	public Image gaugeImageEN;
	public Text textHP;
	public Text textEN;

	private void UpdateUI()
	{
		//HPゲージ
		gaugeImageHP.transform.localScale = new Vector3(GetNowHP() / GetMaxHP(), 1, 1);
		textHP.text = "HP " + GetNowHP();
		//ENゲージ
		gaugeImageEN.transform.localScale = new Vector3(acLikeController.boostEnergy / acLikeController.boostEnergyMax, 1, 1);
		textEN.text = "EN " + Mathf.Round(acLikeController.boostEnergy / acLikeController.boostEnergyMax * 100.0f) + "%";

	}
}
