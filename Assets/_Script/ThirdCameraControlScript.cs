 //ThirdPersonCamera.cs


using UnityEngine;
using System.Collections;

[AddComponentMenu("controls/ThirdCameraControl")]
public class ThirdCameraControlScript : MonoBehaviour
{
    public Transform cameraTransform;
    private Transform target;
	public GameObject rayFrom;
	public GameObject rayTo;
    public float distance = 7.0f;
    public float height = 3.0f;
    public float angularSmoothLag = 0.3f;
    public float angularMaxSpeed = 15.0f;
    public float heightSmoothLag = 0.3f;
    public float snapSmoothLag = 0.2f;
    public float snapMaxSpeed = 720.0f;
    public float clampHeadPositionScreenSpace = 0.75f;
    public float lockCameraTimeout = 0.2f;
    private Vector3 headOffset = Vector3.zero;
    private Vector3 centerOffset = Vector3.zero;
    private float heightVelocity = 0.0f;
    private float angleVelocity = 0.0f;
    private bool snap = false;
    private ThirdPlayerControlScript controller;
    private float targetHeight = 100000.0f;
	

    void Start ()
    {
        if (!cameraTransform && Camera.main)
            cameraTransform = Camera.main.transform;
        if(!cameraTransform)
        {
            Debug.Log("Please assign a camera to the ThirdPersonCamera script.");
            enabled = false;
        }
        target = transform;
        if (target)
            controller = target.GetComponent<ThirdPlayerControlScript>();
        if (controller)
        {
            CharacterController characterController = target.GetComponent<CharacterController>();
            centerOffset = characterController.bounds.center - target.position;
            headOffset = centerOffset;
            headOffset.y = characterController.bounds.max.y - target.position.y;
        }
        else
            Debug.Log("Please assign a target to the camera that has a ThirdPersonController script attached.");

        Cut(target, centerOffset);
    }

    void Cut(Transform dummyTarget,Vector3 dummyCenter)
    {
        float oldHeightSmooth = heightSmoothLag;
        float oldSnapMaxSpeed = snapMaxSpeed;
        float oldSnapSmooth = snapSmoothLag;

        snapMaxSpeed = 10000;
        snapSmoothLag = 0.001f;
        heightSmoothLag = 0.001f;
        snap = true;
        Apply(transform, Vector3.zero);
        heightSmoothLag = oldHeightSmooth;
        snapMaxSpeed = oldSnapMaxSpeed;
        snapSmoothLag = oldSnapSmooth;
    }
    
    float AngleDistance(float a,float b)
    {
        a = Mathf.Repeat(a, 360);
        b = Mathf.Repeat(b, 360);

        return Mathf.Abs(b-a);
    }

