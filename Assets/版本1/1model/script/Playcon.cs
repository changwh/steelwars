using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Playcon : MonoBehaviour {
    #region
    public float animSpeed = 1f;              

    public float forwardSpeed = 4.0f;
    public float backwardSpeed = 2.0f;
    public float rotateSpeed = 2.0f;
    public float jumpPower = 3.0f;
    public GameObject prefabA;
    public GameObject bullspot;

    //public GameObject particle;
    private CapsuleCollider col;
    private Rigidbody rb;

    private Vector3 velocity;
    private float orgColHight;
    private Vector3 orgVectColCenter;

    private Animator anim;                         
    private AnimatorStateInfo currentBaseState;           
    private AnimatorStateInfo previousState;
    private GameObject cameraObject;    

   
    static int idleState = Animator.StringToHash("Base Layer.idle");
    static int crouchState = Animator.StringToHash("Base Layer.crouch");
    static int rollState = Animator.StringToHash("Base Layer.roll");

    static int attack1State = Animator.StringToHash("Base Layer.attack1.1");
    static int attack2State = Animator.StringToHash("Base Layer.attack1.2");
    static int attack3State = Animator.StringToHash("Base Layer.attack1.3");

    public static int attack11State = Animator.StringToHash("Base Layer.attack2.1");
	public static int attack22State = Animator.StringToHash("Base Layer.attack2.2");
	public static int attack33State = Animator.StringToHash("Base Layer.attack2.3");

    private float h, v;
    private float baseCapsuleHeight;
    private bool state;
    private float currenttime, lasttime;
    private float currenttime1, lasttime1;
    #endregion
    void Start () {
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
     

        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
        previousState = currentBaseState;
        baseCapsuleHeight = col.height;
}

    void FixedUpdate()
    {
      
        //初始化移动速度方向速度
        #region
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        
       
        anim.speed = animSpeed;
        previousState = currentBaseState;
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);

        velocity = new Vector3(0, 0, v);
        velocity = transform.TransformDirection(velocity);


        if (v > 0.1)
        {
            velocity *= forwardSpeed;       
        }
        else if (v < -0.1)
        {
            velocity *= backwardSpeed; 
        }
        if(Input.GetKey(KeyCode.LeftShift))
        {
            velocity *= 2;
            v *= 2;
        }
        #endregion
        //蹲下
        #region
        if (Input.GetKey(KeyCode.Q))
        {
            if (v > 0.1)
            {
                velocity *= backwardSpeed;
            }
            else if (v < -0.1)
            {
                velocity *=0;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                velocity /= 2;
                v /= 2;
            }
            anim.SetBool("crouch", true);
        }
        else
        {
            anim.SetBool("crouch", false);
        }
        #endregion
        //移动和旋转
        #region
        if (currentBaseState.fullPathHash == idleState || currentBaseState.fullPathHash == crouchState)
            {

                transform.localPosition += velocity * Time.fixedDeltaTime;
                transform.Rotate(0, h * rotateSpeed, 0);
                anim.SetFloat("rotate", h);
                anim.SetFloat("speed", v);
                state = true;
            }


        #endregion
        //滚动
        #region
        if (currentBaseState.fullPathHash == rollState && !state)
        {
            velocity = new Vector3(0, 0, 1);
            velocity = transform.TransformDirection(velocity) *2;
            transform.localPosition += velocity * Time.fixedDeltaTime;
            print(transform.localPosition);
        }
        state = false;
        #endregion
        //三种攻击方式
        #region
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (anim.GetBool("sword") == false && anim.GetBool("gunon") == false)
            {
                Attack1();
            }
        }

		if (/*Input.GetKeyDown(KeyCode.Alpha2)*/Input.GetButton ("Fire1"))
        {
            if (anim.GetBool("sword"))
            {
                Attack2();
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
           if(anim.GetBool("gunon"))
            {
                anim.SetTrigger("gunshot");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (anim.GetBool("gunon"))
            {
                if(anim.GetBool("ready"))
                {
                    IKtrans ik = gameObject.GetComponent<IKtrans>();
                    ik.ikActive = false;
                    anim.SetBool("ready", false);
                }
                else
                {
                    IKtrans ik = gameObject.GetComponent<IKtrans>();
                    ik.ikActive = true;
                    anim.SetBool("ready", true);
                }
            }
        }
        #endregion
        //跳跃
        #region
        if (Input.GetKeyDown(KeyCode.Space))
        {   
            if (currentBaseState.fullPathHash == idleState)
            {
               
                if (!anim.IsInTransition(0))
                {
                    rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
                    anim.SetTrigger("jump");     
                }
            }
        }
        #endregion
        //拔剑
        #region
        if (Input.GetKeyDown(KeyCode.R))
        {
            if(anim.GetBool("gunon")==false)
            SwordOnHand();
        }
        #endregion
        //拿枪
        #region
        if(Input.GetKeyDown(KeyCode.E) && anim.GetBool("sword")==false)
        {
            if (currentBaseState.fullPathHash == idleState)
            {
                if (!anim.IsInTransition(0))
                {
                    anim.SetTrigger("gun");
                }
            }
        }
        #endregion
    }
    void Update () {
		//
        if (currentBaseState.IsName("Base Layer.crouch") || currentBaseState.IsName("Base Layer.roll"))
        {
            col.height = 1.2f;
            col.center = new Vector3(0, 0.6f, 0);
        }
        else
        {
            col.height = baseCapsuleHeight;
            col.center = new Vector3(0, 0.87f, 0);
        }

		//
		if (Input.GetKeyDown(KeyCode.W))
		{
			
			if (currentBaseState.fullPathHash == idleState || currentBaseState.fullPathHash == crouchState)
			{
				
				if (!anim.IsInTransition(0))
				{
					
					currenttime = Time.realtimeSinceStartup;
					if (currenttime - lasttime < 0.2f)
					{
						anim.SetTrigger("crouchgo");	
					}
					lasttime = Time.realtimeSinceStartup;
					
				}
			}
		}
    }

    void Attack1()
    {
       if( currentBaseState.fullPathHash == idleState)
        {
            if (!anim.IsInTransition(0))
            {
                anim.SetTrigger("attack1");
                anim.SetInteger("attack",1);
            }
        }
       else if(currentBaseState.fullPathHash==attack1State && currentBaseState.normalizedTime>0.5)
        {
            anim.SetInteger("attack", 2);
        }
        else if (currentBaseState.fullPathHash == attack2State && currentBaseState.normalizedTime > 0.5)
        {
            anim.SetInteger("attack", 3);
        }
       else if (currentBaseState.fullPathHash==attack3State)
        {
            print("PL**************************************PL");
        }

    }
    void Attack2()
    {
        if (currentBaseState.fullPathHash == idleState)
        {
            if (!anim.IsInTransition(0))
            {
                anim.SetTrigger("attack2");
                anim.SetInteger("attack", 1);
            }
        }
        else if (currentBaseState.fullPathHash == attack11State && currentBaseState.normalizedTime > 0.5)
        {
            anim.SetInteger("attack", 2);
        }
        else if (currentBaseState.fullPathHash == attack22State && currentBaseState.normalizedTime > 0.5)
        {
            anim.SetInteger("attack", 3);
        }
        else if (currentBaseState.fullPathHash == attack33State)
        {
            print("PL**************************************PL");
        }

    }

    //剑到手
    void SwordOnHand()
    {
        if(anim.GetBool("sword"))
        {
            Transform obj= transform.FindChild("hip")
                                    .FindChild("spine")
                                    .FindChild("chest")
                                    .FindChild("R_shoulder")
                                    .FindChild("R_arm")
                                    .FindChild("R_elbow")
                                    .FindChild("R_wrist")
                                    .FindChild("sword")
                                    .FindChild("SWO002");
            obj.parent = transform.FindChild("swordback");
            Vector3 v = new Vector3(0,0, 0);

            obj.transform.localPosition = v;
            obj.transform.localRotation = Quaternion.Euler(v);
            anim.SetBool("sword", false);

        }
        else
        {

            Transform obj = transform.FindChild("swordback")
                                     .FindChild("SWO002");
            obj.parent = transform.FindChild("hip")
                                    .FindChild("spine")
                                    .FindChild("chest")
                                    .FindChild("R_shoulder")
                                    .FindChild("R_arm")
                                    .FindChild("R_elbow")
                                    .FindChild("R_wrist")
                                    .FindChild("sword");
            Vector3 v = new Vector3(0.013f, 0.191f, -0.035f);
        
            obj.transform.localPosition=v;
            v = new Vector3(356.586f, 276.3318f, 187.6685f);
            obj.transform.localRotation = Quaternion.Euler(v);
            anim.SetBool("sword", true);
        }
        
        
       
    }
    //枪到手
    void GunOnHand()
    {
        if(anim.GetBool("gunon"))
        {
            Transform obj = transform.FindChild("hip")
                                .FindChild("spine")
                                .FindChild("chest")
                                .FindChild("R_shoulder")
                                .FindChild("R_arm")
                                .FindChild("R_elbow")
                                .FindChild("R_wrist")
                                .FindChild("gun")
                                .FindChild("pistol");
            obj.parent = transform.FindChild("hip")
                                 .FindChild("R_leg")
                                 .FindChild("gunoff");
            Vector3 v = new Vector3(0, 0, 0);
            obj.localPosition=v;
            obj.localRotation = Quaternion.Euler(v);
			anim.SetBool("ready",false);
            anim.SetBool("gunon", false);
            if (anim.layerCount >= 2)
            {
                anim.SetLayerWeight(0, 1);
                anim.SetLayerWeight(1, 0);
            }

        }
        else
        {
        Transform obj = transform.FindChild("hip")
                                 .FindChild("R_leg")
                                 .FindChild("gunoff")
                                 .FindChild("pistol");
            obj.parent = transform.FindChild("hip")
                                .FindChild("spine")
                                .FindChild("chest")
                                .FindChild("R_shoulder")
                                .FindChild("R_arm")
                                .FindChild("R_elbow")
                                .FindChild("R_wrist")
                                .FindChild("gun");

            Vector3 v = new Vector3(0, 0, 0);
            obj.localPosition = v;
            obj.localRotation = Quaternion.Euler(v);
            anim.SetBool("gunon", true);
            if (anim.layerCount >= 2)
            {
                anim.SetLayerWeight(1, 1);
            }
        }




    }

    //枪击
    #region
    void GunShot()
    {
           GameObject bull= (GameObject)Instantiate(prefabA,bullspot.transform.position,bullspot.transform.rotation);
           Rigidbody rb = bull.GetComponent<Rigidbody>();
            if (rb != null)
            {
                
                rb.velocity = bullspot.transform.TransformDirection(Vector3.forward * 20);
            }
    }
    void GunPosStart()
    {
        Transform obj = transform.FindChild("hip")
                                .FindChild("spine")
                                .FindChild("chest")
                                .FindChild("R_shoulder")
                                .FindChild("R_arm")
                                .FindChild("R_elbow")
                                .FindChild("R_wrist")
                                .FindChild("gun")
                                .FindChild("pistol");
        Vector3 v = new Vector3(0.0008f, -0.0011f, -0.0197f);
        obj.localPosition = v;
        v = new Vector3(9.513455f, 2.816742f, 23.83434f);
        obj.localRotation=Quaternion.Euler(v);
    }
    void GunPosEnd()
    {
        Transform obj = transform.FindChild("hip")
                               .FindChild("spine")
                               .FindChild("chest")
                               .FindChild("R_shoulder")
                               .FindChild("R_arm")
                               .FindChild("R_elbow")
                               .FindChild("R_wrist")
                               .FindChild("gun")
                               .FindChild("pistol");
       
        obj.localPosition = Vector3.zero;
       
        obj.localRotation = Quaternion.Euler(Vector3.zero);
    }
    #endregion
   
    }
