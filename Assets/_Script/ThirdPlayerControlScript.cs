

//ThirdPersonController.cs


using UnityEngine;
using System.Collections;

[AddComponentMenu("controls/ThirdPlayerControl")]
public class ThirdPlayerControlScript : MonoBehaviour
{
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip jumpPoseAnimation;

    public float walkMaxAnimationSpeed=0.75f;
    public float trotMaxAnimationSpeed = 1.0f;
    public float runMaxAnimationSpeed = 1.0f;
    public float jumpAnimationSpeed = 1.15f;
    public float landAnimationSpeed = 1.0f;

    private Animation _animation;
    
    enum CharaCterSate
    {
        Idle=0,
        Walking=1,
        Trotting=2,
        Running=3,
        Jumping=4
    }
    private CharaCterSate _characterState;
    // The speed when walking�����ٶ�
    public float walkSpeed = 2.0f;
    // after trotAfterSeconds of walking we trot with trotSpeed
    public float trotSpeed = 4.0f;
    public float runSpeed = 6.0f;
    public float inAirControlAcceleration=3.0f;
    //���ĸ߶�
    public float JumpHeight = 0.5f;
    public float gravity = 20;//����
    public float speedSmoothing = 10.0f;
    public float rotateSpeed = 500;
    public float trotAfterSeconds=3.0f;
    public bool canJump = true;

    private float JumpRepeatTime = 0.05f;
    private float jumpTimeout = 0.15f;
    private float groundedTimeout = 0.25f;
    private float lockCameraTimer = 0;
    private Vector3 moveDirection = Vector3.zero;//xz��Ļ�ϵ��ƶ�����
    private float verticalSpeed = 0;//��ǰ��ֱ�ٶ�
    private float moveSpeed = 0;//��ǰ��xz��Ļ���ƶ��ٶ�
    //CollisionFlags��CharacterController.Move���ص�λ���롣
    private CollisionFlags collisionFlags;
    private bool jumping = false;
    private bool jumpingReachedApex = false;
    //�ж��Ƿ���ˣ������������180��ת��
    private bool movingBack = false;
    // Is the user pressing any keys?
    private bool isMoving = false;
    //����ɫ��ʼ���ߵ�ʱ��
    private float walkTimeStart = 0;
    private float lastJumpButtonTime = -10.0f;
    private float lastJumpTime = -1.0f;
    private float lastJumpStartHeight = 0f;
    private Vector3 inAirVelocity = Vector3.zero;
    private float lastGroundedTime = 0;
    private bool isControlLable = true;

    void Awake()
    {
        moveDirection = transform.TransformDirection(Vector3.forward);

        _animation = this.gameObject.GetComponent<Animation>();
        if(_animation)
            Debug.Log("The character you would like to control doesn't have animations. Moving her might look weird.");

        if (!idleAnimation)
        {
            _animation = null;
            Debug.Log("No idle animation found. Turning off animations.");
        }
        if(!walkAnimation)
        {
            _animation = null;
            Debug.Log("No walk animation found. Turning off animations.");
        }
        if(!runAnimation)
        {
            _animation = null;
            Debug.Log("No run animation found. Turning off animations.");
        }
        if(!jumpPoseAnimation && canJump)
        {
            _animation = null;
            Debug.Log("No jump animation found and the character has canJump enabled. Turning off animations.");
        }
    }
    

