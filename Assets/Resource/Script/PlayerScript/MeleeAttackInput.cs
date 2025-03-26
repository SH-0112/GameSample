using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackInput : MonoBehaviour
{
	//-Animator取得用-
	private Animator animator;
	private AnimatorStateInfo stateInfo;
	public MeleeAttackHit meleeAttackHit;
	public GameObject meleeAttackEffectObj;
	public Transform meleeAttackEffectTrans;
	private AudioSource audioSource;
	//　鳴らす音声クリップ
	[SerializeField]
	private AudioClip melleeAttackSE;

	// Start is called before the first frame update
	void Start()
    {
		animator = GetComponent<Animator>();//Animator取得
		audioSource = gameObject.AddComponent<AudioSource>();//AudioSource追加
		stateInfo = animator.GetCurrentAnimatorStateInfo(0);

		
	}

    // Update is called once per frame
    void Update()
    {
		if (Input.GetButtonDown("MeleeAttack"))
		{
			animator.SetTrigger("MeleeAttackTrigger");
			//エフェクト再生
			Instantiate(meleeAttackEffectObj, meleeAttackEffectTrans.position, transform.rotation);
			//SE再生
			audioSource.PlayOneShot(melleeAttackSE);
		}
    }
}
