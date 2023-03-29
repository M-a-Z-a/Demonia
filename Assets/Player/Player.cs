using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static InputManagerClasses;
using static Utility;

public class Player : PlayerController
{

    public static Player instance { get; protected set; }
    public static Transform pTransform { get; protected set; }


    Transform cameraTarget;
    InputVector2 inputVector;
    InputKey left, right, jump;
    float moveMultLeft = 1f, moveMultRight = 1f;
    

    EntityStats.Attribute coyoteTime, jumpForce, airJumpForce;

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    protected override void Awake()
    {
        base.Awake();
        instance = this;

        pTransform = transform;
        cameraTarget = transform.Find("CameraTarget");

        coyoteTime = entityStats.GetSetAttribute("coyoteTime", 0.1f);
        jumpForce = entityStats.GetSetAttribute("jumpforce", 8f);
        airJumpForce = entityStats.GetSetAttribute("airjumpforce", 4f);
        

        Debug.Log($"jumpforce: {jumpForce.value}");
    }



    protected override void Start()
    {
        base.Start();
        inputVector = InputManager.GetInputVector<InputVector2>("direction");
        left = inputVector.inputX.negative;
        right = inputVector.inputX.positive;
        jump = InputManager.SetInputKey("jump", KeyCode.X);

    }


    protected override void Update()
    {
        Vector2 dir = (Vector2)inputVector;

        base.Update();

        Move(dir.x *= dir.x < 0 ? moveMultLeft : moveMultRight);
        if (isGrounded || wasGrounded < coyoteTime)
        {
            if (jump.down)
            { JumpInit(new Vector2(0, jumpForce), jumpForce); StartCoroutine(IWaitJumpRelease()); }
        }

        if (wallLeft)
        {
            _velocity.y = Mathf.Max(-0.5f, _velocity.y);
            if (jump.down)
            { 
                JumpInit(new Vector2(1, 1).normalized * jumpForce, jumpForce * 0.5f); 
                StartCoroutine(IWaitLeftWalljump(0.4f));
                StartCoroutine(IWaitJumpRelease());
            }
        }

        if (wallRight)
        {
            _velocity.y = Mathf.Max(-0.5f, _velocity.y);
            if (jump.down)
            { 
                JumpInit(new Vector2(-1, 1).normalized * jumpForce, jumpForce * 0.5f);
                StartCoroutine(IWaitRightWalljump(0.4f));
                StartCoroutine(IWaitJumpRelease());
            }
        }
    }

    IEnumerator IWaitLeftWalljump(float time = 0.5f)
    {
        float t = 0;
        while (t < time || !wallRight)
        {
            t += Time.deltaTime;
            moveMultLeft = Mathf.Lerp(0f, 1f, t / time);
            yield return null;
        }
        moveMultLeft = 1f;
    }
    IEnumerator IWaitRightWalljump(float time = 0.5f)
    {
        float t = 0;
        while (t < time || !wallLeft)
        {
            t += Time.deltaTime;
            moveMultRight = Mathf.Lerp(0f, 1f, t / time);
            yield return null;
        }
        moveMultRight = 1f;
    }
    IEnumerator IWaitJumpRelease(float max_wait = 5f)
    {
        float t = 0;
        while (jump.hold && t < max_wait)
        {
            t += Time.deltaTime;
            yield return null; 
        }
        JumpRelease();
    }


    protected override void OnEnterGrounded(Vector2 velocity, float fallDamageDelta)
    {
        base.OnEnterGrounded(velocity, fallDamageDelta);
        if (fallDamageDelta > 0.2f)
        {
            TimeControl.SetTimeScaleFadeForTime(0, 0.5f, 0f, 1f);
            _velocity.y = 5f;
        }
    }
    protected override void OnTouchWallLeft(Vector2 velocity)
    {
        base.OnTouchWallLeft(velocity);

    }
    protected override void OnTouchWallRight(Vector2 velocity)
    {
        base.OnTouchWallRight(velocity);

    }

}
