
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AI_Boss : EntityController
{
    public enum StateEnum { Idle, Attack, AttackTp, Dash, Missile, Gatling, LaserArray }

    
    List<StateEnum> statePool = new() { StateEnum.AttackTp, StateEnum.Dash, StateEnum.Missile, StateEnum.LaserArray },
        statePoolNear = new() { StateEnum.Attack, StateEnum.Dash, StateEnum.AttackTp },
        statePoolFar = new() { StateEnum.Dash, StateEnum.AttackTp, StateEnum.Missile, StateEnum.LaserArray };

    List<ProjectileControlled> missiles = new(), laserplatforms = new();

    StateEnum GetRandomState(List<StateEnum> pool)
    { return pool[UnityEngine.Random.Range(0, pool.Count)]; }
    IEnumerator GetStateEnum(StateEnum state)
    {
        Debug.Log("GetSetEnum()");
        switch (state)
        {
            case StateEnum.Idle: return IState_Idle();
            case StateEnum.Attack: return IState_Attack();
            case StateEnum.AttackTp: return IState_AttackTp();
            case StateEnum.Dash: return IState_Dash();
            case StateEnum.Missile: return IState_Missile();
            //case StateEnum.Gatling: return null;
            case StateEnum.LaserArray: return IState_LaserArray();
            default: return IState_Idle();
        }
    }



    float _startTime, idle_time = 2f;
    public float TimePassed { get => Time.time - _startTime; }

    Transform target;

    Coroutine stateCoroutine;
    bool stopState = false;

    int facing;

    Vector2 tmove = Vector2.zero;
    bool stopGrav = false;

    EntityStats.Stat stat_hp;

    void SetFacing(bool isRight)
    { facing = isRight ? 1 : -1; }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        target = Player.pTransform;

        stat_hp = entityStats.SetStat("health", 1000f, 1000f);
        ignorePlatform = true;

        entityStats.SetAttribute("forceResist", 50f);
    }

    private void OnEnable()
    {
        idle_time = 4f;
        //StartCoroutine(IUpdateLoop());
        StartCoroutine(ISetBossHpBar());
    }
    IEnumerator ISetBossHpBar()
    {
        yield return new WaitForEndOfFrame();
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

    [SerializeField] GameObject prefab_missile;
    void FireMissile(Vector3 point, Vector2 direction)
    {
        Debug.Log($"FireMissile({point}, {direction})");
        GameObject go = Instantiate(prefab_missile, point, Quaternion.identity);
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

    void OnMissileHit(ProjectileControlled proj, Collider2D coll)
    {
        if (coll.tag == "Player")
        {
            if (coll.TryGetComponent<RelayCollider>(out RelayCollider rel))
            {
                if (rel.HasTag("damage"))
                {
                    rel.entity.ApplyDamage(new Damage(15f), this);
                    Destroy(proj.gameObject);
                }
            }
        }
    }
    void OnMissileDestroyed(ProjectileControlled proj)
    {
        missiles.Remove(proj);
    }


    [SerializeField] GameObject prefab_laserPlatform_down, prefab_laserPlatform_up;
    void FireLaserPlatform(Vector3 point, Vector2 direction, bool laserDown)
    {
        GameObject go = Instantiate(laserDown ? prefab_laserPlatform_down : prefab_laserPlatform_up, point, Quaternion.identity);
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
        Debug.Log($"FireLaser({point}, {direction})");
    }

    void OnLaserHit(Laser laser, Collider2D coll)
    {
        if (coll.tag == "Player")
        {
            if (coll.TryGetComponent<RelayCollider>(out RelayCollider rel))
            {
                if (rel.HasTag("damage"))
                {
                    rel.entity.ApplyDamage(new Damage(15f), this);
                }
            }
        }
    }

    


    IEnumerator IUpdateLoop()
    {
        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        idle_time = 4f;
        SetState(GetStateEnum(StateEnum.Idle));
        while (true)
        {
            yield return stateCoroutine;
            GetNextState();
        }
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
        Debug.Log("GetNextState()");
        float tdist = GetTargetDistance();
        if (tdist <= 5f)
        {
            SetState(GetStateEnum(GetRandomState(statePoolNear)), set_idle: set_idle);
            return;
        }
        else if(tdist > 15f)
        {
            SetState(GetStateEnum(GetRandomState(statePoolFar)), set_idle: set_idle);
            return;
        }
        
        SetState(GetStateEnum(GetRandomState(statePool)), set_idle: set_idle);
    }


    void SetState(IEnumerator state, bool force = false, bool set_idle = false)
    {
        Debug.Log("SetState()");
        if (force)
        {
            if (stateCoroutine != null) StopCoroutine(stateCoroutine);
            stateCoroutine = StartCoroutine(state);
            return;
        }
        StartCoroutine(ISetState(state, set_idle));
    }
    IEnumerator ISetState(IEnumerator state, bool set_idle = false)
    {
        Debug.Log("ISetState()");
        stopState = true;
        yield return new WaitForEndOfFrame();
        stopState = false;
        if (set_idle)
        {
            if (stateCoroutine != null) StopCoroutine(stateCoroutine);
            yield return stateCoroutine = StartCoroutine(IState_Idle());
        }
        if (stateCoroutine != null) StopCoroutine(stateCoroutine);
        stateCoroutine = StartCoroutine(state);
    }

    IEnumerator IState_Idle()
    {
        Debug.Log($"IState_Idle({idle_time})");
        float t = 0;
        while (t < idle_time)
        {
            t += Time.deltaTime;
            yield return new WaitForEndOfFrame(); 
        }
        stateCoroutine = null;
    }
    IEnumerator IState_Attack()
    {
        Debug.Log("IState_Attack()");

        yield return new WaitForSeconds(2f);
        idle_time = 1f;
        stateCoroutine = null;
    }
    IEnumerator IState_AttackTp()
    {
        Debug.Log($"IState_AttackTp()");
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
        idle_time = 1f;
        stateCoroutine = null;
    }

    IEnumerator IState_Dash()
    {
        Debug.Log("IState_Dash()");

        Vector2Int tdir = GetTargetDirectionInt();
        yield return StartCoroutine(IDash(40f * tdir.x));
        idle_time = 4f;
        stateCoroutine = null;
    }

    IEnumerator IState_Missile()
    {
        Debug.Log("IState_Missile()");

        float rx = Room.ActiveRoom.roomWorldBounds.center.x;
        float ddir = transform.position.x < rx ? 25f : -25f;
        yield return StartCoroutine(IDash(ddir));

        int missile_count = 10;
        float fire_rate = 1f / 5f;
        Vector3 firepos = transform.position; Vector2 firedir = Vector2.up;
        for (int i = 0; i < missile_count; i++)
        {
            if (stopState) break;
            FireMissile(firepos, firedir);
            yield return new WaitForSeconds(fire_rate);
        }
        idle_time = 2f;
        stateCoroutine = null;
    }

    IEnumerator IState_LaserArray()
    {
        Debug.Log("IState_LaserArray()");

        float rx = Room.ActiveRoom.roomWorldBounds.center.x;
        int x = transform.position.x < rx ? 1 : -1;
        float ddir = 25f * x;
        yield return StartCoroutine(IDash(ddir));

        int laser_count = 10;
        float fire_over_time = 7f;
        float fire_rate = fire_over_time / laser_count;
        Rect rbounds = Room.ActiveRoom.roomWorldBounds;
        Vector3 firepos = transform.position; firepos.y = rbounds.center.y; firepos.z = 0;
        bool laserDown = true; 
        for (int i = 0; i < laser_count; i++)
        {
            if (stopState) break;
            FireLaserPlatform(firepos, new Vector2(-x, 0f), laserDown);
            laserDown = !laserDown;
            yield return new WaitForSeconds(fire_rate);
        }
        idle_time = 2f;
        stateCoroutine = null;
    }


    IEnumerator IDash(float speed)
    {

        Debug.Log($"IDash({speed})");
        // set anim state
        int speed_pol;
        float ctmove = _velocity.x;

        if (speed < 0)
        {
            speed_pol = -1;
            while (!wallLeft && !stopState)
            {
                if (ctmove > speed)
                { ctmove = Mathf.Max(ctmove + speed * Time.deltaTime, speed); }
                tmove.x = ctmove;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            speed_pol = 1;
            while (!wallRight && !stopState)
            {
                if (ctmove < speed)
                { ctmove = Mathf.Min(ctmove + speed * Time.deltaTime, speed); }
                tmove.x = ctmove;
                yield return new WaitForEndOfFrame();
            }
        }

        Vector2 vec = new Vector2(1 * speed_pol, 0);

        if (!stopState)
        {
            // wall hit effect
        }

        Debug.Log($"IDash({speed}) ended. stopState:{stopState}, wallLeft:{wallLeft}, wallRight:{wallRight}");

        tmove.x = 0;
    }


    class StateWeightPair
    {
        public StateEnum state = 0;
        public float distance = 1f, targetAggression = 1f, targetHp = 1f, selfHp = 1f;
        Vector2 distancevec = new Vector2(1f, 1f);
        public StateWeightPair(StateEnum state, float distance, Vector2 distancevec)
        {
            this.state = state;
            this.distance = distance;
            this.distancevec = distancevec;
        }
        public float TestWeight(float tdist, Vector2 tdistvec, float taggr, float thp, float shp)
        {
            return tdist * distance +
                tdistvec.x * distancevec.x +
                tdistvec.y * distancevec.y +
                taggr * targetAggression +
                thp * targetHp +
                shp * selfHp;
        }

    }
}
