using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameData : MonoBehaviour
{
	//シングルトン
	public static GameData Instance { get; private set;}
	//データ群--
	private float clearTime;	//クリアタイム
	private float playerHP;		//プレイヤー残HP
	private int enemyDestroyNum;//撃破数
	private int difficultyLevel;//難易度
	//難易度レベル
	public enum Difficulty
	{ EASY = 0, NORMAL, HARD, UNKNOWN }

	//シングルトン
	void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
	//データ群SetGet
	public float ClearTime
	{
		set	{ clearTime = value; }
		get { return clearTime; }
	}
	public float PlayerHP
	{
		set { playerHP = value; }
		get { return playerHP; }	
	}
	public int EnemyDestroyNum
	{
		set { enemyDestroyNum = value; }
		get { return enemyDestroyNum; }
	}
	public int DifficultyLevel
	{
		set { difficultyLevel = value; }
		get { return difficultyLevel; }	
	}


	//撃破数加算
	public void AddEnemyDestroyNum(int addDestroyNum = 1)
	{
		enemyDestroyNum += addDestroyNum;	
	}

	//難易度文字列取得
	public string GetDefficultyLevelStr()
	{

		return "" + ((Difficulty)Enum.ToObject(typeof(Difficulty), DifficultyLevel)).ToString();	
	}
	
	//スコア初期化
	public void ResetData()
	{
		//リセットするデータを書く--
		clearTime = 0f;
		playerHP = 0f;
		enemyDestroyNum = 0;
	}
}
