using UnityEngine;
using System.Collections;

public class EnemyDestory : MonoBehaviour {
	public int maxhp;
	public UnityEngine.UI.Slider slider;
	public GameObject gameobject;

	public int hp;

	void Start(){
		slider.enabled = false;
		slider.maxValue = maxhp;
		hp = maxhp;
		slider.value = slider.maxValue;
		gameobject.SetActive (false);
	}
	/*void OnTriggerEnter(Collider other){

		if (other.tag == "bolt") {
			slider.value=hp;
			gameobject.SetActive (true);
			Destroy (other.gameObject);
			hp=hp-10;
		}

	}*/
	void Update(){
		/*if (slider.value <= hp) {
			slider.value=hp;
		}*/
		if (slider.value > hp) {
			slider.value--;
		}
		if (hp <= 0&&slider.value<= 0) {
			Destroy (gameObject);
			gameobject.SetActive (false);
		}
	}
}
