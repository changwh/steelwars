using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public int blood=100;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	if(blood<=0)
        {
            
            Destroy(gameObject);
        }
	}
}
