using UnityEngine;
using System.Collections;
using System;

public class SceneController1 : MonoBehaviour {
	public UnityEngine.UI.Text text1;
	public UnityEngine.UI.Text text2;
	public UnityEngine.UI.Text text3;
	public UnityEngine.UI.Text text4;
	public UnityEngine.UI.Text text5;
	public GameObject panel1;
	public GameObject panel2;
	public GameObject panel3;
	public GameObject panel4;
	public GameObject panel5;

	Color c1;
	Color c2;
	Color c3;
	Color c4;
	Color c5;

	void Start(){
		Cursor.visible = false; 
		c1 = text1.color;
		c2 = text1.color;
		c3 = text1.color;
		c4 = text1.color;
		c5 = text1.color;
		c1.a = 0;
		c2.a = 0;
		c3.a = 0;
		c4.a = 0;
		c5.a = 0;
	}
	void LateUpdate(){
		StartCoroutine(ShowUp (text1,c1,1));
		TextHide (text1, panel1,c1);
		//StartCoroutine(Hide (text1,panel1,4));
		StartCoroutine(ShowUp (text2,c2,4));
		TextHide (text2, panel2,c2);
		//StartCoroutine(Hide (text2,panel2,8));
		StartCoroutine(ShowUp (text3,c3,7));
		TextHide (text3, panel3,c3);
		//StartCoroutine(Hide (text3,panel3,12));
		StartCoroutine(ShowUp (text4,c4,10));
		TextHide (text4, panel4,c4);
		//StartCoroutine(Hide (text4,panel4,16));
		StartCoroutine(ShowUp (text5,c5,13));
		TextHide (text5, panel5,c5);
		//StartCoroutine(Hide (text5,panel5,20));
		StartCoroutine(NextLevel (16));

	}
	void Update(){
		if(Input.anyKeyDown){
			Application.LoadLevel(1);
			Cursor.visible = true;
		}
	}
	void TextShowUp(UnityEngine.UI.Text text,Color c){
		if (text.color.a < 1) {
			StartCoroutine(WaitAndPlus(text,c));  
		}
	}
	void TextHide(UnityEngine.UI.Text text,GameObject panel,Color c){
		if (text.color.a >=0.9) {
			StartCoroutine(WaitAndDestroy(panel,c));
		}
	}
	IEnumerator WaitAndPlus(UnityEngine.UI.Text text,Color c){
		yield return new WaitForSeconds(1);
		while (c.a < 1) {
			c.a = c.a + 0.01f;
		}
		if (text != null) {
			text.color = c;
		}
	}
	IEnumerator WaitAndDestroy(GameObject panel,Color c){
		yield return new WaitForSeconds(2);
		Destroy (panel);
		if (panel == null) {
			c.a=0;
		}
	}
	IEnumerator ShowUp(UnityEngine.UI.Text text,Color c,float delaySeconds) {
		yield return new WaitForSeconds(delaySeconds);
		TextShowUp (text,c);
	}
	IEnumerator NextLevel(float delaySeconds) {
		yield return new WaitForSeconds(delaySeconds);
		Application.LoadLevel(1);
		Cursor.visible = true;
	}
}
