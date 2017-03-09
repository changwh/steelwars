using UnityEngine;
using System.Collections;

public class playershoot : MonoBehaviour {

    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;
	public GameObject shootEffect;


    private float nextFire;

    //private AudioSource au;
    // Use this for initialization
    void Start () {
		shootEffect.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButton ("Fire1")  && Time.time > nextFire) {
			nextFire = Time.time + fireRate;
			GameObject clone = Instantiate (shot, shotSpawn.position, shotSpawn.rotation) as GameObject;
			shootEffect.SetActive(true);
			//  au.Play();
		}
		if (Input.GetMouseButtonUp (0)) {
			shootEffect.SetActive(false);
		}
    }
}
