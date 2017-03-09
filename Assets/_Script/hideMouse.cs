using UnityEngine;
using System.Collections;

public class hideMouse : MonoBehaviour {
	public GameObject Inventory;
	public GameObject EquipmentSystem;
	// Use this for initialization
	void Start () {
		Cursor.visible = false; 
	}
	
	// Update is called once per frame
	void Update () {
		if(Inventory.activeSelf||EquipmentSystem.activeSelf){
			Cursor.visible=true;
		}
		else{
			Cursor.visible=false;
		}
	}
}
