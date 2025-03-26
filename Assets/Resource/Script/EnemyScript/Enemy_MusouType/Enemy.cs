using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
	public enum EnemyState
	{
		Walk,
		Wait,
		Chase,
		Attack,
		Freeze
	};

	private CharacterController enemyController;
	private Animator animator;
	//　目的地
	private Vector3 destination;
	//　歩くスピード
	//[SerializeField]
	/*NavMeshAgent採用のためコメントアウト
	//private float walkSpeed = 1.0f;	
	//　速度
	private Vector3 velocity;
	//　移動方向
	private Vector3 direction;
	//　到着フラグ
	private bool arrived;
	*/
	//　SetPositionスクリプト
	private SetPosition setPosition;
	//　待ち時間
	[SerializeField]
	private float waitTime = 5f;
	//　経過時間
	private float elapsedTime;
	// 敵の状態
	private EnemyState state;
	//　プレイヤーTransform
	private Transform playerTransform;
	//　攻撃した後のフリーズ時間
	[SerializeField]
	private float freezeTimeAfterAttack = 0.5f;
	//　エージェント
	private NavMeshAgent navMeshAgent;
	//　回転スピード
	[SerializeField]
	private float rotateSpeed = 45f;

	// Use this for initialization
	void Start()
	{
		enemyController = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();
		setPosition = GetComponent<SetPosition>();
		setPosition.CreateRandomPosition();
		//velocity = Vector3.zero;
		//arrived = false;
		navMeshAgent = GetComponent<NavMeshAgent>();
		elapsedTime = 0f;
		SetState(EnemyState.Walk);
	}

	void Update()
	{
		//　見回りまたはキャラクターを追いかける状態
		if (state == EnemyState.Walk || state == EnemyState.Chase)
		{
			//　キャラクターを追いかける状態であればキャラクターの目的地を再設定
			if (state == EnemyState.Chase)
			{
				setPosition.SetDestination(playerTransform.position);
				navMeshAgent.SetDestination(setPosition.GetDestination());
			}
			//　エージェントの潜在的な速さを設定
			animator.SetFloat("Speed", navMeshAgent.desiredVelocity.magnitude);

			if (state == EnemyState.Walk)
			{
				//　目的地に到着したかどうかの判定
				if (navMeshAgent.remainingDistance < 0.1f)
				{
					SetState(EnemyState.Wait);
					animator.SetFloat("Speed", 0f);
				}
			}
			else if (state == EnemyState.Chase)
			{
				//　攻撃する距離だったら攻撃
				if (navMeshAgent.remainingDistance < 1.2f)
				{
					SetState(EnemyState.Attack);
				}
			}
			//　到着していたら一定時間待つ
		}
		else if (state == EnemyState.Wait)
		{
			elapsedTime += Time.deltaTime;

			//　待ち時間を越えたら次の目的地を設定
			if (elapsedTime > waitTime)
			{
				SetState(EnemyState.Walk);
			}
			//　攻撃後のフリーズ状態
		}
		else if (state == EnemyState.Freeze)
		{
			elapsedTime += Time.deltaTime;

			if (elapsedTime > freezeTimeAfterAttack)
			{
				SetState(EnemyState.Walk);
			}
		}
		else if (state == EnemyState.Attack)
		{
			//　プレイヤーの方向を取得
			Vector3 playerDirection = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z) - transform.position;
			//　敵の向きをプレイヤーの方向に少しづつ変える
			Vector3 dir = Vector3.RotateTowards(transform.forward, playerDirection, rotateSpeed * Time.deltaTime, 0f);
			//　算出した方向の角度を敵の角度に設定
			transform.rotation = Quaternion.LookRotation(dir);
		}
	}

	//　敵キャラクターの状態変更メソッド
	public void SetState(EnemyState tempState, Transform targetObj = null)
	{
		Debug.Log("NowState"+tempState.ToString());

		state = tempState;
		//velocity = Vector3.zero;

		if (tempState == EnemyState.Walk)
		{
			//arrived = false;
			elapsedTime = 0f;
			setPosition.CreateRandomPosition();
			navMeshAgent.SetDestination(setPosition.GetDestination());
			navMeshAgent.isStopped = false;
		}
		else if (tempState == EnemyState.Chase)
		{
			//　待機状態から追いかける場合もあるのでOff
			//arrived = false;
			//　追いかける対象をセット
			playerTransform = targetObj;
			navMeshAgent.SetDestination(playerTransform.position);
			navMeshAgent.isStopped = false;
		}
		else if (tempState == EnemyState.Wait)
		{
			elapsedTime = 0f;
			//arrived = true;
			//velocity = Vector3.zero;
			animator.SetFloat("Speed", 0f);
			animator.SetBool("Attack", false);
			navMeshAgent.isStopped = true;
		}
		else if (tempState == EnemyState.Attack)
		{
			//velocity = Vector3.zero;
			animator.SetFloat("Speed", 0f);
			animator.SetBool("Attack", true);
			navMeshAgent.isStopped = true;
		}
		else if (tempState == EnemyState.Freeze)
		{
			elapsedTime = 0f;
			//velocity = Vector3.zero;
			animator.SetFloat("Speed", 0f);
			animator.SetBool("Attack", false);
		}
	}
	//　敵キャラクターの状態取得メソッド
	public EnemyState GetState()
	{
		return state;
	}
}
