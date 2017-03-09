using UnityEngine;
using System.Collections;

public class PlayC : MonoBehaviour {
    private Animator ani;
    public float rotateSpeed = 15;
    public float speed = 2;
    private Vector3 velocity;
    // Use this for initialization
    void Start () {
        ani = this.GetComponent<Animator>();
    }
	
	// Update is called once per frame
	void Update () {

        float vertical = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        transform.Rotate(0, h * rotateSpeed, 0);
        velocity = new Vector3(0, 0, vertical);
        velocity = transform.TransformDirection(velocity);//chao xiang

        ani.SetFloat("speed", vertical);
        if (vertical > 0.1 && ani.GetBool("isrun") == false)
        {
            velocity *= speed;
            transform.localPosition+= velocity * Time.deltaTime;
           
        }


        if (Input.GetKey(KeyCode.Space))
        {
            ani.SetBool("isattack", true);
        }
        else
        {
            ani.SetBool("isattack", false);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity *= speed * 2;
            ani.SetBool("isrun", true);
            transform.localPosition += velocity * Time.deltaTime;
        }
        else
        {
            ani.SetBool("isrun", false);
        }
    
}
}
