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
    InputKey left, right, up, down, jump, attackA, attackB;
    float moveMultLeft = 1f, moveMultRight = 1f;
    float wallYVel = 0f;

    EntityStats.Attribute coyoteTime, jumpForce, airJumpForce;
    int mJumps = 1, cmJumps = 0;

    int cComboStage = 0; 
    float cComboTimer = 0, cComboLastAttackDelay = 0;

    Rect rect_stand = new Rect(new Vector2(0,0), new Vector2(0.8f, 1.8f));
    Rect rect_crouch = new Rect(new Vector2(0, -0.45f), new Vector2(0.8f, 0.9f));

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
        airJumpForce = entityStats.GetSetAttribute("airjumpforce", 6f);
        

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
        attackA = InputManager.SetInputKey("attackA", KeyCode.C);
        attackB = InputManager.SetInputKey("attackB", KeyCode.V);

    }


    protected override void FixedUpdate()
    {
        Vector2 dir = (Vector2)inputVector;

        base.FixedUpdate();

        Move(dir.x *= dir.x < 0 ? moveMultLeft : moveMultRight);
        if (wasGrounded < coyoteTime)
        {
            if (jump.down)
            { JumpInit(new Vector2(0, jumpForce), jumpForce); StartCoroutine(IWaitJumpRelease()); }

            if (isGrounded)
            {
                cmJumps = mJumps;
                if (attackA.down)
                {
                    if (cComboTimer > 0)
                    {
                        if (cComboTimer < cComboLastAttackDelay)
                        {
                            cComboStage++;
                            Debug.Log($"!Combo+ [{cComboStage}]");
                            cComboTimer = 1f;
                            cComboLastAttackDelay = cComboTimer - 0.2f;
                        }
                    }
                    else
                    {
                        cComboStage = 1;
                        Debug.Log($"!Combo Init [{cComboStage}]");
                        cComboTimer = 1f;
                        cComboLastAttackDelay = cComboTimer - 0.2f;
                        StartCoroutine(IComboTimer(cComboTimer));
                    }
                }
            }
        }
        else
        {

            if (ledgeLeft)
            {
                cmJumps = mJumps;
                if (dir.y < 0)
                { StartCoroutine(IHaltLedgeGrab(0.2f)); }
                else if (jump.down)
                { 
                    JumpInit(AngleToVector2(dir.x < 0 ? 90f : 60f) * jumpForce, jumpForce * 0.5f); 
                    StartCoroutine(IWaitJumpRelease()); 
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
            else if (ledgeRight)
            {
                cmJumps = mJumps;
                if (dir.y < 0)
                { StartCoroutine(IHaltLedgeGrab(0.2f)); }
                else if (jump.down)
                {
                    JumpInit(AngleToVector2(dir.x > 0 ? 90f : 105f) * jumpForce, jumpForce * 0.5f);
                    StartCoroutine(IWaitJumpRelease());
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
            else if(jump.down && cmJumps > 0 && _velocity.y <= 0 && _velocity.y > -10f)
            {
                cmJumps--;
                _velocity.y = 0;
                JumpInit(new Vector2(0, airJumpForce), airJumpForce); StartCoroutine(IWaitJumpRelease());
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

    IEnumerator IComboTimer(float time)
    {
        cComboTimer = time;
        while (cComboTimer > 0)
        {
            cComboTimer -= Time.deltaTime;
            yield return null;
        }
        cComboStage = 0;
        cComboTimer = 0;
        Debug.Log($"!Combo End");
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
