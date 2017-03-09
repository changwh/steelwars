using UnityEngine;

public class TiltWindow : MonoBehaviour
{
	public Vector2 range = new Vector2(5f, 3f);

	Transform mTrans;
	Quaternion mStart;
	Vector2 mRot = Vector2.zero;

	void Start ()
	{
		mTrans = transform;
		mStart = mTrans.localRotation;//����������ת�Ƕ�
	}

	void Update ()
	{
		Vector3 pos = Input.mousePosition; //���λ��

		float halfWidth = Screen.width * 0.5f;
		float halfHeight = Screen.height * 0.5f;
		float x = Mathf.Clamp((pos.x - halfWidth) / halfWidth, -1f, 1f);//����value��ֵ��min��max֮�䣬 ���valueС��min������min�� ���value����max������max�����򷵻�value
        float y = Mathf.Clamp((pos.y - halfHeight) / halfHeight, -1f, 1f);
		mRot = Vector2.Lerp(mRot, new Vector2(x, y), Time.deltaTime * 5f); //��������֮������Բ�ֵ��

        mTrans.localRotation = mStart * Quaternion.Euler(-mRot.y * range.y, mRot.x * range.x, 0f);//����һ����ת�Ƕȣ���z����תz�ȣ���x����תx�ȣ���y����תy�ȣ���������˳��
    }//��Ԫ��Q=Q1*Q2 ��ʾQ��������Q2����ת������Q1����ת�Ľ�����������Ԫ������תҲ�ǿ��Ժϲ��ģ����ж����ת����ʱ��ʹ����Ԫ�����Ի�ø��ߵļ���Ч�ʡ�
}
