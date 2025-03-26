using UnityEngine;
using System.Collections;

//継承する場合はBase部分をユニークな単語に置き換える.
public class UnitStatus_Base : MonoBehaviour {
	
	//-基本ステータス-.
	public float maxHp = 2000.0f;		//HP最大値.
	public float nowHp;					//HP現在値.
	public float accumlatedDamage = 0f;	//蓄積ダメージ.
	public float deadDestroyDelay = 0.0f;//死亡判定が入った際にDestroyされるまでの猶予時間.
	public GameObject[] setDeadEffect;		//死亡時のエフェクト(設置済みのものをInspectorから登録する).
	public GameObject[] generateDeadEffect; //死亡時のエフェクト(生成する場合).
	//死亡時SE
	[SerializeField]
	private AudioClip deathSE = null;
	private AudioSource deathAudioSource = null;
	//-カメラに写っているか-
	private const string MAIN_CAMERA_TAG_NAME = "MainCamera";//メインカメラに付いているタグ名
	private bool isRendered = false;//カメラに表示されているか


	// Use this for initialization
	public virtual void Start () {
		nowHp = maxHp;
	}
	
	// Update is called once per frame
 	public virtual void Update () {
		if (isRendered) {
			isRendered = false;//正常動作
			//isRendered = true;//デバッグ用:全対象をロックオンする場合
			Debug.Log("isRenderd = false");
		}
	}
	//回復処理.
	public void ApplyHeal(float argHealPoint){
		nowHp += argHealPoint;
		//上限値突破時の修正.
		if(nowHp > maxHp){
			nowHp = maxHp;
		}
	}
	//被ダメージの適用処理.
	public void ApplyDamage(float argDamage){
		nowHp -= argDamage;				//現在HPから減算.
		accumlatedDamage += argDamage;	//蓄積ダメージを加算.
		//下限値突破時の修正.
		if(0 > nowHp){
			nowHp = 0;
		}
	}
	//現在HPを返す
	public float GetNowHP()
	{
		return nowHp;
	}

	public float GetMaxHP()
	{
		return maxHp;
	}

	//死亡時のDestroy処理(引数を入力しなければデフォルト入力の引数が適用される).
	public virtual void ApplyDeath(float argDestroyTime = 0.0f){
		if(0 >= nowHp){
			foreach(GameObject setDeadEffects in setDeadEffect){//設置済みのもの用.
				if(setDeadEffects != null){//死亡時エフェクトがあれば.
					//Instantiate(deadEffect, transform.position, transform.rotation);//生成する.
					setDeadEffects.SetActive(true);//登録されている物を有効化する.
				}	
			}
			foreach(GameObject generateDeadEffects in generateDeadEffect){//生成する場合用.
				if(generateDeadEffects != null){
					Instantiate(generateDeadEffects, transform.position, transform.rotation);
				}
			}
			PlayDeathSE();
			Destroy(gameObject,argDestroyTime);
		}
	}

	//死亡時のSE再生
	private void PlayDeathSE()
	{
		if (deathSE == null)
		{
			return;
		}
		deathAudioSource = gameObject.AddComponent<AudioSource>();
		deathAudioSource.spatialBlend = 0.5f;//音を3Dにする
		deathAudioSource.PlayOneShot(deathSE);
	}

	//カメラに映っているかの取得
	public bool GetIsRendered()
	{
		return isRendered;
	}

	//カメラに映ってる間に呼ばれる(Updateの後に呼ばれる)
	private void OnWillRenderObject()
	{
		//メインカメラに映った時だけ_isRenderedを有効に
		if (Camera.current.tag == MAIN_CAMERA_TAG_NAME)
		{
			isRendered = true;
		}
	}

}
