
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AI_Boss : EntityController
{
    public static AI_Boss activeBoss { get; protected set; }

    public enum StateEnum { Idle, Attack, AttackTp, Dash, Missile, Railgun, Throw, Stomp, LaserBeam, LaserArray }

    List<StateEnum> statePool = new() { StateEnum.AttackTp, StateEnum.Dash, StateEnum.Missile, StateEnum.LaserArray, StateEnum.Throw, StateEnum.Railgun },
        statePoolFloor = new() { StateEnum.Attack, StateEnum.AttackTp, StateEnum.Dash, StateEnum.Missile, StateEnum.LaserArray, StateEnum.Throw, StateEnum.Railgun },
        statePoolNear = new() { StateEnum.AttackTp, StateEnum.Throw, StateEnum.Stomp },
        statePoolNearFloor = new() { StateEnum.Attack, StateEnum.Dash, StateEnum.Stomp },
        statePoolFar = new() { StateEnum.Dash, StateEnum.AttackTp, StateEnum.Missile, StateEnum.LaserArray, StateEnum.Throw, StateEnum.Railgun },
        statePoolFarFloor = new() { StateEnum.Dash, StateEnum.AttackTp, StateEnum.Missile, StateEnum.LaserArray, StateEnum.Throw, StateEnum.Railgun };

    List<ProjectileControlled> missiles = new(), laserplatforms = new();

    List<StateEnum> stateQue = new();
    float _rageLevel = 0f;
    public float rageLevel { get => GetRageLevel(); }
    float GetRageLevel()
    {
        _rageLevel = (1f - stat_hp.delta) * 0.5f + (target_stat_hp.delta - stat_hp.delta) * 0.5f;
        return _rageLevel;
    }

    StateEnum GetRandomState(List<StateEnum> pool)
    { return pool[UnityEngine.Random.Range(0, pool.Count)]; }
    IEnumerator GetStateEnum(StateEnum state)
    {
        //Debug.Log("GetSetEnum()");
        switch (state)
        {
            case StateEnum.Idle: return IState_Idle();
            case StateEnum.Attack: return IState_Attack();
            case StateEnum.AttackTp: return IState_AttackTp();
            case StateEnum.Dash: return IState_Dash();
            case StateEnum.Missile: return IState_Missiles();
            case StateEnum.Throw: return IState_Throw();
            case StateEnum.Stomp: return IState_Stomp();
            case StateEnum.Railgun: return IState_Railgun();
            case StateEnum.LaserArray: return IState_LaserArray();
            default: return IState_Idle();
        }
    }

    

    float _startTime, idle_time = 2f;
    public float TimePassed { get => Time.time - _startTime; }

    Transform target;

    Coroutine stateCoroutine;
    bool stopState = false, forceIdle = false;

    int _facing = 1;
    public int facing { get => _facing; protected set => Set_facing(value); }
    public bool facingBool { get => _facing >= 0; protected set => Set_facing(value); }

    Vector2 tmove = Vector2.zero;
    bool stopGrav = false;

    int animState = 0;

    EntityStats.Stat stat_hp, target_stat_hp;

    MatAnimator animator;

    [SerializeField] RelayCollider dashRelay;
    [System.Serializable] public class BossProjectiles { public GameObject meleeA, stomp, sword_v, sword_h, missile, railshot, laserPlatform_up, laserPlatform_down; }
    [SerializeField] BossProjectiles projectiles;

    void Set_facing(bool isRight, bool isInstant = false)
    { 
        _facing = isRight ? 1 : -1;
        if (isInstant)
        { animator.FlipX(isRight); return; }
        animator.flipX = !isRight;
    }
    void Set_facing(int f, bool isInstant = false)
    {
        if (f > 0) { _facing = 1; }
        else if (f < 0) { _facing = -1; }
        if (isInstant)
        { animator.FlipX(_facing < 0); return; }
        animator.flipX = _facing < 0;
    }

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<MatAnimator>();
    }

    protected override void Start()
    {
        base.Start();
        target = Player.pTransform;

        stat_hp = entityStats.SetStat("health", 1000f, 1000f);
        stat_hp.onValueChanged.AddListener(OnHealthChanged);

        ignorePlatform = true;

        entityStats.SetAttribute("forceResist", 50f);

        animator.FetchAnimationData();
        //animator.FetchAnimationData(atlas_path: "Data/boss_robotA_atlas");
        //animator.flagActions.Add("step", OnStep);
        //animator.flagActions.Add("attack", OnAttack);
        //animator.flagActions.Add("charging", OnCharge);
        animator.onAnimEnd.Add(OnAnimEnd);
        animator.flagActions.Add("faceTarget", TurnTowardsTarget);
        animator.flagActions.Add("throwA1", Action_ThrowA1);
        animator.flagActions.Add("throwA2", Action_ThrowA2);
        animator.flagActions.Add("attackA", Action_AttackA);
        animator.flagActions.Add("stomp", Action_Stomp);
        //animator.onAnimEnd.Add(OnAnimEnd);

        dashRelay.gameObject.SetActive(false);

    }

    protected void OnEnable()
    {
        activeBoss = this;
        idle_time = 2f;
        StartCoroutine(ISetBossHpBar());
        animator.SetState("idle", 0);
    }
    protected void OnDisable()
    { 
        if (activeBoss == this) activeBoss = null; 
    }

    IEnumerator ISetBossHpBar(int wait_frames = 1, float wait_time = 0, bool is_realtime = false)
    {
        for (int f = 0; f < wait_frames; f++)
        { yield return new WaitForEndOfFrame(); }
        if (wait_time > 0)
        {
            if (is_realtime) yield return new WaitForSecondsRealtime(wait_time);
            else yield return new WaitForSeconds(wait_time);
        }
        HUD.instance.SetBossHP(stat_hp);
        HUD.instance.EnableBossHpBar(true);
    }

    protected override void FixedUpdate()
    {
        if (isGrounded)
        {
            _velocity.x = tmove.x;
        }
        else
        {
            if (!stopGrav)
            {
                _velocity.x = tmove.x;
                _velocity.y += tmove.y + gravity * Time.deltaTime;
            }
            else
            {
                _velocity = tmove;
            }
        }

        base.FixedUpdate();
        tmove = Vector2.zero;
    }


    private void Update()
    {
        UpdateLoop();
        if (missiles.Count > 0)
        {
            float movef;
            foreach (ProjectileControlled missile in missiles)
            {
                missile.UpdateLifetime();
                missile.RotateTowards(target.position, (missile.TimeToLive / 3f * 135f + 45f) * Time.deltaTime, out movef);
                missile.MoveForward(10f * Time.deltaTime);
            }
        }
        if (laserplatforms.Count > 0)
        {
            foreach (ProjectileControlled laser in laserplatforms)
            {
                laser.UpdateLifetime();
                laser.MoveTowardsDirection(8f * Time.deltaTime);
            }
        }
    }

    void UpdateLoop()
    {
        if (stateCoroutine != null) return;
        //StopCoroutine(stateCoroutine);
        //SetState(GetStateEnum(StateEnum.Idle));
        GetNextState();
    }


    Vector2Int GetTargetDirectionInt()
    {
        Vector2 dist = GetTargetDistanceVector();
        return new Vector2Int(dist.x < 0 ? -1 : 1, dist.y < 0 ? -1 : 1); 
    }

    Vector2 GetTargetDirection()
    { return GetTargetDistanceVector().normalized; }

    Vector2 GetTargetDistanceVector()
    { return target.position - transform.position; }
    float GetTargetDistance()
    { return Vector2.Distance(transform.position, target.position); }

    void GetNextState(bool set_idle = true)
    {
        //Debug.Log("GetNextState()");
        
        if (stateQue.Count > 0)
        {
            //SetState(GetStateEnum(stateQue.state), set_idle: set_idle);
            SetState(GetStateEnum(stateQue[0]), set_idle: set_idle);
            stateQue.RemoveAt(0);
            return;
        }
        

        float tdist = GetTargetDistance();
        Vector2 vdist = GetTargetDistanceVector();

        if (tdist <= 5f)
        {
            if (vdist.y < 0)
            {
                if (vdist.x < 2.5f && vdist.x > -2.5f)
                {
                    SetState(GetStateEnum(StateEnum.Stomp), set_idle: set_idle);
                    return;
                }
                SetState(GetStateEnum(GetRandomState(statePoolNearFloor)), set_idle: set_idle);
                return;
            }
            SetState(GetStateEnum(GetRandomState(statePoolNear)), set_idle: set_idle);
            return;
        }
        else if(tdist > 15f)
        {
            if (vdist.y < 0)
            {
                SetState(GetStateEnum(GetRandomState(statePoolFarFloor)), set_idle: set_idle);
                return;
            }
            SetState(GetStateEnum(GetRandomState(statePoolFar)), set_idle: set_idle);
            return;
        }

        if (vdist.y < 0)
        {
            SetState(GetStateEnum(GetRandomState(statePoolFloor)), set_idle: set_idle);
            return;
        }
        SetState(GetStateEnum(GetRandomState(statePool)), set_idle: set_idle);
    }


    void SetState(IEnumerator state, bool force = false, bool set_idle = false)
    {
        //Debug.Log("SetState()");
        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        if (force)
        {
            stateCoroutine = StartCoroutine(state);
            return;
        }
        StartCoroutine(ISetState(state, set_idle));
    }
    IEnumerator ISetState(IEnumerator state, bool set_idle = false)
    {
        //Debug.Log("ISetState()");
        stopState = true;
        yield return new WaitForEndOfFrame();
        stopState = false;
        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        if (set_idle)
        { yield return stateCoroutine = StartCoroutine(IState_Idle()); }
        stateCoroutine = StartCoroutine(state);
    }


    IEnumerator IState_Idle()
    {
        animator.SetState("idle");
        //Debug.Log($"IState_Idle({idle_time})");
        float t = 0;
        while (t < idle_time || forceIdle)
        {
            TurnTowardsTarget();
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame(); 
        }
        stateCoroutine = null;
    }

    IEnumerator IState_Attack()
    {
        //Debug.Log("IState_Attack()");

        //animator.SetState("");
        yield return new WaitForEndOfFrame();
        //idle_time = 1f;
        SetRandomIdleTime(0.5f, 1.5f);
        stateCoroutine = null;
    }

    IEnumerator IState_Throw()
    {
        TurnTowardsTarget();
        animator.SetState("melee.init");
        animState = 1;

        // init throw
        while (animState == 1)
        { yield return new WaitForEndOfFrame(); }

        // throwing animation
        animator.SetState("throw.A");
        while (animState == 2)
        { yield return new WaitForEndOfFrame(); }

        //idle_time = 2f;
        SetRandomIdleTime(1f, 2f);
        stateCoroutine = null;
    }

    IEnumerator IState_Stomp()
    {
        animator.SetState("stomp");
        animState = 2;

        while (animState == 2)
        { yield return new WaitForEndOfFrame(); }

        //idle_time = 2f;
        SetRandomIdleTime(0.5f, 2f);
        stateCoroutine = null;
    }

    IEnumerator IState_AttackTp()
    {
        //Debug.Log($"IState_AttackTp()");
        Vector2 tpos = transform.position;
        Vector2 epos = target.position; ;
        float dist = GetTargetDistance(), speed = 50f;
        float t = 0, tend = dist / speed;
        stopGrav = true;
        while (t < tend)
        {
            transform.position = Vector2.Lerp(tpos, epos, t / tend);
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        transform.position = epos;
        stopGrav = false;
        //idle_time = 1f;
        SetRandomIdleTime(0.5f, 2f);
        stateCoroutine = null;
    }

    IEnumerator IState_Dash()
    {
        //Debug.Log("IState_Dash()");

        animator.SetState("melee.init");
        Vector2Int tdir = GetTargetDirectionInt();
        TurnTowardsTarget();
        float mfrate = -1f;
        if (stat_hp.delta <= 0.25f)
        { mfrate = 0.4f; }
        else if (stat_hp.delta <= 0.25f)
        { mfrate = 0.6f; }
        else if (stat_hp.delta <= 0.75f)
        { mfrate = 0.8f; }
        yield return StartCoroutine(IDash(50f * tdir.x, mfrate));
        //idle_time = 4f;
        SetRandomIdleTime(2f, 4f);
        stateCoroutine = null;
    }

    IEnumerator IState_Missiles()
    {
        //Debug.Log("IState_Missile()");

        float rx = Room.ActiveRoom.roomWorldBounds.center.x;
        float ddir = transform.position.x < rx ? 25f : -25f;

        animator.SetState("melee.init");

        yield return StartCoroutine(IDash(ddir));

        animator.SetState("missiles.init");
        animState = 1;
        while (animState == 1)
        { yield return new WaitForEndOfFrame(); }

        int missile_count = 10;
        float fire_rate = 1f / 5f;
        Vector3 firepos = transform.position.Add(0.5f * facing, 3f); Vector2 firedir = Utility.AngleToVector2(30);
        for (int i = 0; i < missile_count; i++)
        {
            if (stopState) break;
            animator.SetState("missiles.fire");
            FireMissile(firepos, firedir);
            yield return new WaitForSeconds(fire_rate);
        }

        animState = 0;
        //idle_time = 2f;
        SetRandomIdleTime(1f, 3f);
        stateCoroutine = null;
    }

    IEnumerator IState_LaserArray()
    {
        //Debug.Log("IState_LaserArray()");

        float rx = Room.ActiveRoom.roomWorldBounds.center.x;
        int x = transform.position.x < rx ? 1 : -1;
        float ddir = 25f * x;
        yield return StartCoroutine(IDash(ddir));

        animator.SetState("missiles.init");
        animState = 1;
        while (animState == 1)
        { yield return new WaitForEndOfFrame(); }

        int laser_count = 10;
        float fire_over_time = 7f;
        float fire_rate = fire_over_time / laser_count;
        Rect rbounds = Room.ActiveRoom.roomWorldBounds;
        Vector3 firepos = transform.position; firepos.y = rbounds.center.y; firepos.z = 0;
        bool laserDown = true;
        Vector2 tdist;
        for (int i = 0; i < laser_count; i++)
        {
            if (stopState) break;
            FireLaserPlatform(firepos, new Vector2(-x, 0f), laserDown);
            laserDown = !laserDown;
            tdist = GetTargetDistanceVector();
            if (tdist.y < transform.position.y && tdist.x < 2f && tdist.x > -2f)
            {
                stateQue.Insert(0, StateEnum.Stomp);
                break;
            }
            yield return new WaitForSeconds(fire_rate);
        }
        //idle_time = 2f;
        SetRandomIdleTime(1f, 3f);
        stateCoroutine = null;
    }

    IEnumerator IState_Railgun()
    {
        animator.SetState("railgun.init");
        animState = 1;
        int shots = 5;
        float a, shotCooldown = 0.5f;
        Vector3 spos = transform.position.Add(1.5f * facing, 2f);
        Vector2 tdir;

        while (animState == 1)
        { yield return new WaitForEndOfFrame(); }

        for (int i = 0; i < shots; i++)
        {
            animator.SetState("railgun.fire");
            animState = 1;

            tdir = GetTargetDirection();

            a = Vector2.SignedAngle(new Vector2(facing, 0), tdir);

            FireRailshot(spos, tdir.normalized);

            yield return new WaitForSeconds(shotCooldown);
        }
        animState = 0;

        //animator.SetState("railgun.end");
        //animState = 2;
        //while (animState == 2)
        //{ yield return new WaitForEndOfFrame(); }

        //idle_time = 2f;
        SetRandomIdleTime(1f, 2f);
        stateCoroutine = null;
    }


    IEnumerator IDash(float speed, float missile_firerate = 0)
    {

        //Debug.Log($"IDash({speed})");
        // set anim state

        animator.SetState("melee.init");
        animState = 1;
        while (animState == 1)
        { yield return new WaitForEndOfFrame(); }

        animState = 1;

        int speed_pol;
        float ctmove = _velocity.x, dmgspeedtreshold = 8f;
        bool fire_missiles = missile_firerate > 0;
        float mt = 0;
        Vector2 moffset = new Vector2(0, 2f), mdir = Utility.AngleToVector2(60);

        // enable hitbox

        if (speed < 0)
        {
            Set_facing(-1, true);
            speed_pol = -1;
            mdir.x *= speed_pol;
            moffset.x *= speed_pol;
            while (!wallLeft && !stopState)
            {
                if (ctmove > speed)
                { ctmove = Mathf.Max(ctmove + speed * Time.deltaTime, speed); }
                tmove.x = ctmove;

                dashRelay.gameObject.SetActive(tmove.x < -dmgspeedtreshold); 

                if (fire_missiles && (mt += Time.deltaTime) > missile_firerate)
                { mt %= missile_firerate; FireMissile(transform.position.Add(moffset.x, moffset.y), mdir); }

                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            Set_facing(1, true);
            moffset = new Vector2(0, 2f);
            speed_pol = 1;
            while (!wallRight && !stopState)
            {
                if (ctmove < speed)
                { ctmove = Mathf.Min(ctmove + speed * Time.deltaTime, speed); }
                tmove.x = ctmove;
                dashRelay.gameObject.SetActive(tmove.x > dmgspeedtreshold);

                if (fire_missiles && (mt += Time.deltaTime) > missile_firerate)
                { mt %= missile_firerate; FireMissile(transform.position.Add(moffset.x, moffset.y), mdir); }

                yield return new WaitForEndOfFrame();
            }
        }

        dashRelay.gameObject.SetActive(false);

        Vector2 vec = new Vector2(1 * speed_pol, 0);

        if (!stopState)
        {
            // wall hit effect
        }

        //Debug.Log($"IDash({speed}) ended. stopState:{stopState}, wallLeft:{wallLeft}, wallRight:{wallRight}");

        tmove.x = 0;
    }



    void Action_AttackA()
    {
        // attack horizontal
        // instantiate attack projectile
    }
    void Action_Stomp()
    {
        // stomp
        // instantiate stomp projectile
        GameObject go = Instantiate(projectiles.stomp, transform.position.Add(1.25f * facing, 1f), Quaternion.identity);
        go.transform.parent = transform.root;

        Stomp projs = go.GetComponent<Stomp>();
        projs.onTargetHit.AddListener(OnStompHit);

        go.SetActive(true);
    }

    void Action_ThrowA1()
    {
        // throw horizontal blade forward
        Vector2 tpos = transform.position, ppos = Player.pDamageRelayTransform.position, diff = ppos - tpos;
        float a, up_angle = 15f, down_angle = 15f;
        if (facing < 0)
        { a = 180 + Mathf.Clamp(Vector2.SignedAngle(Vector2.left, diff), -up_angle, down_angle); }
        else
        { a = Mathf.Clamp(Vector2.SignedAngle(Vector2.right, diff), -down_angle, up_angle); }
        a *= Mathf.Deg2Rad;

        GameObject go = Instantiate(projectiles.sword_h, tpos.Add(x: 2f * facing), Quaternion.identity);
        go.transform.parent = transform.root;
        SwordProjectile sproj = go.GetComponent<SwordProjectile>();
        sproj.direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        sproj.flipY = facingBool;
        sproj.gravityOverride = Vector2.zero;
        sproj.speed = 40f;

        sproj.onTargetHit.AddListener(OnTargetHit_SThrowH);
        sproj.onStuck.AddListener(OnSwordStuck);
        go.SetActive(true);
    }

    void Action_ThrowA2()
    {
        // throw vertical blade towards player
        Vector2 tpos = transform.position, ppos = Player.pDamageRelayTransform.position, diff = ppos - tpos;
        float a, up_angle = 75f, down_angle = 45f;
        if (facing < 0)
        { a = 180 + Mathf.Clamp(Vector2.SignedAngle(Vector2.left, diff), -up_angle, down_angle); }
        else
        { a = Mathf.Clamp(Vector2.SignedAngle(Vector2.right, diff), -down_angle, up_angle); }
        a *= Mathf.Deg2Rad;

        GameObject go = Instantiate(projectiles.sword_v, tpos.Add(x:1f*facing), Quaternion.identity);
        go.transform.parent = transform.root;
        SwordProjectile sproj = go.GetComponent<SwordProjectile>();
        sproj.direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        sproj.speed = 40f;
        sproj.flipX = !facingBool;
        sproj.gravityOverride = new Vector2(0, gravity);

        sproj.onTargetHit.AddListener(OnTargetHit_SThrowV);
        sproj.onStuck.AddListener(OnSwordStuck);
        go.SetActive(true);
    }


    void OnSwordStuck(SwordProjectile sproj, Collider2D coll)
    { 
        sproj.timeToLive = 2f; 
    }

    void OnAttackHit(Collider2D coll)
    {
        if (TestTarget(coll, out Entity ent))
        { ent.ApplyDamage(new Damage(15f), this); }
    }

    void OnStompHit(Stomp stomp, Collider2D coll, float d)
    {
        if (TestTarget(coll, out Entity ent))
        {
            Vector2 spos = stomp.origin, tpos = coll.transform.position;
            Vector2 dir = tpos.Add(-spos.x, -spos.y).normalized; // new Vector2(tpos.x - spos.x, tpos.y - spos.y).normalized;
            Debug.Log($"dir: vec[{dir}] angle[{Vector2.SignedAngle(Vector2.right, dir)}]");


            float force = 25f * (1f - d);
            Debug.DrawRay(spos, dir, Color.green, 2f);
            Debug.DrawRay(spos + dir, dir * force, Color.red, 2f);

            ent.AddForce(dir * force);
            if (d < 0.5f)
            {
                float dv = 1f - d*2f;
                ent.ApplyDamage(new Damage(20f * dv), this); 
            }
        }
    }
    public void OnDashHit(RelayCollider relay, Collider2D coll)
    {
        if (TestTarget(coll, out Entity ent))
        { ent.ApplyDamage(new Damage(15f), this); }
    }

    void OnTargetHit_SThrowH(SwordProjectile sproj, Collider2D coll)
    {
        if (TestTarget(coll, out Entity ent))
        { ent.ApplyDamage(new Damage(15f), this); }
    }
    void OnTargetHit_SThrowV(SwordProjectile sproj, Collider2D coll)
    {
        if (TestTarget(coll, out Entity ent))
        { ent.ApplyDamage(new Damage(15f), this); }
    }

    void OnRailshotHit(Projectile_ shot, RaycastHit2D rhit)
    {
        if (TestTarget(rhit.collider, out Entity ent))
        { ent.ApplyDamage(new Damage(15f), this); }
    }

    void OnLaserHit(Laser laser, Collider2D coll)
    {
        if (TestTarget(coll, out Entity ent))
        { ent.ApplyDamage(new Damage(15f), this); }
    }

    void OnMissileHit(ProjectileControlled proj, Collider2D coll)
    {
        if (TestTarget(coll, out Entity ent))
        { 
            ent.ApplyDamage(new Damage(15f), this);
            Destroy(proj.gameObject);
        }
    }
    void OnMissileDestroyed(ProjectileControlled proj)
    {
        missiles.Remove(proj);
    }



    void FireRailshot(Vector3 point, Vector2 direction)
    {
        GameObject go = Instantiate(projectiles.railshot, point, Quaternion.identity);
        go.transform.parent = transform.root;

        Projectile_ proj = go.GetComponent<Projectile_>();
        proj.velocity = direction * 200f;
        proj.turnTowardsVelocity = true;
        proj.TurnTowardsVelocityInstant();
        proj.onColliderEnter.AddListener(OnRailshotHit);

        go.SetActive(true);
    }


    void FireMissile(Vector3 point, Vector2 direction)
    {
        //Debug.Log($"FireMissile({point}, {direction})");
        GameObject go = Instantiate(projectiles.missile, point, Quaternion.identity);
        go.transform.parent = transform.root;
        Vector3 eul = go.transform.eulerAngles;
        eul.z = UnityEngine.Random.Range(105f, 75f);
        go.transform.eulerAngles = eul;
        go.SetActive(true);

        var pcontr = go.GetComponent<ProjectileControlled>();
        pcontr.TimeToLive = 3f;

        missiles.Add(pcontr);
        pcontr.onTriggerEnter.AddListener(OnMissileHit);
        pcontr.onDestroy.AddListener(OnMissileDestroyed);
    }



    void FireLaserPlatform(Vector3 point, Vector2 direction, bool laserDown)
    {
        GameObject go = Instantiate(laserDown ? projectiles.laserPlatform_down : projectiles.laserPlatform_up, point, Quaternion.identity);
        go.transform.parent = transform.root;
        go.SetActive(true);

        Vector3 eul = go.transform.eulerAngles;
        eul.z = laserDown ? 180f : 0f;
        go.transform.eulerAngles = eul;

        var pcontr = go.GetComponent<ProjectileControlled>();
        var plaser = go.GetComponent<Laser>();

        pcontr.TimeToLive = 10f;
        pcontr.direction = direction;

        laserplatforms.Add(pcontr);
        //pcontr.onTriggerEnter.AddListener(O);
        plaser.onLaserHit.AddListener(OnLaserHit);
        pcontr.onDestroy.AddListener(OnLaserplatformDestroyed);
    }
    void OnLaserplatformDestroyed(ProjectileControlled proj)
    {
        laserplatforms.Remove(proj);
    }


    void FireLaser(Vector3 point, Vector2 direction)
    {

    }




    void OnAnimEnd()
    {
        if (animState > 0)
        { animState = (animState + 1) % 3; }
    }

    void TurnTowardsTarget()
    {
        Set_facing(GetTargetDirection().x >= 0);
        //animator.FlipX(_facing < 0); 
    }


    void SetRandomIdleTime(float min = 1f, float max = 3f)
    {
        float hpvd = Mathf.Lerp(0.5f, 1f, stat_hp.delta); 
        idle_time = UnityEngine.Random.Range(min * hpvd, max * hpvd); 
    }

    bool TestTarget(Collider2D coll, out Entity entity_out)
    {
        if (coll.tag == "Player")
        {
            if (coll.TryGetComponent<RelayCollider>(out RelayCollider rel))
            {
                if (rel.HasTag("damage"))
                {
                    entity_out = rel.entity;
                    return true;
                }
            }
        }
        entity_out = null;
        return false;
    }

    void OnHealthChanged(EntityStats.Stat stat, float value, float last_value)
    {
        if (value < last_value) OnDamageTaken(last_value - value);
    }

    void OnDamageTaken(float value)
    {

    }


    /*
    IEnumerator ITestStates(int tests_per_yield = 1)
    {
        tests_per_yield = Mathf.Max(tests_per_yield, 1);
        StateWeightPair lsw = states[0], nsw;
        EntityStats.Stat tstathp = target.GetComponent<EntityStats>()?.GetStat("health");
        float thp = tstathp.delta, shp = stat_hp.delta, dist = GetTargetDistance();
        Vector2 vecdist = GetTargetDistanceVector();
        int i, test = 0, scount = states.Count;
        float ltest = lsw.TestWeight(dist, vecdist, 0f, thp, shp, 0, 20f), ntest;
        while (test < scount)
        {
            thp = tstathp.delta;
            shp = stat_hp.delta;
            for (i = 0; i < tests_per_yield; i++)
            {
                nsw = states[i];
                ntest = nsw.TestWeight(dist, vecdist, 0f, thp, shp, 0, 20f);
                if (ntest > ltest)
                { lsw = nsw; ltest = ntest; }
            }
            yield return null;
        }
        stateQue = lsw;
    }
    

    class StateWeightPair
    {
        public StateEnum state = 0;
        public WeightValue distance = new(), distance_x = new(), distance_y = new(), aggression = new(), targetHp = new(), selfHp = new(), hpDiff = new();
        //public float distance = 1f, targetAggression = 1f, targetHp = 1f, selfHp = 1f;
        Vector2 distancevec = new Vector2(1f, 1f);
        public StateWeightPair(StateEnum state)
        { this.state = state; }
        public StateWeightPair(StateEnum state, 
            WeightValue distance = null, WeightValue distance_x = null, WeightValue distance_y = null, 
            WeightValue aggression = null, WeightValue targetHp = null, WeightValue selfHp = null, WeightValue hpDiff = null)
        { 
            this.state = state;
            this.distance = distance != null ? distance : new();
            this.distance_x = distance_x != null ? distance_x : new();
            this.distance_y = distance_y != null ? distance_y : new();
            this.aggression = aggression != null ? aggression : new();
            this.targetHp = targetHp != null ? targetHp : new();
            this.selfHp = selfHp != null ? selfHp : new();
            this.hpDiff = hpDiff != null ? hpDiff : new();
        }
        public float TestWeight(float tdist, Vector2 tdistvec, float taggr, float thp, float shp, float baseValue = 0f, float deviation = 0f)
        {
            return distance.TestValue(tdist) +
                distance.TestValue(tdistvec.x) +
                distance.TestValue(tdistvec.y) +
                aggression.TestValue(taggr) +
                targetHp.TestValue(thp) +
                selfHp.TestValue(shp) +
                hpDiff.TestValue(shp>0?(thp / shp):1f) + 
                UnityEngine.Random.Range(0f, deviation) - deviation * 0.5f +
                baseValue;
        }
     
    }

    class WeightValue
    {
        public float baseValue = 50f, multiplier = 0f, minValue = 0f, maxValue = 100f, deviation = 0f;
        public WeightValue() { }
        public WeightValue(float base_value = 50f, float mult = 0f, float minValue = 0f, float maxValue = 100f, float deviation = 0f)
        { this.baseValue = base_value; this.multiplier = mult; this.minValue = minValue; this.maxValue = maxValue; this.deviation = deviation; }
        public float TestValue(float value)
        {
            if (deviation != 0) return Mathf.Clamp(value * multiplier + baseValue + (UnityEngine.Random.Range(0f, deviation) - deviation * 0.5f), minValue, maxValue);
            return Mathf.Clamp(value * multiplier + baseValue, minValue, maxValue);
        }
    }
    */
}
