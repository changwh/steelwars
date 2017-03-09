using UnityEngine;
using System.Collections;

public class DestroyEffect : MonoBehaviour {
	void Update()
	{
		Destroy(this.gameObject, 10);
	}
}