    void Apply(Transform dummyTarget,Vector3 dummyCenter)
    {
        // Early out if we don't have a target
        if (!controller)
            return;

        Vector3 targetCenter = target.position + centerOffset;
        Vector3 targetHead = target.position + headOffset;

        // Calculate the current & target rotation angles���㵱ǰ��Ŀ����ת�Ƕ�
        float originalTargetAngle = target.eulerAngles.y;
        float currentAngle = cameraTransform.eulerAngles.y;

        // Adjust real target angle when camera is locked���������ʱ������������Ŀ��Ƕ�
        float targetAngle=originalTargetAngle;
        // When pressing Fire2 (alt) the camera will snap to the target direction real quick.
        //������Fire2(alt)������ܿ�������Ŀ�귽��
        // It will stop snapping when it reaches the target
        //���쵽��Ŀ��ʱ��ֹͣ����
        if(snap)
        {
            // We are close to the target, so we can stop snapping now!
            if (AngleDistance(currentAngle, originalTargetAngle) < 3.0)
                snap = false;
            //����ʱ��������𽥸ı�һ�������ĽǶȵ������ĽǶȡ�
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle,ref angleVelocity, snapSmoothLag, snapMaxSpeed);

        }
        // Normal camera motion//����������˶�
        else
        {
            if(controller.GetLockCameraTimer() < lockCameraTimeout)
            {
                targetAngle = currentAngle;
            }
            // Lock the camera when moving backwards!//������ʱ�������
            // * It is really confusing to do 180 degree spins when turning around.//���Ƕ���ĺܴ�ʱ����ת180��
            if(AngleDistance(currentAngle,targetAngle)>160 && controller.IsMovingBackwards())
            {
                targetAngle += 180;
            }
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, angularSmoothLag, angularMaxSpeed);

        }
        // When jumping don't move camera upwards but only down!
        if(controller.IsJumping())
        {
            //��������ĺܸ�ʱ�����ǲ������ƶ����
            // We'd be moving the camera upwards, do that only if it's really high
            float newTargetHeight = targetCenter.y + height;
            if(newTargetHeight<targetHeight || newTargetHeight-targetHeight>5)
            {
                targetHeight = targetCenter.y + height;
            }
        }
        else
        {
            targetHeight = targetCenter.y + height;
        }
        // Damp the height����߶�
        float currentHeight = cameraTransform.position.y;
        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothLag);
        // Convert the angle into a rotation, by which we then reposition the camera���ǶȻ���Y����תֵ,Ȼ���������¶�λ���
        Quaternion currentRotation = Quaternion.Euler(0, currentAngle, 0);
        // Set the position of the camera on the x-z plane to:x-zƽ�������������λ����
        // distance meters behind the target����target��meters�ױ���
        cameraTransform.position = targetCenter;
        cameraTransform.position += currentRotation * Vector3.back * distance;
        // Set the height of the camera��������߶�
        cameraTransform.position = new Vector3(cameraTransform.position.x, currentHeight, cameraTransform.position.z);
		SetUpRotation(targetCenter, targetHead);
        
    }
	

    void LateUpdate ()
    {
        Apply(transform, Vector3.zero);
		Debug.DrawLine(rayFrom.transform.position,rayTo.transform.position,Color.red);//??
		RaycastHit hit;
		if(Physics.Linecast(rayFrom.transform.position,rayTo.transform.position,out hit))
		{
			string name =  hit.collider.gameObject.tag;
			if(name !="terrain"&&name!="Player")
			{
				
				rayFrom.transform.position = hit.point;
				
				/*print(hit.point.x);     //??
				print(hit.point.y);
				print(hit.point.z);*/
			}
		}
	}

    

    void SetUpRotation(Vector3 centerPos,Vector3 headPos)
    {
        // Now it's getting hairy. The devil is in the details here, the big issue is jumping of course.
        //����Խ��Խ��ë����ħ������ϸ��,��Ȼ������������Ծ��
        // * When jumping up and down we don't want to center the guy in screen space.
        //����Ծʱ���ǲ����������Ļ�ռ���м���������
        //  This is important to give a feel for how high you jump and avoiding large camera movements.
        //   ����Ҫ��������Ӧ����ô����о�����ڲ����ܴ���ƶ�
        // * At the same time we dont want him to ever go out of screen and we want all rotations to be totally smooth.
        //ͬʱ���ǲ��������뿪��Ļ���������������е���ת����ƽ��
        // So here is what we will do:�����������Ҫ����
        //
        // 1. We first find the rotation around the y axis. Thus he is always centered on the y-axis
        //�����������������Y����������Ҳ������Y�������
        // 2. When grounded we make him be centered//����ɫ�ŵ�ʱ����ʹ��ɫ�����������
        // 3. When jumping we keep the camera rotation but rotate the camera to get him back into view if his head is above some threshold
        //����ɫ��Ծʱ���������ת������ɫͷ����������ֵʱ������ת���ʹ�ý�ɫ�ص���ͼ���������
        // 4. When landing we smoothly interpolate towards centering him on screen
        //���ŵ�ʱ����ƽ������ʹ�ý�ɫ�ص���Ļ������

        Vector3 cameraPos = cameraTransform.position;
        Vector3 offsetToCenter = centerPos - cameraPos;
        // Generate base rotation only around y-axis//���ɽ�������Y�����ת
        Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));
        Vector3 relativeOffset = Vector3.forward * distance + Vector3.down * height;
        cameraTransform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);

        //Calculate the projected center position and top position in world space
        //����ͶӰ����λ�õ�����λ�õľ���
        Ray centerRay = cameraTransform.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, 0.5f, 1));
        Ray topRay = cameraTransform.GetComponent<Camera>().ViewportPointToRay(new Vector3(0.5f, clampHeadPositionScreenSpace, 1));
        //��������������distance���뵥λ�ĵ㡣
        Vector3 centerRayPos = centerRay.GetPoint(distance);
        Vector3 topRayPos = topRay.GetPoint(distance);
        //���������붥������֮��ĽǶ�
        float centerToTopAngle = Vector3.Angle(centerRay.direction, topRay.direction);
        float heightToAngle=centerToTopAngle/(centerRayPos.y-topRayPos.y);
        float extraLookAngle=heightToAngle*(centerRayPos.y-centerPos.y);
        if(extraLookAngle<centerToTopAngle)
        {
            extraLookAngle = 0;
        }
        else
        {
            extraLookAngle = extraLookAngle - centerToTopAngle;
            cameraTransform.rotation *= Quaternion.Euler(-extraLookAngle, 0, 0);
        }
    }

    public Vector3 GetCenterOffset()
    {
        return centerOffset;
    }


}