using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

using static InputManagerClasses;
using static UnityEngine.Rendering.DebugUI;
using static Utility;

public class Player : PlayerController
{
    public static Player instance { get; protected set; }
    public static Transform pTransform { get; protected set; }
    public Dictionary<string, Hitbox> melee_hbs;

    MeshRenderer rend;
    Material mat;
    MatAnimator animator;

    Transform cameraTarget;
    InputVector2 inputVector;
    InputKey left, right, up, down, jump, attackA, attackB;
    float moveMultLeft = 1f, moveMultRight = 1f;
    float wallYVel = 0f;
    int facing = 1;

    EntityStats.Attribute coyoteTime, jumpForce, airJumpForce;
    int mJumps = 1, cmJumps = 0;

    int cComboStage = 0; 
    float cComboTimer = 0, cComboLastAttackDelay = 0;

    float anim_run_speed = 32f / 16f * 0.3f * 10f;

    Rect rect_stand = new Rect(new Vector2(0,0), new Vector2(0.8f, 1.8f));
    Rect rect_crouch = new Rect(new Vector2(0, -0.45f), new Vector2(0.8f, 0.9f));

    Shockwave deathShockwave;

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
        mat = rend.material;

        animator = GetComponent<MatAnimator>();

        entityStats.SetAttribute("speed", 6f);
        entityStats.SetAttribute("accelSpeed", 30f);
        entityStats.SetAttribute("decelSpeed", 15f);
        coyoteTime = entityStats.GetSetAttribute("coyoteTime", 0.1f);
        jumpForce = entityStats.GetSetAttribute("jumpforce", 8f);
        airJumpForce = entityStats.GetSetAttribute("airjumpforce", 6f);

    }

    public void PlayStepSound()
    {
        //Debug.Log("step");
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

        animator.FetchAnimationData(atlas_path: "Data/player_atlas");
        animator.flagActions.Add("step", PlayStepSound);

        GetMeleeHitboxes();

        deathShockwave = transform.Find("Shockwave").GetComponent<Shockwave>();
        deathShockwave.Deactivate();

        //transform.position = GameManager.Checkpoint.position;
        //SetVelocity(Vector2.zero);
    }

    float aspeed = 1f;
    /*
    protected override void FixedUpdate()
    { base.FixedUpdate(); }
    */
    protected void Update()
    {
        Vector2 dir = (Vector2)inputVector;

        Move(dir.x *= dir.x < 0 ? moveMultLeft : moveMultRight);
        aspeed = 1f;
        if (wasGrounded < coyoteTime && !isJumping)
        {
            if (jump.down)
            { JumpInit(new Vector2(0, jumpForce), jumpForce); StartCoroutine(IWaitJumpRelease()); }

            if (isGrounded)
            {
                if (dir.x > 0)
                {
                    if (relativeVelocity.x < 0)
                    { 
                        animator.SetState("break");
                        animator.FlipX(true);
                    }
                    else
                    {
                        animator.SetState("run");
                        animator.FlipX(false);
                        aspeed = Mathf.Clamp(Mathf.Abs(velocity.x) / anim_run_speed, 0.5f, 2f);
                        facing = 1;
                    }
                }
                else if (dir.x < 0)
                { 
                    if (relativeVelocity.x > 0)
                    {
                        animator.SetState("break");
                        animator.FlipX(false);
                    }
                    else
                    {
                        animator.SetState("run");
                        animator.FlipX(true);
                        aspeed = Mathf.Clamp(Mathf.Abs(velocity.x) / anim_run_speed, 0.5f, 2f);
                        facing = -1;
                    }
                }
                else
                { 
                    if (relativeVelocity.x > 0.5f)
                    {
                        animator.SetState("break");
                        animator.FlipX(false);
                    }
                    else if (relativeVelocity.x < -0.5f)
                    {
                        animator.SetState("break");
                        animator.FlipX(true);
                    }
                    else
                    {
                        animator.SetState("idle");
                    }
                }

                cmJumps = mJumps;
                if (attackA.down)
                {
                    if (cComboTimer > 0)
                    {
                        if (cComboTimer < cComboLastAttackDelay)
                        {
                            Attack(dir);
                            cComboStage++;
                            Debug.Log($"!Combo+ [{cComboStage}]");
                            cComboTimer = 1f;
                            cComboLastAttackDelay = cComboTimer - 0.2f;
                        }
                    }
                    else
                    {
                        Attack(dir);
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
                //if (velocity.y <= 0) { 
                    animator.SetState("ledge_grab"); animator.FlipX(false); 
                //}
                cmJumps = mJumps;
                if (dir.y < 0)
                { StartCoroutine(IHaltLedgeGrab(0.2f)); }
                else if (jump.down)
                {
                    //if (dir.x < 0) animator.FlipX(true);
                    JumpInit(AngleToVector2(dir.x < 0 ? 90f : 60f) * jumpForce, jumpForce * 0.5f);
                    StartCoroutine(IWaitJumpRelease());
                }
            }
            else if (wallLeft)
            {
                //_velocity.y = Mathf.Max(-0.5f, _velocity.y);
                if (velocity.y <= 0) { 
                    animator.SetState("ledge_grab"); animator.FlipX(false); 
                } 
                else 
                { animator.SetState("air_ascend"); }

                if (_velocity.y < 0)
                { _velocity.y = TowardsTargetValue(_velocity.y, 0, -(dir.y < 0 ? currentGravity * 0.5f : (dir.x < 0 ? currentGravity * 1f : currentGravity * 0.8f)) * Time.deltaTime); }
                if (jump.down && !ledgeLeft)
                {
                    animator.FlipX(false);
                    Vector2 jforce = AngleToVector2(dir.x > 0 ? 75f : 45f) * jumpForce;
                    if (velocity.y > 0)
                    { jforce.y = Mathf.Min(velocity.y + jforce.y, jforce.y * 1.5f) - velocity.y; }
                    JumpInit(jforce, jumpForce * 0.5f);
                    StartCoroutine(IWaitLeftWalljump(0.4f));
                    StartCoroutine(IWaitJumpRelease());
                }
            }
            else if (ledgeRight)
            {
                //if (velocity.y <= 0) { 
                    animator.SetState("ledge_grab"); animator.FlipX(true); 
                //}
                cmJumps = mJumps;
                if (dir.y < 0)
                { StartCoroutine(IHaltLedgeGrab(0.2f)); }
                else if (jump.down)
                {
                    //if (dir.x > 0) animator.FlipX(false);
                    JumpInit(AngleToVector2(dir.x > 0 ? 90f : 105f) * jumpForce, jumpForce * 0.5f);
                    StartCoroutine(IWaitJumpRelease());
                }
            }
            else if (wallRight)
            {
                if (velocity.y <= 0) { 
                    animator.SetState("ledge_grab"); animator.FlipX(true);
                }
                else
                { animator.SetState("air_ascend"); }
                //_velocity.y = Mathf.Max(-0.5f, _velocity.y);
                if (_velocity.y < 0)
                { _velocity.y = TowardsTargetValue(_velocity.y, 0, -(dir.y < 0 ? currentGravity * 0.5f : (dir.x > 0 ? currentGravity * 1.2f : currentGravity * 0.8f)) * Time.deltaTime); }
                if (jump.down && !ledgeRight)
                {
                    animator.FlipX(true);
                    Vector2 jforce = AngleToVector2(dir.x < 0 ? 105f : 135f) * jumpForce;
                    if (velocity.y > 0)
                    { jforce.y = Mathf.Min(velocity.y + jforce.y, jforce.y * 1.5f) - velocity.y; }
                    
                    JumpInit(jforce, jumpForce * 0.5f);
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
            else
            {
                aspeed = Mathf.Clamp(velocity.y * 0.5f, 0.5f, 2f);
                if (velocity.y > 2f) animator.SetState("air_ascend");
                else if (velocity.y < -2f) animator.SetState("air_descend");
                else animator.SetState("air_float");
            }
        }
        animator.animSpeed = aspeed;
    }

    void Attack(Vector2 dir)
    {
        if (dir.y > 0) { Attack_Up(); return; }
        if (dir.y < 0) { Attack_Down(); return; }
        Attack_Neutral();
    }

    Hitbox _chbox;
    void Attack_Neutral()
    {
        _chbox = melee_hbs["melee_grounded_neutral"];
        _chbox.xflip = facing < 0;
        _chbox.Enable();
    }
    void Attack_Up()
    {
        _chbox = melee_hbs["melee_grounded_up"];
        _chbox.xflip = facing < 0;
        _chbox.Enable();
    }
    void Attack_Down()
    {
        _chbox = melee_hbs["melee_grounded_down"];
        _chbox.xflip = facing < 0;
        _chbox.Enable();
    }

    
    void GetMeleeHitboxes()
    {
        melee_hbs = new();
        Hitbox[] hboxes = transform.Find("MeleeHitboxes")?.GetComponentsInChildren<Hitbox>();
        foreach (Hitbox hbox in hboxes)
        {
            hbox.origin = this;
            hbox.Disable();
            melee_hbs.Add(hbox.gameObject.name, hbox);
        }
    }

    IEnumerator IWaitLeftWalljump(float time = 0.5f)
    {
        float t = 0;
        while (t < time || !wallRight)
        {
            t += Time.deltaTime;
            moveMultLeft = Mathf.Lerp(0f, 1f, t / time);
            yield return new WaitForEndOfFrame();
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
            yield return new WaitForEndOfFrame();
        }
        moveMultRight = 1f;
    }
    IEnumerator IWaitJumpRelease(float max_wait = 5f)
    {
        float t = 0;
        while (jump.hold && t < max_wait)
        {
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
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

    void DIEEE()
    {
        deathShockwave.Activate();
        StartCoroutine(IDeathFade(0.1f, 0.2f));
        GameManager.Reset_Game_Fade();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Hazard":
                DIEEE();
                break;
            case "Checkpoint":
                if (collision.gameObject.TryGetComponent<Checkpoint>(out Checkpoint c))
                { c.SetActiveCheckpoint(); }
                break;
        }
    }

    IEnumerator IDeathFade(float t_a, float t_b)
    {
        float t = 0;
        float d;
        Color cs = mat.color;
        while (t < t_a)
        {
            t += Time.unscaledDeltaTime;
            d = t / t_a;
            mat.SetFloat("_ColorLerp", d);
            mat.color = Color.Lerp(cs, Color.white, d);
            yield return null;
        }
        t = 0;
        while (t < t_b)
        {
            d = t / t_b;
            t += Time.unscaledDeltaTime;
            mat.SetFloat("_Transparency", 1f - d);
            yield return null;
        }
    }

}
