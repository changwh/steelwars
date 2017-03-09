using UnityEngine;
using System.Collections;

public class NoLockView_Camera : MonoBehaviour 
{
	//�۲�Ŀ��
	public Transform Target;
	//�۲����
	//public float Distance = 5F;
	//��ת�ٶ�
	private float SpeedX=240;
	private float SpeedY=120;
	//�Ƕ�����
	public float  MinLimitY = 5;
	public float  MaxLimitY = 180;

	//��ת�Ƕ�
	private float mX = 0.0F;
	private float mY = 0.0F;

    //������ž�����ֵ
	private float MaxDistance=10;
	private float MinDistance=1.5F;
	//�����������
	//private float ZoomSpeed=2F;

	//�Ƿ����ò�ֵ
	public bool isNeedDamping=true;
	//�ٶ�
	public float Damping=2.5F;

	public GameObject rayFrom;
	public GameObject rayTo;

	float distance;
    public GameObject spawn;

	void Start () 
	{
		//��ʼ����ת�Ƕ�
		mX=transform.eulerAngles.x;
		mY=transform.eulerAngles.y;
	}
	void LateUpdate () 
	{
		distance=5.0f;
		Debug.DrawLine(rayFrom.transform.position,rayTo.transform.position,Color.red);//??
		RaycastHit hit;
		if(Physics.Linecast(rayFrom.transform.position,rayTo.transform.position,out hit))
		{
			string name =  hit.collider.gameObject.tag;
			if(name !="terrain"&&name!="Player")
			{
				
				rayFrom.transform.position = hit.point;
				distance = Vector3.Distance (hit.point,rayTo.transform.position);
				/*print(hit.point.x);     //??
				print(hit.point.y);
				print(hit.point.z);*/
			}
	
		}

		//����Ҽ���ת
		if(Target!=null /*&& Input.GetMouseButton(1)*/)
		{
		    //��ȡ�������
			mX += Input.GetAxis("Mouse X") * SpeedX * 0.02F;
			mY -= Input.GetAxis("Mouse Y") * SpeedY * 0.02F;
			//��Χ����
			mY = ClampAngle(mY,MinLimitY,MaxLimitY);
		}

		//����������

		/*Distance-=Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
		Distance=Mathf.Clamp(Distance,MinDistance,MaxDistance);*/
		if (Input.GetAxis("Mouse ScrollWheel") <0)
		{
			if(Camera.main.fieldOfView<=100)
				Camera.main.fieldOfView +=2;
			if(Camera.main.orthographicSize<=20)
				Camera.main.orthographicSize +=0.5F;
		}
		//Zoom in
		if (Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			if(Camera.main.fieldOfView>30)//????????
				Camera.main.fieldOfView-=2;
			/*if(Camera.main.orthographicSize>=1)
				Camera.main.orthographicSize-=0.5F;*/
			
		}
		//���¼���λ�úͽǶ�
//-----
		Quaternion mRotation = Quaternion.Euler(mY, mX, 0);
		Vector3 mPosition = mRotation * new Vector3(0.0F, 0.0F, -distance) + Target.position;


        Quaternion mmRotation = Quaternion.Euler(mY, spawn.transform.rotation.eulerAngles.y, spawn.transform.rotation.eulerAngles.z);//*spawn.transform.rotation;
        spawn.transform.rotation = mmRotation;
       
        
        //��������ĽǶȺ�λ��
        if (isNeedDamping){
		   //���β�ֵ
		   transform.rotation = Quaternion.Lerp(transform.rotation,mRotation, Time.deltaTime*Damping); 
		   //���Բ�ֵ
		   transform.position = Vector3.Lerp(transform.position,mPosition, Time.deltaTime*Damping); 
		}else{
		   transform.rotation = mRotation;
		   transform.position = mPosition;
		}
        //�����ת���������Ӧ��λ����
        /*if (Target.GetComponent<NoLockiVew_Player>().State == NoLockiVew_Player.PlayerState.Walk)
        {
            Target.eulerAngles = new Vector3(0, mX, 0);
        }*/

    }
	
	private float  ClampAngle (float angle,float min,float max) 
	{
		if (angle < -360) angle += 360;
		if (angle >  360) angle -= 360;
		return Mathf.Clamp (angle, min, max);
	}
    



}
