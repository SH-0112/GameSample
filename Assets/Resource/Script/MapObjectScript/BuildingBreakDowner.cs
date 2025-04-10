using UnityEngine;
using System.Collections;

[System.Serializable]
//インスペクターに構造体を表示できないため,classで代用
public class ModelData {//崩壊後の破片モデル用のデータをまとめたclass
	public GameObject fragmentObject;//破片モデル登録用
	public Vector3 appearancePos;//出現位置(元オブジェクトとの相対的な位置を指定)
}

[System.Serializable]
public class BuildingBreakDowner : MonoBehaviour {
	//更新:2013/03/24
	public float hp = 1000.0f;//オブジェクトの耐久度
	public bool isBreakDownSetup = false;//崩壊処理実行前の設定を開始するか否か
	public bool isBreakDownStart = false;//崩壊処理の実行を開始するか否か
	public float breakDownFinTime = 3.0f;//崩壊にかける(元オブジェクトが落下しきるまでの)時間(秒)
	public float breakDownDist = 100.0f;//崩壊時に元オブジェクトの落下、沈下する距離
	private Vector3 breakDownFinPos;//崩壊時の元オブジェクトの移動終了地点登録用
	public ModelData[] fragmentModelDatas;//崩壊後の破片モデル登録用
	public GameObject smokeEffect;//オブジェクトと地面の設置面を隠す煙エフェクト登録用
	
	// Use this for initialization
	void Start () {
			
	}
	
	// Update is called once per frame
	void Update () {
		if(isBreakDownSetup == true){
			BreakDownSetup();//崩壊処理に必要な数値の設定,破片モデルの生成を実行
		}
		if(isBreakDownStart == true){
			BreakDownRun();//崩壊処理を実行
		}			
	}
	//崩壊処理に必要な数値の設定,破片モデルの生成を実行する処理
	void BreakDownSetup(){
		//崩壊時の元オブジェクト移動終了地点を計算
		breakDownFinPos = new Vector3(transform.position.x, transform.position.y - breakDownDist, transform.position.z);
		//煙エフェクトを生成
		Instantiate(smokeEffect, new Vector3(transform.position.x, 0.0f, transform.position.z), Quaternion.identity);
		//登録されている破片モデルを全て生成
		foreach(ModelData fragmentModelData in fragmentModelDatas){
			Instantiate(fragmentModelData.fragmentObject, transform.position+fragmentModelData.appearancePos, fragmentModelData.fragmentObject.transform.rotation);			
		}
		isBreakDownStart = true;//崩壊処理を実行するためture
		isBreakDownSetup = false;//崩壊処理の実行前の設定が終了したので繰り返さないようにfalse
	}
	//崩壊処理の実行部分
	void BreakDownRun(){
		//線形補完で元オブジェクトを移動(現在位置から目標位置までの距離を1として,崩壊にかける時間で距離を割り,前フレームからの秒数をかける事で移動距離を算出する)
		transform.position = Vector3.Lerp(transform.position, breakDownFinPos, (1.0f/breakDownFinTime)*Time.deltaTime);
		//崩壊にかける時間を過ぎたら,目標位置に到達していてもいなくても削除する
		Destroy(gameObject, breakDownFinTime);	

	}
	//命令を受けとって崩壊処理を開始する際の受信部分
	void ApplyBreakDown(){
		isBreakDownSetup = true;
	}
	//オブジェクトの耐久度を削るダメージ処理
	void ApplyDamage(float argDamage){
		hp -= argDamage;
		if(hp <= 0){
			hp = 0;
			ApplyBreakDown();
		}
	}
}
