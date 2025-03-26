using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSE : MonoBehaviour
{
	private AudioSource audioSource;
	//　鳴らす音声クリップ
	[SerializeField]
	private AudioClip spawnSE;

	void Start()
	{
		audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.spatialBlend = 1.0f;//音を3Dにする
		audioSource.PlayOneShot(spawnSE);
	}
}
