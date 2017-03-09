using UnityEngine;
using System.Collections;

public class playercontroll : MonoBehaviour {
    private Animator ani;
    public float rotateSpeed=15;
    public float speed=2;
    private Vector3 velocity;
    // Use this for initialization
    void Start () {
	
	}
	void Awake()
    {
        ani = this.GetComponent<Animator>();
    }
	// Update is called once per frame
	void Update () {
        float vertical = Input.GetAxis("Vertical");
        float h= Input.GetAxis("Horizontal");
        velocity = new Vector3(0, 0, vertical);
        velocity = transform.TransformDirection(velocity);//chao xiang
        print(vertical);
        ani.SetFloat("speed", vertical);
        if (vertical > 0.1 && ani.GetBool("isrun") ==false)
        {
            velocity *= speed;
            transform.localPosition += velocity * Time.fixedDeltaTime;
            print(transform.localPosition);
        }

        transform.Rotate(0, h * rotateSpeed, 0);
      
        if(Input.GetKey(KeyCode.Space))
        {
            ani.SetBool("isattack", true);
        }
        else
        {
            ani.SetBool("isattack", false);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            velocity *= speed*2;
            ani.SetBool("isrun", true);
            transform.localPosition += velocity * Time.fixedDeltaTime;
        }
        else
        {
            ani.SetBool("isrun", false);
        }
    }
}
