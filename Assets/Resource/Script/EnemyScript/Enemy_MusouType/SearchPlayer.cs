using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchPlayer : MonoBehaviour
{
	private Enemy enemy;

	void Start()
	{
		enemy = GetComponentInParent<Enemy>();
	}

	void OnTriggerStay(Collider col)
	{
		//　プレイヤーキャラクターを発見
		if (col.tag == "Player")
		{
			//　敵キャラクターの状態を取得
			Enemy.EnemyState state = enemy.GetState();
			//　敵キャラクターが追いかける状態でなければ追いかける設定に変更
			if (state == Enemy.EnemyState.Wait || state == Enemy.EnemyState.Walk)
			{
				enemy.SetState(Enemy.EnemyState.Chase, col.transform);
			}
		}
	}

	void OnTriggerExit(Collider col)
	{
		if (col.tag == "Player")
		{
			Debug.Log("見失う");
			enemy.SetState(Enemy.EnemyState.Wait);
		}
	}
}