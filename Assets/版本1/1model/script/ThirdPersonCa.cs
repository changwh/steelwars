using UnityEngine;
using System.Collections;
using System;
[RequireComponent(typeof(Transform))]
public class ThirdPersonCa : MonoBehaviour
{
    //观察目标
    public Transform Target;
    public Transform ikobj;
    public GameObject model;
    public GameObject prefabA;
    public GameObject bullspot;
    //观察距离
    public float Distance = 5F;
    //旋转速度
    private float SpeedX = 240;
    private float SpeedY = 120;
    //角度限制
    public float MinLimitY = 5;
    public float MaxLimitY = 180;

    //旋转角度
    private float mX = 0.0F;
    private float mY = 0.0F;

    //鼠标缩放距离最值
    private float MaxDistance = 10;
    private float MinDistance = 1.5F;
    //鼠标缩放速率
    private float ZoomSpeed = 2F;



    public GameObject rayFrom;
    public GameObject rayTo;

    float distance;


    // Use this for initialization
    void Start()
    {
        //初始化旋转角度
        mX = transform.eulerAngles.x;
        mY = transform.eulerAngles.y;
        //  Target = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        distance = 5.0f;
        if (Target != null)
        {
            //获取鼠标输入
            mX += Input.GetAxis("Mouse X") * SpeedX * 0.5F*Time.deltaTime;
			mY -= Input.GetAxis("Mouse Y") * SpeedY * 0.5F*Time.deltaTime;
            //范围限制
            mY = ClampAngle(mY, MinLimitY, MaxLimitY);
        }

        //鼠标滚轮缩放
        Distance -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
        Distance = Mathf.Clamp(Distance, MinDistance, MaxDistance);
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (Camera.main.fieldOfView <= 100)
                Camera.main.fieldOfView += 2;
            if (Camera.main.orthographicSize <= 20)
                Camera.main.orthographicSize += 0.5F;
        }
        //Zoom in
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (Camera.main.fieldOfView > 30)
                Camera.main.fieldOfView -= 2;
            if (Camera.main.orthographicSize >= 1)
                Camera.main.orthographicSize -= 0.5F;
        }
        //重新计算位置和角度
        Quaternion mRotation = Quaternion.Euler(mY, mX, 0);
        Vector3 mPosition = mRotation * new Vector3(0.0F, 0.0F, -distance) + Target.position;

        //设置相机的角度和位置
        {
            transform.rotation = mRotation;
            transform.position = mPosition;
        }
        Debug.DrawLine(rayFrom.transform.position, rayTo.transform.position, Color.red);
        RaycastHit hit;
        if (Physics.Linecast(rayFrom.transform.position, rayTo.transform.position, out hit))
        {
            string name = hit.collider.gameObject.tag;
            if (name != "terrain" && name != "Player"&&name !="AI"&&name!="weapon")
            {
                transform.position = hit.point;
                distance = Vector3.Distance(hit.point, rayTo.transform.position);
            }
        }

        IKtrans ik = model.GetComponent<IKtrans>();
        if (ik.ikActive == true)
        {
            float y, z;
            y = ikobj.localRotation.y;
            z = ikobj.localRotation.z;

            ikobj.localRotation = Quaternion.Euler(-mY, y, z);
            if (Input.GetButtonDown("Fire1"))
            {
                GameObject bull = (GameObject)Instantiate(prefabA, bullspot.transform.position, bullspot.transform.rotation);
                Rigidbody rb = bull.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = bullspot.transform.TransformDirection(Vector3.forward * 20);
                }

            }

        }
    }


    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

}
