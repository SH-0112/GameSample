using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatus_CommonEnemy_Musou : UnitStatus_CommonEnemy
{
	//　HP表示用UI
	[SerializeField]
	private GameObject HPUI;
	[SerializeField]
	//　HP表示用スライダー
	private Slider hpSlider;

	public override void Start()
	{
		base.Start();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
		//hpSlider = HPUI.transform.Find("HPBar").GetComponent<Slider>();
		hpSlider.value = 1f;
	}

	// Update is called once per frame
	public override void Update()
	{
		base.Update();//継承クラスなので基底クラスで必要な処理をしている場合override後,基底のStartを呼ぶ.
		ApplyDeath(deadDestroyDelay);
		UpdateHPValue();
	}

	//無双敵用死亡処理
	public override void ApplyDeath(float argDestroyTime = 0)
	{
		base.ApplyDeath(argDestroyTime);
		if (GetNowHP() <= 0)
		{
			GameData.Instance.AddEnemyDestroyNum();
		}
	}

	public void UpdateHPValue()
	{
		hpSlider.value = (float)nowHp / (float)maxHp;
	}

}
