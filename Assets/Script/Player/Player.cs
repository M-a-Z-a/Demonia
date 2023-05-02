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
    InputKey left, right, up, down, jump, attackA, attackB, interact;
    float moveMultLeft = 1f, moveMultRight = 1f;
    float wallYVel = 0f;
    int facing = 1;

    EntityStats.Attribute coyoteTime, jumpForce, airJumpForce;
    int mJumps = 1, cmJumps = 0;

    int cComboStage = 0; 
    float cComboTimer = 0, cComboLastAttackDelay = 0;
    float wallSlow = 1f;

    float anim_run_speed = 32f / 16f * 0.3f * 10f;

    Rect rect_stand = new Rect(new Vector2(0,0), new Vector2(0.8f, 1.8f));
    Rect rect_crouch = new Rect(new Vector2(0, -0.45f), new Vector2(0.8f, 0.9f));

    Shockwave swJumpAir, swDeath;
    Vector2 dir;

    float jumpHoldTime = 0.4f;
    float chargeAmount = 0f;
    int isCharging = 0, attackState = 0;

    //string curAttackState = "";
    int curAttack = 0;

    EntityStats.Stat stat_hp;

    [SerializeField]
    AudioSource asource, asourceFeet;
    [SerializeField]
    AudioClip soundStep, soundChargeup, soundSlash, soundAxeExpl;

    [SerializeField]
    ProjClass projectiles;

    string meleeChargeAnim = "melee.g.n.c.release", 
        meleeUpChargeAnim = "melee.g.u.c.release",
        meleeDownChargeAnim = "melee.g.d.c.release";

    string meleeDownAirCharge = "melee.a.d.init",
        meleeDownAirRelease = "melee.a.d.release",
        meleeDownAirGround = "melee.a.d.ground";

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
        cameraTarget = transform.Find("CameraTarget");

        rend = GetComponentInChildren<MeshRenderer>();
        mat = rend.material;

        animator = GetComponent<MatAnimator>();

        entityStats.SetAttribute("speed", 8f);
        entityStats.SetAttribute("accelSpeed", 40f);
        entityStats.SetAttribute("decelSpeed", 15f);
        coyoteTime = entityStats.GetSetAttribute("coyoteTime", 0.1f);
        jumpForce = entityStats.GetSetAttribute("jumpforce", 10f);
        airJumpForce = entityStats.GetSetAttribute("airjumpforce", 6f);

        asource.volume = 0.1f;
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
        interact = InputManager.SetInputKey("interact", KeyCode.A);

        animator.FetchAnimationData(atlas_path: "Data/player_atlas");
        animator.flagActions.Add("step", OnStep);
        animator.flagActions.Add("attack", OnAttack);
        animator.flagActions.Add("charging", OnCharge);
        animator.onAnimEnd.Add(OnAnimEnd);

        //GetMeleeHitboxes();

        Transform effs = transform.Find("Effects");
        swDeath = effs.Find("ShockwaveDeath").GetComponent<Shockwave>();
        swDeath.Deactivate();
        swJumpAir = effs.Find("ShockwaveAirJump").GetComponent<Shockwave>();
        swJumpAir.Deactivate();
        gravityMultiplier = 1.25f;

        stat_hp = entityStats.SetStat("health", 100, 100);
        stat_hp.AddListener(OnHealthChanged);
        HUD.instance.SetStatHP(stat_hp);

        //Debug.Log($"stat_hp: {entityStats.GetAttribute("health").value}");

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

        dir = (Vector2)inputVector;

        if (interact.down)
        { 
            foreach (var intact in interactsInRange)
            { intact.Interact(); } 
        }

        if (isCharging < 1 && attackState < 1 && cComboTimer <= 0) Move(dir.x *= dir.x < 0 ? moveMultLeft : moveMultRight);
        aspeed = 1f;
        if (wasGrounded < coyoteTime && !isJumping)
        {
            if (jump.down)
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
                }
                

                cmJumps = mJumps;

                if (attackA.hold && attackA.holdTime < 0.05f)
                { AttackCharge(); }

                if (attackA.down)
                {
                    /*
                    if ()
                    {
                        
                        if (cComboTimer > 0)
                        {
                            if (cComboTimer < cComboLastAttackDelay)
                            {
                                Attack(dir);
                                cComboStage++;
                                Debug.Log($"!Combo+ [{cComboStage}]");
                                cComboTimer = 0.5f;
                                cComboLastAttackDelay = cComboTimer - 0.2f;

                                if (dir.x > 0) facing = 1;
                                else if (dir.x < 0) facing = -1;
                            }
                        }
                        else
                        {
                            Attack(dir);
                            cComboStage = 1;
                            Debug.Log($"!Combo Init [{cComboStage}]");
                            cComboTimer = 0.5f;
                            cComboLastAttackDelay = cComboTimer - 0.2f;
                            StartCoroutine(IComboTimer(cComboTimer));

                            if (dir.x > 0)
                            { facing = 1; animator.FlipX(false); }
                            else if (dir.x < 0)
                            { facing = -1; animator.FlipX(true); }
                        }
                        
                    }
                    */
                }
                /*
                else if (isCharging == 0 && attackA.holdTime > 0.2f)
                {
                    if (dir.x > 0)
                    { facing = 1; animator.FlipX(false); }
                    else if (dir.x < 0)
                    { facing = -1; animator.FlipX(true); }

                    cComboTimer = 0;
                    AttackCharge(dir);
                }
                */
            }
        }
        else if (isCharging == 0 && attackState == 0) // != 1
        {
            if (dir.y < 0 && attackA.hold)
            { StartCoroutine(IHoldAirAttack(dir)); goto AIR_OUT; }

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
            else if (wallLeft)
            {
                //_velocity.y = Mathf.Max(-0.5f, _velocity.y);
                if (velocity.y <= 0) { 
                    animator.SetState("ledge_grab"); animator.FlipX(false); 
                } 
                else 
                { animator.SetState("air_ascend"); }

                if (_velocity.y < 0)
                { WallFriction(); }
                if (jump.down && !ledgeLeft)
                {
                    animator.FlipX(false);
                    Vector2 jforce = AngleToVector2(dir.x > 0 ? 75f : 45f) * jumpForce;
                    if (velocity.y > 0)
                    { jforce.y = Mathf.Min(velocity.y + jforce.y, jforce.y * 1.5f) - velocity.y; }
                    JumpInit(jforce, jumpForce * 0.5f, jumpHoldTime);
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
                    JumpInit(AngleToVector2(dir.x > 0 ? 90f : 105f) * jumpForce, jumpForce * 0.5f, jumpHoldTime);
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
                { WallFriction(); }
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
                swJumpAir.Activate();
                JumpInit(new Vector2(0, airJumpForce), airJumpForce, jumpHoldTime); StartCoroutine(IWaitJumpRelease());
            }
            else
            {
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

    void WallFriction()
    {
        _velocity.y = TowardsTargetValue(_velocity.y, 0, -currentGravity * 1.30f * Time.deltaTime); //TowardsTargetValue(_velocity.y, 0, -(dir.y < 0 ? currentGravity * 0.5f : (dir.x > 0 ? currentGravity * 1.2f : currentGravity * 0.8f)) * Time.deltaTime);
    }

    bool AttackCharge()
    {
        if (isCharging > 0) return true;
        float stime = 0.1f;
        string anim;
        Vector2 att_dir;
        if (up.hold && up.holdTime < stime)
        {
            anim = "melee.g.u.c";
            //animator.SetState("melee.g.u.c");
            att_dir = new Vector2(dir.x != 0 ? dir.x : facing, 1); 
        }
        else if (down.hold && down.holdTime < stime)
        {
            anim = "melee.g.d.c";
            //animator.SetState("melee.g.d.c");
            att_dir = new Vector2(dir.x != 0 ? dir.x : facing, -1); 
        }
        else if (left.hold && left.holdTime < stime)
        {
            anim = "melee.g.n.c";
            //animator.SetState("melee.g.n.c");
            facing = -1; animator.FlipX(true); 
            att_dir = new Vector2(-1, 0); 
        }
        else if (right.hold && right.holdTime < stime)
        {
            anim = "melee.g.n.c";
            //animator.SetState("melee.g.n.c");
            facing = 1; animator.FlipX(false); 
            att_dir = new Vector2(1, 0); 
        }
        else { return false; }

        isCharging = Mathf.Max(1, isCharging);

        //facing = animator.flipX ? -1 : 1;
        curAttack = 0;
        Debug.Log($"charge init");
        //if (chargeCoroutine == null) chargeCoroutine = StartCoroutine(IChargeAttack());
        StartCoroutine(IChargeAttack(anim, att_dir));
        return true;


    }
    void AttackChargeRelease(Vector2 vec)
    {
        if (isCharging < 1) { chargeAmount = 0; return; }
        //isCharging = 1;
        if (vec.y > 0)
        { Attack_Charge_Up(); }
        else if (vec.y < 0)
        { Attack_Charge_Down(); }
        else
        { Attack_Charge_Neutral(); }
        chargeAmount = 0;
    }

    void Attack(Vector2 vec)
    {
        if (vec.y > 0) { Attack_Up(); return; }
        if (vec.y < 0) { Attack_Down(); return; }
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

    GameObject SpawnProjectile(GameObject prefab, Vector3 position, Vector2 direction, bool flipX)
    {
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        Projectile proj = go.GetComponent<Projectile>();
        proj.onHitActions.Add(ProjectileHit);
        proj.flip = flipX;
        Vector3 eul = go.transform.eulerAngles;
        eul.z = Vector2.SignedAngle(Vector2.right, direction);
        go.transform.eulerAngles = eul;
        go.SetActive(true);
        return go;
    }

    public void ProjectileHit(Projectile p, Collider2D coll)
    {
        //Debug.Log($"proj_hit {p.name} {coll.name}");
        if (coll.tag != "Player")
        {
            if (coll.TryGetComponent(out Entity ent))
            {
                Debug.Log($"{ent.name}");
                if (p.projectileTag == "ukick1")
                { ent.velocity = Vector2.zero; }
                float kbmult = 0.5f + p.timeToLive / p.lifeTime * 0.6f;
                ent.AddForce(p.knockback * kbmult);
                
                if (p.timeToLive > p.lifeTime - 0.1f)
                { TimeControl.SetTimeScaleFadeForTime(0.1f, 0.2f, 0f, 0f, 1f); }
            }
        }
    }

    void Attack_Charge_Up()
    {
        animator.SetState(meleeUpChargeAnim);
    }
    void Attack_Charge_Down()
    {
        animator.SetState(meleeDownChargeAnim);
    }
    void Attack_Charge_Neutral()
    {
        animator.SetState(meleeChargeAnim);
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
            yield return new WaitForEndOfFrame();
        }
        cComboStage = 0;
        cComboTimer = 0;
        Debug.Log($"!Combo End");
    }

    //Coroutine chargeCoroutine;
    IEnumerator IChargeAttack(string anim, Vector2 att_dir)
    {

        isCharging = Mathf.Max(1, isCharging);
        animator.SetState(anim);
        chargeAmount = 0f;
        //int l_int = 0;
        //animator.SetState("charge.n");
        float stimer = 9f, prate, cdelta;
        while ((attackA.hold && isCharging > 1) || isCharging == 1)
        {
            stimer += Time.deltaTime;
            cdelta = chargeAmount / 3f;
            prate = 0.25f + ((1f - cdelta) * 0.75f);
            if (stimer >= prate)
            {
                Instantiate(projectiles.chargeShockwave, transform.position, Quaternion.identity);
                asource.pitch = 1f + cdelta * 1f;
                asource.PlayOneShot(soundChargeup, 0.5f);
                stimer = 0; 
            }
            chargeAmount = Mathf.Min(chargeAmount + Time.deltaTime, 3f);
            yield return new WaitForEndOfFrame();
        }
        AttackChargeRelease(att_dir);
        //yield return new WaitForSeconds(1f);
        //isCharging = 0;
        //chargeCoroutine = null;
    }

    IEnumerator IHoldAirAttack(Vector2 att_dir)
    {
        _velocity.y = 10f;
        animator.SetState(meleeDownAirCharge);
        attackState = 1;
        string att_name = meleeDownAirRelease;
        while ((attackState > 0 && attackA.hold) || attackState == 1)
        { 
            if (isGrounded)
            {
                if (attackState == 1)
                { attackState = 0; yield break; }
                att_name = meleeDownAirGround; break; 
            }
            yield return new WaitForEndOfFrame(); 
        }
        animator.SetState(att_name);
    }


    public void OnHealthChanged(float new_value, float old_value)
    { 
        if (new_value <= 0) DIEEE();
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
    public void OnCharge()
    {
        //if (isCharging == 1) isCharging = 2;
    }

    public void OnAttack()
    {
        asource.pitch = 1f;
        Vector2 pdir, knockback; bool flip;
        GameObject go;
        Projectile proj;
        Vector3 ppos = transform.position;
        facing = !animator.flipX ? 1 : -1;
        curAttack++;
        string canim = animator.currentAnimation.ID;

        if (canim == meleeDownAirRelease)
        {
            //_velocity.y = 10f;
        }
        else if (canim == meleeDownAirGround)
        {
            if (facing > 0)
            { pdir = AngleToVector2(80f); flip = false; }
            else
            { pdir = AngleToVector2(-80f); flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeDown, ppos.Add(x: 0.4f * facing, y: -0.15f), Vector2.right, flip);//.transform.parent = transform;
            knockback = AngleToVector2(45f) * 20f;
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"adkick{curAttack}";
            asourceFeet.PlayOneShot(soundAxeExpl, 0.1f);
        }
        else if (canim == meleeUpChargeAnim)
        {
            if (facing > 0)
            { pdir = AngleToVector2(80f); flip = false; }
            else
            { pdir = AngleToVector2(-80f); flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeUp, ppos.Add(x: 0f * facing, y: 0f), pdir, flip);//.transform.parent = transform;
            knockback = AngleToVector2(75f) * (curAttack == 1 ? 10f : 20f);
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"ukick{curAttack}";
            asource.clip = soundSlash;
            asource.Play();
            _velocity = new Vector2(facing * 2f + dir.x * 1f, 8f);
        }
        else if (canim == meleeChargeAnim)
        {
            if (curAttack == 2)
            {
                _velocity = new Vector2(6f * -facing, 2f);
                return;
            }
            if (facing > 0)
            { pdir = Vector2.right; flip = false; }
            else
            { pdir = Vector2.left; flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeForward, ppos.Add(x: 0.4f * facing, y: 0.1f), Vector2.right, flip);//.transform.parent = transform;
            knockback = AngleToVector2(30f) * 20f;
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"nkick{curAttack}";
            asource.clip = soundSlash;
            asource.Play();
            if (dir.x != 0)
            { _velocity = new Vector2(6f * dir.x, 2f); }
            else
            { _velocity = new Vector2(4f * facing, 2f); }
        } 
        else if (canim == meleeDownChargeAnim)
        {
            if (facing > 0)
            { pdir = AngleToVector2(80f); flip = false; }
            else
            { pdir = AngleToVector2(-80f); flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeDown, ppos.Add(x: 0.4f * facing, y: -0.15f), Vector2.right, flip);//.transform.parent = transform;
            knockback = AngleToVector2(45f) * 20f;
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"dkick{curAttack}";
            asourceFeet.PlayOneShot(soundAxeExpl, 0.1f);
            //asource.clip = soundAxeExpl;
            //asource.Play();
            //_velocity = new Vector2(facing * 2f + dir.x * 1f, 8f);
        }
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
                if (collision.gameObject.TryGetComponent<Checkpoint>(out Checkpoint c))
                { c.SetActiveCheckpoint(); }
                break;
        }
    }


    List<Interactable> interactsInRange = new();

    public void OnInteractTriggerEnter(Collider2D coll)
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
    public void OnInteractTriggerExit(Collider2D coll)
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
        ApplyDamage(new EntityStats.Damage(999), this);
    }
    public void OnDamageTriggerEnter(Collider2D coll)
    {
        Debug.Log($"entityStats: {entityStats != null}");

        switch (coll.gameObject.tag)
        {
            case "Hazard":
                damageTest();
                // ApplyDamage(new EntityStats.Damage(999), this);
                break;
        }
    }
    public void OnDamageTriggerExit(Collider2D coll)
    {  }


}
