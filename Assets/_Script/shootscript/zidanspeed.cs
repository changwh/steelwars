using UnityEngine;
using System.Collections;

public class zidanspeed : MonoBehaviour {

    public float speed;
	public GameObject explosionEffect;
    private Rigidbody rg;
    // Use this for initialization
    void Start()
    {
        rg = GetComponent<Rigidbody>();
        rg.velocity = transform.forward * speed;
    }

    // Update is called once per frame
    void Update()
    {
		Destroy(this.gameObject, 10);
    }

 
    void OnTriggerEnter(Collider other)
    {
        
        //if (other.tag == "boundary")
        //{ return; }
        //Instantiate(explosion, transform.position, transform.rotation);
        //if(other.tag=="Player")
        //{
        //    Instantiate(playerExplosion, other.transform.position, other.transform.rotation);
        //    gc.GameOver();
        //    Debug.Log("guo");
        //}

        //Destroy(other.gameObject);
		if (other.tag != "Player") {
			Destroy (gameObject);
			if (other.tag == "AI") {
				Instantiate (explosionEffect, transform.position, transform.rotation);
				other.GetComponentInParent<EnemyDestory> ().slider.value = other.GetComponentInParent<EnemyDestory> ().hp;
				other.GetComponentInParent<EnemyDestory> ().gameobject.SetActive (true);
				other.GetComponentInParent<EnemyDestory> ().hp = other.GetComponentInParent<EnemyDestory> ().hp - 10;
			}
		}
    }
}


