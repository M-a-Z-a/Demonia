using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Light2D = UnityEngine.Rendering.Universal.Light2D;

using static InputManagerClasses;
using static Utility;

public partial class Player : PlayerController
{
    public static Player instance { get; protected set; }
    public static Transform pTransform { get; protected set; }
    public static Transform pDamageRelayTransform { get; protected set; }

    MeshRenderer rend;
    Material mat;
    MatAnimator animator;

    Transform cameraTarget;
    Light2D playerLight, playerVision;
    InputVector2 inputVector;
    InputKey left, right, up, down, jump, dash, attackA, attackB, interact;
    float moveMultLeft = 1f, moveMultRight = 1f, moveXMult = 1f;
    float wallYVel = 0f;
    int facing = 1;
    bool isCrouched = false;

    EntityStats.Attribute coyoteTime, jumpForce, airJumpForce;
    EntityStats.Attribute djump, wgrab;
    int mJumps = 0, cmJumps = 0;

    int cComboStage = 0; 
    float cComboTimer = 0, cComboLastAttackDelay = 0;
    float wallSlow = 1f;

    float anim_run_speed = 32f / 16f * 0.3f * 10f;

    //Rect rect_stand = new Rect(new Vector2(0,0), new Vector2(0.8f, 1.8f));
    //Rect rect_crouch = new Rect(new Vector2(0, -0.45f), new Vector2(0.8f, 0.9f));

    Shockwave swJumpAir, swDeath;
    Vector2 dir;

    float jumpHoldTime = 0.4f;
    float chargeAmount = 0f;
    int isCharging = 0, attackState = 0;
    bool wgInit = false, meleeAirJump = false, last_maj = false;

    //string curAttackState = "";
    int curAttack = 0;

    EntityStats.Stat stat_hp, stat_sp, stat_mp;

    [SerializeField] RelayCollider damageRelay, damageRelayCrouch, intRelay, intRelayCrouch;

    [SerializeField] SinMove[] jumpTrails, dashTrails;
    //TrailRenderer[] jumpTrailRend = { };
    [SerializeField]
    AudioSource asource, asourceFeet;
    [SerializeField]
    AudioClip soundStep, soundChargeup, soundSlash, soundAxeExpl;

    [SerializeField]
    ProjClass projectiles;


    Rect standRect, crouchRect = new Rect(new Vector2(0, -0.5f), new Vector2(0.75f, 0.8f));

    string meleeChargeAnim = "melee.g.n.c.release", 
        meleeUpChargeAnim = "melee.g.u.c.release",
        meleeDownChargeAnim = "melee.g.d.c.release";

    string meleeDownAirCharge = "melee.a.d.init",
        meleeDownAirRelease = "melee.a.d.release",
        meleeDownAirGround = "melee.a.d.ground";

    string meleeUpAir = "melee.a.u";

    [System.Serializable]
    class ProjClass
    {
        public GameObject meleeUp, meleeForward, meleeDown;
        public GameObject chargeMeleeUp, chargeMeleeForward, chargeMeleeDown;
        public GameObject chargeShockwave;
    }
    

    protected override void OnValidate()
    {
        base.OnValidate();
    }

    protected override void Awake()
    {
        base.Awake();
        instance = this;

        pTransform = transform;
        pDamageRelayTransform = damageRelay.transform;
        cameraTarget = transform.Find("CameraTarget");

        rend = GetComponentInChildren<MeshRenderer>();
        mat = rend.material;

        animator = GetComponent<MatAnimator>();

        entityStats.SetAttribute("speed", 8f);
        entityStats.SetAttribute("accelSpeed", 40f);
        entityStats.SetAttribute("decelSpeed", 15f);
        djump = entityStats.GetSetAttribute("djump", 0);
        wgrab = entityStats.GetSetAttribute("wgrab", 0);
        djump.onValueChanged.AddListener(OndjumpChanged);
        wgrab.onValueChanged.AddListener(OnwgrabChanged);
        coyoteTime = entityStats.GetSetAttribute("coyoteTime", 0.1f);
        jumpForce = entityStats.GetSetAttribute("jumpforce", 10f);
        airJumpForce = entityStats.GetSetAttribute("airjumpforce", 10f);

        asource.volume = 0.1f;
    }
    

