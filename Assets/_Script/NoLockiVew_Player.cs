using UnityEngine;
using System.Collections;

public class NoLockiVew_Player : MonoBehaviour {

	/*�����ӽ��µĽ�ɫ����*/
	/*���ߣ���Ԫ��*/

	//��ҵ������ٶ�
	public float WalkSpeed=1.5F;
	//����
	public float Gravity=20;

	//��ɫ������
	private CharacterController mController;
	//�������
	private Animation mAnim;
	//��ҷ���Ĭ����ǰ
	private DirectionType mType=DirectionType.Direction_Forward;

	[HideInInspector]
	//���״̬��Ĭ��ΪIdle
	public PlayerState State=PlayerState.Idle;

	//������ҵ�״̬ö��
	public enum PlayerState
	{
		Idle,
		Walk
	}

	//�����ĸ������ö��ֵ��������ʱ�뷽�����
	protected enum DirectionType
	{
		Direction_Forward=90,
		Direction_Backward=270,
		Direction_Left=180,
		Direction_Right=0
	}
	
	void Start () 
	{
	   //��ȡ��ɫ������
	   mController=GetComponent<CharacterController>();
	   //��ȡ�������
	   mAnim=GetComponentInChildren<Animation>();
	}
	
	
	void Update () 
	{
		MoveManager();
		//MouseEvent();
	}

	//����ƶ�����
	void MoveManager()
	{
		//�ƶ�����
		Vector3 mDir=Vector3.zero;
		if(mController.isGrounded)
		{
			//����ɫ��ת����Ӧ�ķ���
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
			//��ɫ��Idle����
			if(Input.GetAxis("Vertical")==0 && Input.GetAxis("Horizontal")==0)
			{
				mAnim.CrossFade("Idle",0.25F);
				State=PlayerState.Idle;
			}

		}
		//������������
		mDir=transform.TransformDirection(mDir);
		float y=mDir.y-Gravity *Time.deltaTime;
		mDir=new Vector3(mDir.x,y,mDir.z);
		mController.Move(mDir);
	}

	//���ý�ɫ�ķ���������
	void SetDirection(DirectionType mDir)
	{
		if(mType!=mDir)
		{
			transform.Rotate(Vector3.up*(mType-mDir));
			mType=mDir;
		}
	}
}