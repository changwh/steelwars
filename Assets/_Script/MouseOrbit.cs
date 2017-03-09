using UnityEngine;
using System.Collections;


public class MouseOrbit : MonoBehaviour
{
	public Transform target;
	//public float distance = 30.0f;
	
	public float xSpeed = 250.0f;
	public float ySpeed = 120.0f;
	
	public float yMinLimit = -20;
	public float yMaxLimit = 80;
	
	private float x = 0.0f;
	private float y = 0.0f;
	
	void Start()
	{
		var angles = transform.eulerAngles;
		x = angles.y;
		y = angles.x;
		
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
	}
	
	void LateUpdate()
	{
		if (/*Input.GetMouseButton(0)*/true)                        //若不改为true，则需要按住左键进行视角旋转
		{
			if (target)
			{
				x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
				y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
				
				y = ClampAngle(y, yMinLimit, yMaxLimit);
				
				var rotation = Quaternion.Euler(y, x, 0); //锁定上下视角移动
				//var position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;
				
				transform.rotation = rotation;
				//transform.position = position;
			}

		}
		//鼠标滚轮缩放
		//Zoom out
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
			if(Camera.main.fieldOfView>30)//设置缩进的最小值
				Camera.main.fieldOfView-=2;
			/*if(Camera.main.orthographicSize>=1)
				Camera.main.orthographicSize-=0.5F;*/

		}
	}
	
	static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
		{
			angle += 360;
		}
		if (angle > 360)
		{
			angle -= 360;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
