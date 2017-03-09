using UnityEngine;
using System.Collections;

public class scrollScreen : MonoBehaviour {
	public UnityEngine.UI.Text text;
	int rollspead=3;
	Vector3 temp;
	// Use this for initialization
	void Start () {
		temp = text.rectTransform.transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if (text.rectTransform.transform.position.y < 1500) {
			temp.y=temp.y+rollspead;
		}
		text.rectTransform.transform.position = temp;
		if (text.rectTransform.transform.position.y == 2000) {
			Destroy(text);
		}
		if(Input.anyKeyDown){
			rollspead=100;
		}
	}
}