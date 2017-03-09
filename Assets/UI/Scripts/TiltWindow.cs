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
		mStart = mTrans.localRotation;//自身物体旋转角度
	}

	void Update ()
	{
		Vector3 pos = Input.mousePosition; //鼠标位置

		float halfWidth = Screen.width * 0.5f;
		float halfHeight = Screen.height * 0.5f;
		float x = Mathf.Clamp((pos.x - halfWidth) / halfWidth, -1f, 1f);//限制value的值在min和max之间， 如果value小于min，返回min。 如果value大于max，返回max，否则返回value
        float y = Mathf.Clamp((pos.y - halfHeight) / halfHeight, -1f, 1f);
		mRot = Vector2.Lerp(mRot, new Vector2(x, y), Time.deltaTime * 5f); //两个向量之间的线性插值。

        mTrans.localRotation = mStart * Quaternion.Euler(-mRot.y * range.y, mRot.x * range.x, 0f);//返回一个旋转角度，绕z轴旋转z度，绕x轴旋转x度，绕y轴旋转y度（像这样的顺序）
    }//四元数Q=Q1*Q2 表示Q的是先做Q2的旋转，再做Q1的旋转的结果，而多个四元数的旋转也是可以合并的，当有多次旋转操作时，使用四元数可以获得更高的计算效率。
}