    protected override void Start()
    {
        base.Start();

        standRect = coll.Rect();
        playerLight = transform.Find("Player Light")?.GetComponent<Light2D>();
        playerVision = transform.Find("Player Vision")?.GetComponent<Light2D>();
        
        inputVector = InputManager.GetInputVector<InputVector2>("direction");
        left = inputVector.inputX.negative;
        right = inputVector.inputX.positive;
        up = inputVector.inputY.positive;
        down = inputVector.inputY.negative;

        dash = InputManager.SetInputKey("dash", KeyCode.Z);
        jump = InputManager.SetInputKey("jump", KeyCode.X);
        attackA = InputManager.SetInputKey("attackA", KeyCode.C);
        attackB = InputManager.SetInputKey("attackB", KeyCode.V); 
        interact = InputManager.SetInputKey("interact", KeyCode.A);

        animator.FetchAnimationData();
        //animator.FetchAnimationData(atlas_path: "Data/player_atlas");
        animator.flagActions.Add("step", OnStep);
        animator.flagActions.Add("attack", OnAttack);
        animator.flagActions.Add("charging", OnCharge);
        animator.onAnimEnd.Add(OnAnimEnd);

        Transform effs = transform.Find("Effects");
        swDeath = effs.Find("ShockwaveDeath").GetComponent<Shockwave>();
        swDeath.Deactivate();
        swJumpAir = effs.Find("ShockwaveAirJump").GetComponent<Shockwave>();
        swJumpAir.Deactivate();
        gravityMultiplier = 1.25f;

        stat_hp = entityStats.SetStat("health", 100, 100);
        stat_sp = entityStats.SetStat("energy", 100, 100);
        stat_mp = entityStats.SetStat("mana", 100, 100);

        stat_hp.onValueChanged.AddListener(OnHealthChanged);
        stat_sp.onValueChanged.AddListener(OnEnergyChanged);
        stat_mp.onValueChanged.AddListener(OnManaChanged);

        entityStats.onStatusEffectAdded.AddListener(OnAddEffect);
        entityStats.onStatusEffectRemoved.AddListener(OnRemoveEffect);

        HUD.instance.SetStatHP(stat_hp);
        HUD.instance.SetStatSP(stat_sp);
        HUD.instance.SetStatMP(stat_mp);

        for (int i = 0; i < jumpTrails.Length; i++)
        { jumpTrails[i].enabled = false; }
        for (int i = 0; i < dashTrails.Length; i++)
        { dashTrails[i].enabled = false; }

        damageRelay.SaveColliderState("stand", damageRelay.Collider);
        intRelay.SaveColliderState("stand", intRelay.Collider);

        damageRelay.SaveColliderState("crouch", damageRelayCrouch.GetComponent<Collider2D>());
        intRelay.SaveColliderState("crouch", intRelayCrouch.GetComponent<Collider2D>());

        Room.onAnyRoomActivated.AddListener(OnRoomActivated);
    }


    float aspeed = 1f;
    bool roofCheck = false;

    string i_anim = "idle", r_anim = "run", b_anim = "break";
    float cmovemult = 1f, rspeedmult = 1f;