    void Update ()
    {
        if(!isControlLable)
        {
            //��һ֡���������е����룬��������ָ��֮�����еķ����ᶼ������Ϊ0�������еİ�����������Ϊ0��
            Input.ResetInputAxes();
        }
        if(Input.GetButtonDown("Jump"))
        {
            lastJumpButtonTime = Time.time;
        }
        UpdateSmoothedMovementDirection();

        ApplyGravity();
        ApplyJumping();

        // Calculate actual motion����ʵ���˶�.
        Vector3 movement= moveDirection * moveSpeed +new Vector3(0, verticalSpeed, 0) + inAirVelocity;
        movement *= Time.deltaTime;

        //Move The controller
        CharacterController controller = GetComponent<CharacterController>();
        collisionFlags = controller.Move(movement);

        //Animation sector
        if(_animation)
        {
            if(_characterState==CharaCterSate.Jumping)
            {
                if(!jumpingReachedApex)
                {
                    _animation[jumpPoseAnimation.name].speed = jumpAnimationSpeed;
                    _animation[jumpPoseAnimation.name].wrapMode = WrapMode.ClampForever;
                    _animation.CrossFade(jumpPoseAnimation.name);
                }
            }
            else
            {
                if(controller.velocity.sqrMagnitude<0.1)
                {
                    _animation.CrossFade(idleAnimation.name);
                }
                else
                {
                    if(_characterState==CharaCterSate.Running)
                    {
                        _animation[runAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0, runMaxAnimationSpeed);
                        _animation.CrossFade(runAnimation.name);
                    }
                    else if(_characterState==CharaCterSate.Trotting)
                    {
                        _animation[walkAnimation.name].speed = Mathf.Clamp(controller.velocity.magnitude, 0, trotMaxAnimationSpeed);
                            _animation.CrossFade(walkAnimation.name);
                    }
                    else if(_characterState==CharaCterSate.Walking)
                    {
                        _animation[walkAnimation.name].speed=Mathf.Clamp(controller.velocity.magnitude,0,walkMaxAnimationSpeed);
                        _animation.CrossFade(walkAnimation.name);
                    }
                }
            }
        }

        // Set rotation to the move direction�趨��ת�ƶ�����
        if(IsGrounded())
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
        else
        {
            Vector3 xzMove = movement;
            xzMove.y = 0;
            if(xzMove.sqrMagnitude>0.001)
            {
                transform.rotation = Quaternion.LookRotation(xzMove);
            }
        }
        // We are in jump mode but just became grounded
        if(IsGrounded())
        {
            lastGroundedTime=Time.time;
            inAirVelocity=Vector3.zero;
            if(jumping)
            {
                jumping=false;
                SendMessage("DidLand",SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void UpdateSmoothedMovementDirection()
    {
        Transform cameraTransform = Camera.main.transform;
        bool grounded = IsGrounded();
        //����������xz��Ļ��ǰ��
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0;
        forward = forward.normalized;
        //����� �������
        //Always orthogonal to the forward vector���Ǵ�ֱ��ǰ������
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        var v = Input.GetAxisRaw("Vertical");
        var h = Input.GetAxisRaw("Horizontal");

        // Are we moving backwards or looking backwards
        if (v < -0.2f)
            movingBack = true;
        else
            movingBack = false;

        bool wasMoving = isMoving;
        isMoving = Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1;
        // Target direction relative to the camera���������ķ���
        Vector3 targetDirection = h * right + v * forward;

        if(grounded)
        {
            // Lock camera for short period when transitioning moving & standing still
            //�����ʱ,��ʱ����ת��&վ�Ų���
            lockCameraTimer += Time.deltaTime;
            if (isMoving != wasMoving)
                lockCameraTimer = 0.0f;

            // We store speed and direction seperately,
            // so that when the character stands still we still have a valid forward direction
            //���Ե������ɫվ��������Ȼ��һ����Ч��ǰ������
            // moveDirection is always normalized, and we only update it if there is user input.
            //moveDirection����normalized�ģ�����ֻ���û������ʱ�����
            if(targetDirection!=Vector3.zero)
            {
                // If we are really slow, just snap to the target direction
                //���������ĺ���,ֻ����ǰ��Ŀ�귽��
                if(moveSpeed<walkSpeed * 0.9 && grounded)
                {
                    moveDirection = targetDirection.normalized;
                }
                // Otherwise smoothly turn towards it
                //ƽ��ת��
                else
                {
                    moveDirection = Vector3.RotateTowards(moveDirection, targetDirection, rotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000);
                    moveDirection = moveDirection.normalized;
                }
            }
            // Smooth the speed based on the current target direction
            //���ݵ�ǰ�ٶ�ƽ���ٶ�
            float curSmooth = speedSmoothing * Time.deltaTime;
            float targetSpeed = Mathf.Min(targetDirection.magnitude, 1.0f);

            _characterState = CharaCterSate.Idle;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                targetSpeed *= runSpeed;
                _characterState = CharaCterSate.Running;
            }
            else if (Time.time - trotAfterSeconds > walkTimeStart)
            {
                targetSpeed *= trotSpeed;
                _characterState = CharaCterSate.Trotting;
            }
            else
            {
                targetSpeed *= walkSpeed;
                _characterState = CharaCterSate.Walking;
            }

            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, curSmooth);
            // Reset walk time start when we slow down
            if (moveSpeed < walkSpeed * 0.3)
            {
                walkTimeStart = Time.time;
            }
        }
        
        else
        {
            //lock camera while in air
            if (jumping)
                lockCameraTimer = 0.0f;
            if (isMoving)
                inAirVelocity += targetDirection.normalized * Time.deltaTime * inAirControlAcceleration;
        }

    }

    void ApplyJumping()
    {
        if(lastJumpTime+JumpRepeatTime>Time.time)
            return;
        if(IsGrounded())
        {
            // - Only when pressing the button down
            // - With a timeout so you can press the button slightly before landing
            //��ʱ���������½ʱ���°�ť
            if(canJump && Time.time < lastJumpButtonTime+jumpTimeout)
            {
                verticalSpeed = CalculateJumpVerticalSpeed(JumpHeight);
                SendMessage("DidJump", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void ApplyGravity()
    {
        if(isControlLable)
        {
            // Apply gravity
            bool jumpButton = Input.GetButton("Jump");
            // When we reach the apex of the jump we send out a message������������ʱ������Ϣ
            if(jumping && !jumpingReachedApex && verticalSpeed<=0.0f)
            {
                jumpingReachedApex = true;
                SendMessage("DidJumpReachApex",SendMessageOptions.DontRequireReceiver);
            }
            if (IsGrounded())
                verticalSpeed = 0;
            else
                verticalSpeed -= gravity * Time.deltaTime;
        }
    }

    void DidJump()
    {
        jumping = true;
        jumpingReachedApex = false;
        lastJumpTime = Time.time;
        lastJumpStartHeight = transform.position.y;
        lastJumpButtonTime = -10;
        _characterState = CharaCterSate.Jumping;
    }

    float CalculateJumpVerticalSpeed(float targetJumpHeight)
    {
        // From the jump height and gravity we deduce the upwards
        // for the character to reach at the apex.
        //speed ���������ĸ߶Ⱥ������Ƴ��������ϴﵽ�ĸ߶�
        return Mathf.Sqrt(2 * targetJumpHeight * gravity);
    }

    bool IsGrounded()
    {
        return (collisionFlags & CollisionFlags.CollidedBelow) != 0;
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.moveDirection.y > 0.01)
            return;
    }

    public float GetSpeed()
    {
        return moveSpeed;
    }
    public bool IsJumping()
    {
        return jumping;
    }

    public Vector3 GetDirection()
    {
        return moveDirection;
    }
    public bool IsMovingBackwards()
    {
        return movingBack;
    }
    public float GetLockCameraTimer()
    {
        return lockCameraTimer;
    }

    public bool IsMoving()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.5f;
    }

    public bool HasJumpReachedApex()
    {
        return jumpingReachedApex;
    }

    public bool IsGroundWithTimeout()
    {
        return lastGroundedTime+groundedTimeout>Time.time;
    }

    public void Reset()
    {
        gameObject.tag = "Player";
    }
}
