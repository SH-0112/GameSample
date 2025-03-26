using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessEnemyAnimEvent : MonoBehaviour
{
	private Enemy enemy;
	[SerializeField]
	private SphereCollider[] sphereColliders;

	void Start()
	{
		enemy = GetComponent<Enemy>();
	}

	void AttackStart()
	{
		for (int i = 0; i < sphereColliders.Length; i++)
		{
			sphereColliders[i].enabled = true;
		}
	}

	public void AttackEnd()
	{
		for (int i = 0; i < sphereColliders.Length; i++)
		{
			sphereColliders[i].enabled = false;
		};

	}

	public void StateEnd()
	{
		enemy.SetState(Enemy.EnemyState.Freeze);
	}
}
