using UnityEngine;
using System.Collections;

public class ThirdPerson2FirstPerson : MonoBehaviour {
	public GameObject firstPerson;
	public GameObject thirdPerson;
	public GameObject camera;
	public int blood;
	bool sword,gun,gunshot;
	Animator ani;
	void LateUpdate () {
		if (Input.GetMouseButtonDown (1) && thirdPerson.activeSelf == false) {

			camera.active=true;
			firstPerson.SetActive(false);
			thirdPerson.transform.position=firstPerson.transform.position;
			thirdPerson.transform.rotation=firstPerson.transform.rotation;
			thirdPerson.SetActive (true);
			ani=ani=thirdPerson.GetComponent<Animator>();
			ani.SetBool("sword",sword);
			ani.SetBool("gunon",gun);
			if(gun)
			{
				if (ani.layerCount >= 2)
				{
					ani.SetLayerWeight(1, 1);
				}
			}
			//ani.SetBool("ready",gunshot);
		} 
		else if (Input.GetMouseButtonDown (1) && thirdPerson.activeSelf == true) {
			ani=thirdPerson.GetComponent<Animator>();
			sword=ani.GetBool("sword");
			gun=ani.GetBool("gunon");
			//gunshot=ani.GetBool("ready");
			camera.active=false;
			firstPerson.SetActive(true);
			firstPerson.transform.position=thirdPerson.transform.position;
			firstPerson.transform.rotation=thirdPerson.transform.rotation;
			thirdPerson.SetActive (false);
		}
	}
	void Update(){
		if (blood <= 0) {
			Destroy (firstPerson);
			Destroy (thirdPerson);
		}
	}
}