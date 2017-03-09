using UnityEngine;
using System.Collections;

public class swordAttack : MonoBehaviour {
	public Animator ani;
	void OnTriggerEnter(Collider other)
	{
		int attackState = ani.GetCurrentAnimatorStateInfo (0).fullPathHash;
		if (attackState == Playcon.attack11State) {
			if (other.tag != "Player") {
				if (other.tag == "AI") {
					other.GetComponentInParent<EnemyDestory> ().slider.value = other.GetComponentInParent<EnemyDestory> ().hp;
					other.GetComponentInParent<EnemyDestory> ().gameobject.SetActive (true);
					other.GetComponentInParent<EnemyDestory> ().hp = other.GetComponentInParent<EnemyDestory> ().hp - 8;
					print ("hp"+other.GetComponentInParent<EnemyDestory> ().hp);
				}
			}
		}
		else if (attackState == Playcon.attack22State) {
			if (other.tag != "Player") {
				if (other.tag == "AI") {
					other.GetComponentInParent<EnemyDestory> ().slider.value = other.GetComponentInParent<EnemyDestory> ().hp;
					other.GetComponentInParent<EnemyDestory> ().gameobject.SetActive (true);
					other.GetComponentInParent<EnemyDestory> ().hp = other.GetComponentInParent<EnemyDestory> ().hp - 10;
					print ("hp"+other.GetComponentInParent<EnemyDestory> ().hp);
				}
			}
		}
		else if (attackState == Playcon.attack33State) {
			if (other.tag != "Player") {
				if (other.tag == "AI") {
					other.GetComponentInParent<EnemyDestory> ().slider.value = other.GetComponentInParent<EnemyDestory> ().hp;
					other.GetComponentInParent<EnemyDestory> ().gameobject.SetActive (true);
					other.GetComponentInParent<EnemyDestory> ().hp = other.GetComponentInParent<EnemyDestory> ().hp - 12;
					print ("hp"+other.GetComponentInParent<EnemyDestory> ().hp);
				}
			}
		}

	}

}
