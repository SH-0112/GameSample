using UnityEngine;
using System.Collections;

[System.Serializable]
public class WeaponObject{
	public string weaponGroupName;
	public GameObject[] weapon;
}

[System.Serializable]
//EnemyWeapnTriggerMode.csなどで作成した兵装オブジェクトの火器管制用クラス.
public class FireControlSystem : MonoBehaviour {
	
	public WeaponObject[] weaponObjectGroup;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	//指定兵装グループ起動.
	public void WeaponsActivation(int argWeaponObjectGroupNum){
		if(weaponObjectGroup != null){//兵装グループがNullで無ければ.
			foreach(GameObject weapons in weaponObjectGroup[argWeaponObjectGroupNum].weapon){//指定番号の兵装グループを探索し.
				weapons.SetActive(true);//起動(アクティブ状態に)する.
			}
		}
	}
	//全登録兵装起動.
	public void AllWeaponsActivation(){
		if(weaponObjectGroup != null){//兵装グループがNullで無ければ.
			foreach(WeaponObject weaponObgectsGroups in weaponObjectGroup){//全兵装グループを探索.
				foreach(GameObject weapons in weaponObgectsGroups.weapon){//兵装グループ毎のすべての兵装を.
					weapons.SetActive(true);//起動(アクティブ状態に)する.
				}
			}
		}
	}
	//指定兵装グループ停止.
	public void WeaponsDeactivation(int argWeaponObjectGroupNum){
		if(weaponObjectGroup != null){//兵装グループがNullで無ければ.
			foreach(GameObject weapons in weaponObjectGroup[argWeaponObjectGroupNum].weapon){//指定番号の兵装グループを探索し.
				weapons.SetActive(false);//停止(非アクティブ状態に)する.
			}
		}
	}
	//全登録兵装停止.
	public void AllWeaponsDeactivation(){
		if(weaponObjectGroup != null){//兵装グループがNullで無ければ.
			foreach(WeaponObject weaponObgectsGroups in weaponObjectGroup){//全兵装グループを探索.
				foreach(GameObject weapons in weaponObgectsGroups.weapon){//兵装グループ毎のすべての兵装を.
					weapons.SetActive(false);//停止(非アクティブ状態に)する.
				}
			}
		}
	}
}
