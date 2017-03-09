using UnityEngine;
using System.Collections;

public class NoLockiVew_Player : MonoBehaviour {

	/*自由视角下的角色控制*/
	/*作者：秦元培*/

	//玩家的行走速度
	public float WalkSpeed=1.5F;
	//重力
	public float Gravity=20;

	//角色控制器
	private CharacterController mController;
	//动画组件
	private Animation mAnim;
	//玩家方向，默认向前
	private DirectionType mType=DirectionType.Direction_Forward;

	[HideInInspector]
	//玩家状态，默认为Idle
	public PlayerState State=PlayerState.Idle;

	//定义玩家的状态枚举
	public enum PlayerState
	{
		Idle,
		Walk
	}

	//定义四个方向的枚举值，按照逆时针方向计算
	protected enum DirectionType
	{
		Direction_Forward=90,
		Direction_Backward=270,
		Direction_Left=180,
		Direction_Right=0
	}
	
	void Start () 
	{
	   //获取角色控制器
	   mController=GetComponent<CharacterController>();
	   //获取动画组件
	   mAnim=GetComponentInChildren<Animation>();
	}
	
	
	void Update () 
	{
		MoveManager();
		//MouseEvent();
	}

	//玩家移动控制
	void MoveManager()
	{
		//移动方向
		Vector3 mDir=Vector3.zero;
		if(mController.isGrounded)
		{
			//将角色旋转到对应的方向
			if(Input.GetAxis("Vertical")==1)
			{
				SetDirection(DirectionType.Direction_Forward);
				mDir=Vector3.forward * Time.deltaTime * WalkSpeed;
				mAnim.CrossFade("Walk",0.25F);
				State=PlayerState.Walk;
			}
			if(Input.GetAxis("Vertical")==-1)
			{
				SetDirection(DirectionType.Direction_Backward);
				mDir=Vector3.forward * Time.deltaTime * WalkSpeed;
				mAnim.CrossFade("Walk",0.25F);
				State=PlayerState.Walk;
			}
			if(Input.GetAxis("Horizontal")==-1)
			{
				SetDirection(DirectionType.Direction_Left);
				mDir=Vector3.forward * Time.deltaTime * WalkSpeed;
				mAnim.CrossFade("Walk",0.25F);
				State=PlayerState.Walk;
			}
			if(Input.GetAxis("Horizontal")==1)
			{
				SetDirection(DirectionType.Direction_Right);
				mDir=Vector3.forward * Time.deltaTime * WalkSpeed;
				mAnim.CrossFade("Walk",0.25F);
				State=PlayerState.Walk;
			}
			//角色的Idle动画
			if(Input.GetAxis("Vertical")==0 && Input.GetAxis("Horizontal")==0)
			{
				mAnim.CrossFade("Idle",0.25F);
				State=PlayerState.Idle;
			}

		}
		//考虑重力因素
		mDir=transform.TransformDirection(mDir);
		float y=mDir.y-Gravity *Time.deltaTime;
		mDir=new Vector3(mDir.x,y,mDir.z);
		mController.Move(mDir);
	}

	//设置角色的方向，有问题
	void SetDirection(DirectionType mDir)
	{
		if(mType!=mDir)
		{
			transform.Rotate(Vector3.up*(mType-mDir));
			mType=mDir;
		}
	}
}