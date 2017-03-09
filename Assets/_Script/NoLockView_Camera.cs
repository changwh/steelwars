using UnityEngine;
using System.Collections;

public class NoLockView_Camera : MonoBehaviour 
{
	//观察目标
	public Transform Target;
	//观察距离
	//public float Distance = 5F;
	//旋转速度
	private float SpeedX=240;
	private float SpeedY=120;
	//角度限制
	public float  MinLimitY = 5;
	public float  MaxLimitY = 180;

	//旋转角度
	private float mX = 0.0F;
	private float mY = 0.0F;

    //鼠标缩放距离最值
	private float MaxDistance=10;
	private float MinDistance=1.5F;
	//鼠标缩放速率
	//private float ZoomSpeed=2F;

	//是否启用差值
	public bool isNeedDamping=true;
	//速度
	public float Damping=2.5F;

	public GameObject rayFrom;
	public GameObject rayTo;

	float distance;
    public GameObject spawn;

	void Start () 
	{
		//初始化旋转角度
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

		//鼠标右键旋转
		if(Target!=null /*&& Input.GetMouseButton(1)*/)
		{
		    //获取鼠标输入
			mX += Input.GetAxis("Mouse X") * SpeedX * 0.02F;
			mY -= Input.GetAxis("Mouse Y") * SpeedY * 0.02F;
			//范围限制
			mY = ClampAngle(mY,MinLimitY,MaxLimitY);
		}

		//鼠标滚轮缩放

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
		//重新计算位置和角度
//-----
		Quaternion mRotation = Quaternion.Euler(mY, mX, 0);
		Vector3 mPosition = mRotation * new Vector3(0.0F, 0.0F, -distance) + Target.position;


        Quaternion mmRotation = Quaternion.Euler(mY, spawn.transform.rotation.eulerAngles.y, spawn.transform.rotation.eulerAngles.z);//*spawn.transform.rotation;
        spawn.transform.rotation = mmRotation;
       
        
        //设置相机的角度和位置
        if (isNeedDamping){
		   //球形插值
		   transform.rotation = Quaternion.Lerp(transform.rotation,mRotation, Time.deltaTime*Damping); 
		   //线性插值
		   transform.position = Vector3.Lerp(transform.position,mPosition, Time.deltaTime*Damping); 
		}else{
		   transform.rotation = mRotation;
		   transform.position = mPosition;
		}
        //将玩家转到和相机对应的位置上
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
