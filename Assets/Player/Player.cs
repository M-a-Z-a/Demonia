using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static InputManagerClasses;
using static Utility;

public class Player : PlayerController
{

    public static Player instance { get; protected set; }
    public static Transform pTransform { get; protected set; }

    MeshRenderer rend;
    Material mat;

    Transform cameraTarget;
    InputVector2 inputVector;
    InputKey left, right, up, down, jump;
    float moveMultLeft = 1f, moveMultRight = 1f;
    float wallYVel = 0f;

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

        rend = GetComponentInChildren<MeshRenderer>();
        mat = new Material(rend.material);
        rend.material = mat;

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
        up = inputVector.inputY.positive;
        down = inputVector.inputY.negative;

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
        else
        {
            if (ledgeLeft)
            {
                if (jump.down)
                { 
                    if (dir.y < 0)
                    { StartCoroutine(IHaltLedgeGrab(0.2f)); }
                    else
                    { 
                        JumpInit(AngleToVector2(dir.x < 0 ? 90f : 60f) * jumpForce, jumpForce * 0.5f); 
                        StartCoroutine(IWaitJumpRelease()); 
                    }

                }
            }
            else if (wallLeft)
            {
                //_velocity.y = Mathf.Max(-0.5f, _velocity.y);
                if (_velocity.y < 0)
                { _velocity.y = TowardsTargetValue(_velocity.y, 0, -(dir.y < 0 ? currentGravity * 0.5f : (dir.x < 0 ? currentGravity * 1f : currentGravity * 0.8f)) * Time.deltaTime); }
                if (jump.down && !ledgeLeft)
                {
                    JumpInit(AngleToVector2(dir.x > 0 ? 75f : 45f) * jumpForce, jumpForce * 0.5f);
                    StartCoroutine(IWaitLeftWalljump(0.4f));
                    StartCoroutine(IWaitJumpRelease());
                }
            }
            
            if (ledgeRight)
            {
                if (jump.down)
                {
                    if (dir.y < 0)
                    { StartCoroutine(IHaltLedgeGrab(0.2f)); }
                    else
                    { 
                        JumpInit(AngleToVector2(dir.x > 0 ? 90f : 105f) * jumpForce, jumpForce * 0.5f);
                        StartCoroutine(IWaitJumpRelease());
                    }
                }
            }
            else if (wallRight)
            {
                //_velocity.y = Mathf.Max(-0.5f, _velocity.y);
                if (_velocity.y < 0)
                { _velocity.y = TowardsTargetValue(_velocity.y, 0, -(dir.y < 0 ? currentGravity * 0.5f : (dir.x > 0 ? currentGravity * 1.2f : currentGravity * 0.8f)) * Time.deltaTime); }
                if (jump.down && !ledgeRight)
                {
                    JumpInit(AngleToVector2(dir.x < 0 ? 105f : 135f) * jumpForce, jumpForce * 0.5f);
                    StartCoroutine(IWaitRightWalljump(0.4f));
                    StartCoroutine(IWaitJumpRelease());
                }
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

    IEnumerator IHaltLedgeGrab(float time)
    {
        if (!ledgegrabEnabled) yield break;
        ledgegrabEnabled = false;
        yield return new WaitForSeconds(time);
        ledgegrabEnabled = true;
    }

    void DamageSlowdown(float pause_t = 0.2f, float fade_t = 0.25f)
    {
        StartCoroutine(IDamageFlash(Color.white, 0.2f));
        TimeControl.SetTimeScaleFadeForTime(0.05f, pause_t, 0f, fade_t);
    }

    IEnumerator IDamageFlash(Color color, float time)
    {
        Color orig_color = mat.color;
        
        mat.SetColor("_Color", color);
        mat.SetFloat("_ColorLerp", 1f);
        yield return new WaitForSeconds(time);
        mat.SetColor("_Color", orig_color);
        mat.SetFloat("_ColorLerp", 0f);
    }


    protected override void OnEnterGrounded(Vector2 velocity, float fallDamageDelta)
    {
        base.OnEnterGrounded(velocity, fallDamageDelta);
        if (fallDamageDelta > 0.1f)
        {
            float fdelta = Mathf.Clamp(fallDamageDelta*2, 0f, 1f);
            DamageSlowdown(fdelta * 1f, fdelta * 0.5f);
            _velocity.y = -velocity.y* EaseInOutCirc01( fdelta * 0.2f);
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