    protected void Update()
    {
        stat_sp.value += 10 * energyMult * Time.deltaTime;
        stat_mp.value += 20 * manaMult * Time.deltaTime;

        if (stat_mp.value < 50f)
        { stat_hp.value -= (1f - stat_mp.value / 50f) * 5f * Time.deltaTime; }

        dir = (Vector2)inputVector;

        SetOverlayPosition();

        if (isDashing) return;

        if (interact.down)
        { 
            foreach (var intact in interactsInRange)
            { intact.Interact(); } 
        }

        roofCheck = isGrounded && Physics2D.Raycast((Vector2)transform.position + crouchRect.position, Vector2.up, standRect.size.y - crouchRect.size.y / 2, groundMask);
        if (isGrounded && (dir.y < 0 || roofCheck)) 
        {
            if (!isCrouched)
            { SetCrouch(true); }
        }
        else
        {
            if (isCrouched)
            { SetCrouch(false); }
        }



        if (isCharging < 1 && attackState < 1 && cComboTimer <= 0)
        {
            if (!isCrouched && dash.down && (dir.x != 0 || dir.y != 0))
            {
                Vector2 dnorm = dir.normalized;
                for (int i = 0; i < dashTrails.Length; i++)
                {
                    stat_mp.value -= 20f;
                    if (stat_mp.value < 1f)
                    {
                        stat_hp.value -= 5f;
                    }
                    
                    dashTrails[i].xyzMult = new Vector3(-dnorm.y * 0.6f, dnorm.x * 0.6f, 0.1f); 
                }
                StartCoroutine(IDash(dnorm * 20f, 0.4f)); return; 
            }
            Move(dir.x *= (dir.x < 0 ? moveMultLeft : moveMultRight) * cmovemult * moveXMult); 
        }
        aspeed = 1f;
        if (wasGrounded < coyoteTime && !isJumping)
        {
            wgInit = false;
            if (jump.down && !roofCheck)
            {
                isCharging = 0;
                if (isPlatform && dir.y < 0)
                {
                    if (ignorePlatformCoroutine == null) ignorePlatformCoroutine = StartCoroutine(IIgnorePlatform(0.25f)); 
                }
                else
                { 
                    JumpInit(new Vector2(0, jumpForce), jumpForce, jumpHoldTime);
                    StartCoroutine(IWaitJumpRelease());
                }
            }

            if (isGrounded && isCharging < 1 && attackState < 1)
            {
                if (cComboTimer <= 0)
                {
                    if (dir.x > 0)
                    {
                        if (relativeVelocity.x < 0)
                        {
                            animator.SetState(b_anim);
                            animator.FlipX(true);
                        }
                        else
                        {
                            animator.SetState(r_anim);
                            animator.FlipX(false);
                            aspeed = Mathf.Clamp(Mathf.Abs(velocity.x) / rspeedmult, 0.5f, 2f);
                            facing = 1;
                        }
                    }
                    else if (dir.x < 0)
                    {
                        if (relativeVelocity.x > 0)
                        {
                            animator.SetState(b_anim);
                            animator.FlipX(false);
                        }
                        else
                        {
                            animator.SetState(r_anim);
                            animator.FlipX(true);
                            aspeed = Mathf.Clamp(Mathf.Abs(velocity.x) / rspeedmult, 0.5f, 2f);
                            facing = -1;
                        }
                    }
                    else
                    {
                        if (relativeVelocity.x > 0.5f)
                        {
                            animator.SetState(b_anim);
                            animator.FlipX(false);
                        }
                        else if (relativeVelocity.x < -0.5f)
                        {
                            animator.SetState(b_anim);
                            animator.FlipX(true);
                        }
                        else
                        {
                            animator.SetState(i_anim);
                        }
                    }
                }
                

                cmJumps = mJumps;

                if (attackA.hold && attackA.holdTime < 0.05f)
                { AttackCharge(); }

                if (attackA.down)
                {

                }
            }
        }
        else if (isCharging == 0 && attackState == 0) // != 1
        {
            if (attackA.hold && attackA.holdTime < 0.1f)
            {
                if (dir.y > 0)
                {
                    animator.SetState(meleeUpAir);
                    attackState = 2; goto AIR_OUT;
                    //StartCoroutine(IHoldAirAttack(dir)); goto AIR_OUT; 
                }
                else if (dir.y < 0)
                {
                    StartCoroutine(IHoldAirAttack(dir)); goto AIR_OUT;
                }

                goto AIR_OUT;
            }

            //isCharging = 0;
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
                    JumpInit(AngleToVector2(dir.x < 0 ? 90f : 60f) * jumpForce, jumpForce * 0.5f, jumpHoldTime);
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
                    JumpInit(AngleToVector2(dir.x > 0 ? 90f : 105f) * jumpForce, jumpForce * 0.5f, jumpHoldTime);
                    StartCoroutine(IWaitJumpRelease());
                }
            }
            else if (wallLeft)
            {
                if ((dir.x < 0 || wgInit) && velocity.y <= 0) {
                    wgInit = true;
                    animator.SetState("ledge_grab"); animator.FlipX(false);

                    WallFriction();
                    if (jump.down && !ledgeLeft)
                    {
                        animator.FlipX(false);
                        //Vector2 jforce = AngleToVector2(dir.x > 0 ? 45f : 75f) * jumpForce;
                        Vector2 jforce = AngleToVector2(45f) * (jumpForce * 1.25f);
                        //if (velocity.y > 0)
                        //{ jforce.y = Mathf.Min(velocity.y + jforce.y, jforce.y * 1.5f) - velocity.y; }

                        JumpInit(jforce, jumpForce * 0.5f, jumpHoldTime);
                        StartCoroutine(IWaitLeftWalljump(0.6f));
                        StartCoroutine(IWaitJumpRelease());
                    }
                } 
                else 
                { animator.SetState("air_ascend"); wgInit = false; }

            }
            else if (wallRight)
            {
                if ((dir.x > 0 || wgInit) && velocity.y <= 0) {
                    wgInit = true;
                    animator.SetState("ledge_grab"); animator.FlipX(true);

                    WallFriction();
                    if (jump.down && !ledgeRight)
                    {
                        animator.FlipX(true);
                        //Vector2 jforce = AngleToVector2(dir.x < 0 ? 135 : 105f) * jumpForce;
                        Vector2 jforce = AngleToVector2(135f) * (jumpForce * 1.25f);
                        //if (velocity.y > 0)
                        //{ jforce.y = Mathf.Min(velocity.y + jforce.y, jforce.y * 1.5f) - velocity.y; }

                        JumpInit(jforce, jumpForce * 0.5f);
                        StartCoroutine(IWaitRightWalljump(0.6f));
                        StartCoroutine(IWaitJumpRelease());
                    }
                }
                else
                { animator.SetState("air_ascend"); wgInit = false; }

            }
            else if(jump.down && dir.y > 0 && cmJumps > 0 && stat_sp.TryConsume(25f))// && _velocity.y <= 0 && _velocity.y > -10f)
            {
                wgInit = false;
                cmJumps--;
                swJumpAir.Activate();
                _velocity.y = 0;
                _velocity.x = Mathf.Clamp(_velocity.x * 0.5f, -4f, 4f);
                StartCoroutine(IWaitMoveX(0.5f));
                JumpInit(new Vector2(0, airJumpForce * 2f), 0f, 0f);//airJumpForce, jumpHoldTime); 
                StartCoroutine(IWaitJumpRelease());
                StartCoroutine(IWaitJumpTrail(0.5f));
            }
            else
            {
                wgInit = false;
                if (dir.x > 0) animator.FlipX(false);
                else if (dir.x < 0) animator.FlipX(true);
                if (dir.y < 0)
                { if (ignorePlatformCoroutine == null) ignorePlatformCoroutine = StartCoroutine(IIgnorePlatform(0.05f)); }
                aspeed = Mathf.Clamp(velocity.y * 0.5f, 0.5f, 2f);
                if (velocity.y > 2f) animator.SetState("air_ascend");
                else if (velocity.y < -2f) animator.SetState("air_descend");
                else animator.SetState("air_float");
            }
        }
        AIR_OUT:;
        animator.animSpeed = aspeed;
    }

    void SetOverlayPosition()
    {
        Vector2 vec = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 vecpos = new Vector2(vec.x / Screen.currentResolution.width, vec.y / Screen.currentResolution.height);
        HUD.instance.SetOverlayPosition(new Vector2(vec.x / Screen.currentResolution.width, vec.y / Screen.currentResolution.height));
        //HUD.instance.SetOverlayPosition(new Vector2(vec.x / 380f, vec.y / 160f));
    }


    void SetCrouch(bool value)
    {
        if (value)
        {
            SetColliderRect(crouchRect);

            damageRelay.LoadColliderState("crouch");
            intRelay.LoadColliderState("crouch");

            playerVision.transform.localPosition = new Vector3(0, -0.6f,0);
            playerLight.transform.localPosition = new Vector3(0, -0.6f, 0);

            isCrouched = true;
            i_anim = "crouch";
            b_anim = "crouch";
            r_anim = "crawl";
            cmovemult = 0.5f;
            rspeedmult = 1f;

            return;
        }

        SetColliderRect(standRect);

        damageRelay.LoadColliderState("stand");
        intRelay.LoadColliderState("stand");

        playerVision.transform.localPosition = new Vector3(0, 0.45f, 0);
        playerLight.transform.localPosition = new Vector3(0, 0, 0);

        isCrouched = false;
        i_anim = "idle";
        b_anim = "break";
        r_anim = "run";
        cmovemult = 1f;
        rspeedmult = anim_run_speed;
    }


    void WallFriction()
    {
        tempGravityMult = 0f;
        _velocity.y = Mathf.Max(TowardsTargetValue(_velocity.y, -2f, -Mathf.Min(velocity.y, gravity) * Time.deltaTime), -2f); // Mathf.Min(_velocity.y - Mathf.Min(_velocity.y * 8f, -gravity) * Time.deltaTime, 0);
    }


    Coroutine ignorePlatformCoroutine;
    IEnumerator IIgnorePlatform(float time = 0.1f)
    {
        ignorePlatform = true;
        float t = 0;
        while (t < time || dir.y < 0)
        {
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        ignorePlatform = false;
        ignorePlatformCoroutine = null;
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
    IEnumerator IWaitMoveX(float time = 0.5f)
    {
        float t = 0;
        while (t < time || !wallLeft)
        {
            t += Time.deltaTime;
            moveXMult = Mathf.Lerp(0f, 1f, t / time);
            yield return new WaitForEndOfFrame();
        }
        moveXMult = 1f;
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

    IEnumerator IWaitJumpTrail(float time = 0.5f)
    {
        for (int i = 0; i < jumpTrails.Length; i++)
        { jumpTrails[i].enabled = true; }// jumpTrailRend[i].emitting = true; }
        yield return new WaitForSeconds(time);
        for (int i = 0; i < jumpTrails.Length; i++)
        { jumpTrails[i].enabled = false; }//jumpTrailRend[i].emitting = false; }
    }

    IEnumerator IHaltLedgeGrab(float time)
    {
        if (!ledgegrabEnabled) yield break;
        ledgegrabEnabled = false;
        yield return new WaitForSeconds(time);
        ledgegrabEnabled = true;
    }

    void DamageSlowdown(float pause_t = 0.2f, float fade_t = 0.25f, float invincduration = 1f)
    {
        StartCoroutine(IDamageFlash(Color.white, 4));
        
        TimeControl.SetTimeScaleFadeForTime(0.05f, pause_t, 0f, fade_t, 1f);
    }

    public Color pColor { get => mat.GetColor("_Color"); set => mat.SetColor("_Color", value); }
    public float pColorLerp { get => mat.GetFloat("_ColorLerp"); set => mat.SetFloat("_ColorLerp", value); }
    public float pColorTransparency { get => mat.GetFloat("_Transparency"); set => mat.SetFloat("_Transparency", value); }

    IEnumerator IDamageFlash(Color color, int frames)
    {
        Color orig_color = pColor;
        float orig_transp = pColorTransparency,
            orig_clerp = pColorLerp;

        pColor = color;
        pColorLerp = 1f;
        pColorTransparency = 0.5f;
        int f = 0;
        while (f < frames)
        { yield return new WaitForEndOfFrame(); }
        pColor = orig_color;
        pColorTransparency = orig_transp;
        pColorLerp = orig_clerp;
    }
    IEnumerator IDamageFlash(Color color, float time)
    {
        Color orig_color = pColor;
        float orig_transp = pColorTransparency, 
            orig_clerp = pColorLerp;

        pColor = color;
        pColorLerp = 1f;
        pColorTransparency = 0.5f;
        yield return new WaitForSeconds(time);
        pColor = orig_color;
        pColorTransparency = orig_transp;
        pColorLerp = orig_clerp;
    }



    bool isDashing = false;
    IEnumerator IDash(Vector2 dash_vel, float time = 0.5f)
    {
        isDashing = true;
        
        float t = 0, d;
        _velocity.y = 0;
        pColor = Color.black;
        pColorLerp = 1f;
        bool d_released = false;
        float t_rate = 1f, t_delta = 0f;
        //bool dreleased = false;
        //pColorTransparency = 0.25f;
        damageRelay.gameObject.SetActive(false);
        for (int i = 0; i < dashTrails.Length; i++)
        { dashTrails[i].enabled = true; }
        while (t < time)
        {
            if (!dash.hold)
            { d_released = true; }
            if (d_released && t_delta < 1f)
            { t_rate = Mathf.Lerp(1f, 3f, Mathf.Max(t_delta += Time.deltaTime * 4f, 1f)); }
            tempGravityMult = 0;
            t += Time.fixedDeltaTime * t_rate;
            d = t / time;
            pColorTransparency = 1f - Mathf.Sin(Mathf.Lerp(0f, 180f, d) * Mathf.Deg2Rad);
            _velocity = dash_vel * Mathf.Sin(Mathf.LerpAngle(90f, 15f, d) * Mathf.Deg2Rad);
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < dashTrails.Length; i++)
        { dashTrails[i].enabled = false; }
        damageRelay.gameObject.SetActive(true);
        pColorLerp = 0;
        pColorTransparency = 1f;
        isDashing = false;
    }

    float damage_treshold = 5f;
    float manaMult = 1f, energyMult = 1f;
    Coroutine manaMultCoroutine, energyMultCoroutine;

    public void OnAddEffect(StatusEffect effect)
    {
        Debug.Log($"Effect added {effect.name}");
        if (damageRelay.colliderState != 0 && effect.flags.Contains("invulnerable"))
        {

            pColor = Color.black;
            pColorLerp = 0.5f;
            pColorTransparency = 0.5f;
            damageRelay.colliderState = RelayCollider.CollState.Disabled;
            if (effect.GetType() == typeof(TimedStatusEffect))
            {
                DamageSlowdown(0.1f, 0.25f, ((TimedStatusEffect)effect).duration);
                //StartCoroutine(IDamageFlash(Color.white, ((TimedStatusEffect)effect).duration)); 
            }
        }
    }
    public void OnRemoveEffect(StatusEffect effect)
    {
        Debug.Log($"Effect removed {effect.name}");
        if (damageRelay.colliderState == 0 && !entityStats.GetFlag("invulnerable"))
        {
            damageRelay.colliderState = RelayCollider.CollState.Enabled;

            pColor = Color.black;
            pColorLerp = 0f;
            pColorTransparency = 1f;
        }
    }


    public void OnHealthMaxChanged(float new_value)
    {
        damage_treshold = stat_hp.max * 0.05f;
    }
    public void OnHealthChanged(float new_value, float old_value)
    { 
        if (old_value - new_value > damage_treshold)
        {
            StatusEffect seff = new TimedStatusEffect("invulnerable", 2f);
            seff.flags.Add("invulnerable");
            entityStats.AddEffect(seff, this);
        }
        if (new_value <= 0) DIEEE();
    }
    public void OnEnergyChanged(float new_value, float old_value)
    {
        if (new_value < old_value)
        {
            if (energyMultCoroutine != null) StopCoroutine(energyMultCoroutine);
            energyMultCoroutine = StartCoroutine(IWaitEnergy(0f, 1f, 1f, 2f));
        }
    }
    public void OnManaChanged(float new_value, float old_value)
    {
        if (new_value < old_value)
        {
            if (manaMultCoroutine != null) StopCoroutine(manaMultCoroutine);
            manaMultCoroutine = StartCoroutine(IWaitMana(0f, 1f, 1f, 2f));
        }
    }


    IEnumerator IWaitMana(float mult0 = 0f, float mult1 = 1f, float multEnd = 1f, float time = 2f)
    {
        float t = 0, d = 0;
        while (t < time)
        { 
            t += Time.deltaTime;
            d = t / time;
            manaMult = Mathf.Lerp(mult0, mult1, d);
            yield return new WaitForEndOfFrame();
        }
        manaMult = multEnd;
    }
    IEnumerator IWaitEnergy(float mult0 = 0f, float mult1 = 1f, float multEnd = 1f, float time = 2f)
    {
        float t = 0, d = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            d = t / time;
            energyMult = Mathf.Lerp(mult0, mult1, d);
            yield return new WaitForEndOfFrame();
        }
        energyMult = multEnd;
    }


    protected override void OnEnterGrounded(Vector2 velocity, float fallDamageDelta)
    {
        base.OnEnterGrounded(velocity, fallDamageDelta);
        if (fallDamageDelta > 0.1f)
        {
            float fdelta = Mathf.Clamp(fallDamageDelta*2, 0f, 1f);
            CameraControl.instance.Nudge(Vector2.down * 0.5f, 0.05f, 0.2f, 2);
            //DamageSlowdown(fdelta * 1f, fdelta * 0.5f);
            _velocity.y = -velocity.y* EaseInOutCirc01( fdelta * 0.2f);
        }
        asourceFeet.PlayOneShot(soundStep, 0.75f * Mathf.Clamp01(Mathf.Abs(velocity.y) / 50));
    }
    protected override void OnTouchWallLeft(Vector2 velocity)
    {
        base.OnTouchWallLeft(velocity);
    }
    protected override void OnTouchWallRight(Vector2 velocity)
    {
        base.OnTouchWallRight(velocity);
    }

    
    public void OnStep()
    {
        asourceFeet.PlayOneShot(soundStep, 0.1f);
    }
    

    

    void OnAnimEnd()
    {
        if (isCharging > 0)
        {
            Debug.Log($"isCharging before: {isCharging}");
            isCharging = (isCharging+1) % 3;
            Debug.Log($"isCharging after: {isCharging}");
        }
        if (attackState > 0)
        {
            attackState = (attackState + 1) % 3;
        }
    }



    void OnwgrabChanged(float value, float lvalue)
    { }
    void OndjumpChanged(float value, float lvalue)
    {
        Debug.Log($"djump {value} {lvalue}");
        mJumps = Mathf.RoundToInt(value); 
    }


    void DIEEE()
    {
        swDeath.Activate();
        StartCoroutine(IDeathFade(0.1f, 0.2f));
        GameManager.Reset_Game_Fade();
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
            yield return new WaitForEndOfFrame();
        }
        t = 0;
        while (t < t_b)
        {
            d = t / t_b;
            t += Time.unscaledDeltaTime;
            mat.SetFloat("_Transparency", 1f - d);
            yield return new WaitForEndOfFrame();
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.gameObject.tag)
        {
            //case "Hazard":
                //ApplyDamage(new EntityStats.Damage(999), this);
                //DIEEE();
                //break;
            case "Checkpoint":
                if (collision.gameObject.TryGetComponent<Checkpoint>(out var cp))
                { cp.SetActiveCheckpoint(); }
                break;
            case "Collectable":
                if (collision.gameObject.TryGetComponent<CollectRelay>(out var cr))
                { cr.Collect(this); }
                break;
        }
    }


    List<Interactable> interactsInRange = new();

    public void OnInteractTriggerEnter(RelayCollider relay, Collider2D coll)
    {
        if (coll.tag == "Interactive")
        {
            if (coll.gameObject.TryGetComponent(out Interactable mono))
            { 
                mono.SetUIState(true);
                interactsInRange.Add(mono);
            }
        }
    }
    public void OnInteractTriggerExit(RelayCollider relay, Collider2D coll)
    {
        if (coll.tag == "Interactive")
        {
            if (coll.gameObject.TryGetComponent(out Interactable mono))
            { 
                mono.SetUIState(false);
                interactsInRange.Remove(mono);
            }
        }
    }
    void damageTest()
    {
        ApplyDamage(new Damage(999), this);
    }
    public void OnDamageTriggerEnter(RelayCollider relay, Collider2D coll)
    {
        //Debug.Log($"entityStats: {entityStats != null}");

        switch (coll.gameObject.tag)
        {
            case "Hazard":
                damageTest();
                // ApplyDamage(new EntityStats.Damage(999), this);
                break;
        }
    }
    public void OnDamageTriggerExit(RelayCollider relay, Collider2D coll)
    {  }


    public void OnRoomActivated(Room active_room, Room last_active_room) 
    {
        if (active_room.isDarkRoom)
        {
            playerLight.intensity = 0.75f;
            playerLight.pointLightInnerRadius = 0f;
            playerLight.pointLightOuterRadius = 10f;
            playerLight.falloffIntensity = 0.5f;
            playerLight.shadowIntensity = 0.9f;
        }
        else
        {
            playerLight.intensity = 0.5f;
            playerLight.pointLightInnerRadius = 0f;
            playerLight.pointLightOuterRadius = 2f;
            playerLight.falloffIntensity = 0.75f;
            playerLight.shadowIntensity = 0.5f;
        }
    }


}
