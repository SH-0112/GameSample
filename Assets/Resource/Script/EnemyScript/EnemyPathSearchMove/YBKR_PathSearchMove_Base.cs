using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CellData{//経路探索を制作する際のCellData.
	public float xMin;
	public float xMax;
	public float zMin;
	public float zMax;
	public int distance;
	public bool block;//falseであれば通過可能.
}
[System.Serializable]
public class MapData{//路探索を制作する際のMapData
	public MapData(int argCellSpritLimit){//x,z軸の分割数(分割数を多くし過ぎるとセルサイズが小さくなり,障害物がセルのサイズを超えてしまう場合があるので注意).
		this.mapCellData = new CellData[argCellSpritLimit, argCellSpritLimit];
		cellSpritLimit = argCellSpritLimit;
	}
	public float mapWidth = 500;//MAPの横幅.
	public float mapDepth = 500;//MAPの奥行.
	public int cellSpritLimit;
	public CellData[,] mapCellData;
	
}
[System.Serializable]
//ステージに応じてMapサイズや分割数(MapDataの引数)を変える場合はBaseの部分をステージを表すユニークな単語にしたクラスを作成する.
//実際に使用する際はGetComponent()して使用する.
public class YBKR_PathSearchMove_Base : MonoBehaviour {

	public MapData mapData = new MapData(10);
	//地形認識のための障害物データ格納用List.
	private List<Vector3> buildingsPosList = new List<Vector3>();
	//経路探索移動の目標地点ランダム選択用の変数.
	public Vector3[] pathSearchTargetPositions;
	public Vector3 pathSearchTargetPos;//目標地点の座標(start関数通過時の初期値は自分の現在座標) 現在デバッグ用にpublic.
	public float pathSearchTargetUpdateTime = 10.0f;//ランダム経路探索移動のターゲット更新時間(秒換算).
	public float pathSearchElapsedTime;//経路探索移動を開始(更新)してからの経過時間.
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void ApplyMakeCostMap(Vector3 argGoalPos){
		//距離マップ初期化.
		for(int i=0; i<mapData.cellSpritLimit; i++){
			for(int j=0; j<mapData.cellSpritLimit; j++){
				mapData.mapCellData[i,j].xMin = -(mapData.mapWidth/2)+(mapData.mapWidth/mapData.cellSpritLimit)*i;
				mapData.mapCellData[i,j].xMax = -(mapData.mapWidth/2)+(mapData.mapWidth/mapData.cellSpritLimit)*(i+1);
				mapData.mapCellData[i,j].zMin = -(mapData.mapDepth/2)+(mapData.mapDepth/mapData.cellSpritLimit)*j;
				mapData.mapCellData[i,j].zMax = -(mapData.mapDepth/2)+(mapData.mapDepth/mapData.cellSpritLimit)*(j+1);
				mapData.mapCellData[i,j].distance = -1;
				mapData.mapCellData[i,j].block = false;
			}
		}
		
		//障害物チェック------------.
		GameObject[] buildings = GameObject.FindGameObjectsWithTag("building4");//building4のタグが付いているGameObject全ての情報を格納.
		foreach(GameObject building in buildings){
			buildingsPosList.Add(building.transform.position);
			//ここから建造再開.list構造に格納したオブジェクトの座標を元にそこから一定範囲へ入らないようにする.
			//再度格納する前には全ての要素をクリアする（参考サイト参照）.
		}
		
		//封鎖セルチェック----------(ここに来るまでに距離マップ,封鎖セルは初期化は済み).
		for(int i=0; i<mapData.cellSpritLimit; i++){
			for(int j=0; j<mapData.cellSpritLimit; j++){
				for(int k=0; k<buildingsPosList.Count; k++){
					if(mapData.mapCellData[i,j].xMin <= buildingsPosList[k].x 
						&& buildingsPosList[k].x <= mapData.mapCellData[i,j].xMax
						&& mapData.mapCellData[i,j].zMin <= buildingsPosList[k].z
						&& buildingsPosList[k].z <= mapData.mapCellData[i,j].zMax){
						mapData.mapCellData[i,j].block = true;//障害物がセル内にあれば,そこを封鎖セルとし通過不能にする.
						//Debug.Log("xMin[i,j]/buildingsPosList[k].x/xMax[i,j] i="+i+"j="+j+"k="+k+"("+map_data[i,j].xMin +"/"+buildingsPosList[k].x+"/"+map_data[i,j].xMax+")" ,gameObject);
						//Debug.Log("zMin[i,j]/buildingsPosList[k].z/zMax[i,j] i="+i+"j="+j+"k="+k+"("+map_data[i,j].zMin +"/"+buildingsPosList[k].z+"/"+map_data[i,j].zMax+")" ,gameObject);
						//Debug.Log("This Cell isBlock! ("+i+","+j+")"+map_data[i,j].block,gameObject);
					}
					else if(mapData.mapCellData[i,j].block != true){//変数k部分のループで封鎖済みセルの情報が入る事もあるので,ここで条件分岐のフィルターをかけてその可能性を排除.
						mapData.mapCellData[i,j].block = false;//障害物が無いor除去されていれば封鎖を解除.
						//Debug.Log("xMin[i,j]/buildingsPosList[k].x/xMax[i,j] i="+i+"j="+j+"k="+k+"("+map_data[i,j].xMin +"/"+buildingsPosList[k].x+"/"+map_data[i,j].xMax+")" ,gameObject);
						//Debug.Log("zMin[i,j]/buildingsPosList[k].z/zMax[i,j] i="+i+"j="+j+"k="+k+"("+map_data[i,j].zMin +"/"+buildingsPosList[k].z+"/"+map_data[i,j].zMax+")" ,gameObject);
						//Debug.Log("This Cell disBlock! ("+i+","+j+")"+map_data[i,j].block,gameObject);
					}
				}
			}
		}
		//Debug.Log("throww!",gameObject);
		//封鎖セルチェックが終了したので障害物情報を破棄.
		buildingsPosList.Clear();

		//チェックすべきIDペアを叩き込むためのリストを作成.
		LinkedList<int> checkListX = new LinkedList<int>();
		LinkedList<int> checkListZ = new LinkedList<int>();
		
		//経路のコスト計算----------------------------------.
		//ゴール地点を最初のポイントとして登録.
		checkListX.AddFirst((int)((argGoalPos.x+(mapData.mapWidth/mapData.cellSpritLimit))/(mapData.mapWidth/mapData.cellSpritLimit))+((mapData.cellSpritLimit/2)-1));//LinkedListの末尾に値valueのノードを作成して追加する.
		checkListZ.AddFirst((int)((argGoalPos.z+(mapData.mapDepth/mapData.cellSpritLimit))/(mapData.mapDepth/mapData.cellSpritLimit))+((mapData.cellSpritLimit/2)-1));
		mapData.mapCellData[checkListX.First.Value, checkListZ.First.Value].distance = 0;//LinkedListの先頭のノードの値を取得して入力.
		
		//Goalから到達できるセルを全探索終了するまでループ.
		while(checkListX.Count!=0 && checkListZ.Count!=0){
			int x = checkListX.First.Value;
			int z = checkListZ.First.Value;
			//X方向ずらしチェック(該当セルの左右).
			for(int i = -1; i <= 1; i+=2){
				if(x+i < 0 || x+i > mapData.cellSpritLimit-1 || mapData.mapCellData[x+i, z].block == true) {
					continue;//範囲外または封鎖セルなら以下の条件分岐などの処理をスキップ.
				}
				//障害物無し&未探索であれば実行.今のセルからの距離に+1した値をセット.
				if(mapData.mapCellData[x+i, z].block == false && mapData.mapCellData[x+i, z].distance == -1){
					mapData.mapCellData[x+i, z].distance = mapData.mapCellData[x, z].distance+1;
					checkListX.AddLast(x+i);
					checkListZ.AddLast(z);
				}
			}
			//Z方向ずらしチェック(該当セルの上下).
			for(int j = -1; j <= 1; j+=2){
				if(z+j < 0 || z+j > mapData.cellSpritLimit-1 || mapData.mapCellData[x, z+j].block == true ) {
					continue;//範囲外または封鎖セルなら以下の条件分岐などの処理をスキップ.
				}
				//障害物無し&未探索であれば実行.今のセルからの距離に+1した値をセット.
				if(mapData.mapCellData[x, z+j].block == false && mapData.mapCellData[x, z+j].distance == -1){
					mapData.mapCellData[x, z+j].distance = mapData.mapCellData[x, z].distance+1;
					checkListX.AddLast(x);
					checkListZ.AddLast(z+j);
				}
			}
			//1セル分の処理が済んだのでリストから削除.
			checkListX.RemoveFirst();
			checkListZ.RemoveFirst();
		}
		//経路コストの計算が終了したので,IDペアの情報を破棄.
		checkListX.Clear();
		checkListZ.Clear();
		return;
	}
	public void ApplyPathSearchMove(Vector3 argGoalPos, float argMoveSpeed, float argRotateSpeed){
		ApplyMakeCostMap(argGoalPos);//ゴール地点を計算.
		int x = (int)(((transform.position.x+(mapData.mapWidth/mapData.cellSpritLimit))/(mapData.mapWidth/mapData.cellSpritLimit))+((mapData.cellSpritLimit/2)-1));
		int z = (int)(((transform.position.z+(mapData.mapDepth/mapData.cellSpritLimit))/(mapData.mapDepth/mapData.cellSpritLimit))+((mapData.cellSpritLimit/2)-1));
		if(x >= 0 && x <= mapData.cellSpritLimit-1 && z >= 0 && z<=mapData.cellSpritLimit-1){//範囲外でないかチェック.
			int nowDist = mapData.mapCellData[x,z].distance;
			int nextX = -1, nextZ = -1;
			if(nowDist != -1){
				//X方向ずらしチェック(該当セルの左右).
				for(int i = -1; i <= 1; i+=2){
					if(x+i < 0 || x+i > mapData.cellSpritLimit-1) {
						//Debug.Log ("X方向範囲外能");
						continue;//範囲外ならスキップ.		
					}
					if(mapData.mapCellData[x+i, z].block == true){
						//Debug.Log ("X方向封鎖セル:map_data("+(x+i)+"/"+z+") = ["+map_data[x+i, z].block+"](X):false=通過可能,true=通過不能");.
						continue;//封鎖セルであればスキップ.						
					}
					//今見ている距離よりも小さいセル(負の数にはならない)が隣接していればそちらへ.
					if(mapData.mapCellData[x+i, z].block == false && mapData.mapCellData[x+i, z].distance < nowDist && mapData.mapCellData[x+i, z].distance > -1){
						nextX = x+i;
						nextZ = z;
						//Debug.Log ("X方向通過:map_data("+(x+i)+"/"+z+") = ["+map_data[x+i, z].block+"](X):false=通過可能,true=通過不能",gameObject);.
						break;
					}
					else{
						//Debug.Log ("map_data("+(x+i)+"/"+z+") = ["+map_data[x+i, z].block+"](X):false=通過可能,true=通過不能",gameObject);.
					}
				}
				//Z方向ずらしチェック(該当セルの上下).
				for(int j = -1; j <= 1; j+=2){
					if(z+j < 0 || z+j > mapData.cellSpritLimit-1) {
						//Debug.Log ("Z方向範囲外");.
						continue;//範囲外ならスキップ.
					}
					if(mapData.mapCellData[x, z+j].block == true){
						//Debug.Log ("Z方向封鎖セル:map_data("+(x)+"/"+(z+j)+") = ["+map_data[x, z+j].block+"](Z):false=通過可能,true=通過不能");.
						continue;//封鎖セルであればスキップ.
					}
					//今見ている距離よりも小さいセル(負の数にはならない)が隣接していればそちらへ.
					if(mapData.mapCellData[x, z+j].block == false && mapData.mapCellData[x, z+j].distance < nowDist && mapData.mapCellData[x, z+j].distance > -1){
						nextX = x;
						nextZ = z+j;
						//Debug.Log ("Z方向通過:map_data("+(x)+"/"+(z+j)+") = ["+map_data[x, z+j].block+"](Z):false=通過可能,true=通過不能",gameObject);.
						;break;
					}
					else{
						//Debug.Log ("map_data("+(x)+"/"+(z+j)+") = ["+map_data[x, z+j].block+"](Z):false=通過可能,true=通過不能",gameObject);.
					}
				}
				//距離が小さい隣接セルが見つかっていれば.
				if(nextX != -1 && nextZ != -1){
					//移動先のセルの座標指定など.
					Vector3 targetPos= new Vector3((float)((mapData.mapCellData[nextX,nextZ].xMax+mapData.mapCellData[nextX,nextZ].xMin)/2),
						transform.position.y, (float)((mapData.mapCellData[nextX,nextZ].zMax+mapData.mapCellData[nextX,nextZ].zMin)/2));//y軸の座標をボスと同じにすることで謎のもぐりこみを回避.
					Vector3 targetRotate = new Vector3(targetPos.x - transform.position.x,targetPos.y - transform.position.y,targetPos.z - transform.position.z);
					//--物理に影響を受ける移動の停止--.
					GetComponent<Rigidbody>().velocity=Vector3.zero;//速度ベクトル0.
					GetComponent<Rigidbody>().angularVelocity=Vector3.zero;//角速度ベクトル0.
					//-----------------------------
					//回転処理、移動処理--------.
					transform.rotation = /*Quaternion.LookRotation((targetRotate).normalized);*/Quaternion.Slerp(transform.rotation, Quaternion.LookRotation((targetRotate).normalized), Time.deltaTime *argRotateSpeed);
					transform.Translate(Vector3.forward * argMoveSpeed /** Random.Range(1.0f,2.0f)*/ * Time.deltaTime,Space.Self);
					transform.position = new Vector3 (Mathf.Clamp(transform.position.x, -mapData.mapWidth/2, mapData.mapWidth/2), Mathf.Clamp(transform.position.y, 0.0f, transform.position.y), Mathf.Clamp(transform.position.z, -mapData.mapDepth/2, mapData.mapDepth/2));
				}
			}
		}
	}
	public Vector3 RandomTargetPosSelect(GameObject argTarget){
		pathSearchElapsedTime += Time.deltaTime;//1フレームごとに前フレームからの経過時間を加算.
		if(pathSearchElapsedTime >= pathSearchTargetUpdateTime){//目的地の更新時間を開始(更新)してからの経過時間が過ぎていたら.
			int clacSeed =(int)(transform.position.x+transform.position.y+transform.position.z)*(int)Random.Range(1.0f,10.0f);//現在位置を計算用の種に ※計算用に丸める,精度は重視しない.
			pathSearchElapsedTime = 0;//経過時間を初期化.
			pathSearchTargetPos = pathSearchTargetPositions[(int)(clacSeed % (pathSearchTargetPositions.Length - 1))];//clacSeedを配列の要素数で割った余りの番号に格納されている位置を.
			/*CS0472,0162回避のためコメントアウト.
			if(pathSearchTargetPositions[clacSeed % pathSearchTargetPositions.Length] == null){//もし,nullが返ってきた場合は.
				pathSearchTargetPos = argTarget.transform.position;//引数に渡されたTargetオブジェクトの現在位置を.
			}
			else{//そうでなければ(正常であれば).
				pathSearchTargetPos = pathSearchTargetPositions[(int)(clacSeed % (pathSearchTargetPositions.Length - 1))];//clacSeedを配列の要素数で割った余りの番号に格納されている位置を.
			}
			*/
		}
		return pathSearchTargetPos;
	}
}