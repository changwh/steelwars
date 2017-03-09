using UnityEngine;
using System.Collections;


public class LightUp : MonoBehaviour {
	public UnityEngine.UI.Image shade;
	public float spead;
	bool isPause=false;
	Color c;
	// Use this for initialization
	void Start () {
		c = shade.color;
	}
	
	// Update is called once per frame
	void Update () {
		Time.timeScale = 0;
		if (c.a >0) {
			c.a=c.a-(0.01f/spead);
			shade.color=c;
		}
		if (c.a <0.1) {
			Destroy(shade);
			Time.timeScale=1;
		}
		if(Input.anyKeyDown){
			spead=1;
		}
	}
}
