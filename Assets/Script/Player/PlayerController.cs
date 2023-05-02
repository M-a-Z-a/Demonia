
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Utility;

public class PlayerController : Entity
{

    CurveSlider velAttack = new CurveSlider(1, 0), velRelease = new CurveSlider(-1f, -1f);

    Rigidbody2D rb;
    EntityController entity;
    BoxCollider2D coll;
    protected bool simulationEnabled = true;

    // rect
    Rect rect;
    Vector2 r_sz2;
    bool rectChanged = false;

    LayerMask groundMask, platformMask, gpMask;

    // attributes
    EntityStats.Attribute speed, fallSpeedCap, fallDamageTreshold, speedApexMult, accelSpeed, decelSpeed;
    float xSpeedMult = 1f, gravityMult = 1f;
    protected float gravityMultiplier = 1f;

    RayGroup2D rgLeft, rgRight, rgUp, rgDown;

    Vector2 grabPoint = new Vector2(0.1f, 0.1f);

    protected bool stunFlag, ignorePlatform = false;
    protected bool isGrounded, isPlatform, wallLeft, wallRight, wallUp, ledgeLeft, ledgeRight;
    protected float wasGrounded = 0;

    protected bool ledgegrabEnabled = true;

    Vector2 targetMove = Vector2.zero, groundVelocity = Vector2.zero, _relativeVelocity = Vector2.zero;
    protected bool isJumping = false;
    bool stopGravity = false;
    float cGrav = 0f;
    protected float currentGravity { get => cGrav; }
    public Vector2 relativeVelocity { get => _relativeVelocity; }

    Dir4 maxMove = new Dir4();


    
    protected override void Awake()
    {
        base.Awake();

        groundMask = groundMaskDefault;
        platformMask = platformMaskDefault;
        gpMask = groundMask | platformMask;

        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        UpdateRect();
        SetRayGroups();

        speed = entityStats.GetSetAttribute("speed", 10f);
        speedApexMult = entityStats.GetSetAttribute("speedapexmult", 1.5f);
        fallSpeedCap = entityStats.GetSetAttribute("fallSpeedCap", 60f);
        fallDamageTreshold = entityStats.GetSetAttribute("fallDamageTreshold", 20f);
        accelSpeed = entityStats.GetSetAttribute("accelSpeed", 20f);
        decelSpeed = entityStats.GetSetAttribute("decelSpeed", 10f);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected void SetColliderRect(Rect rect)
    {
        if (rect.size == this.rect.size && rect.position == this.rect.position)
            return;
        coll.offset = rect.position;
        coll.size = rect.size;
        rectChanged = true;
    }
    void UpdateRect()
    {
        rect = coll.Rect();
        r_sz2 = rect.size / 2;
    }
    void SetRayGroups()
    {
        rgLeft = new RayGroup2D(5, groundMask); rgLeft.baseDistance = r_sz2.x;
        rgRight = new RayGroup2D(5, groundMask); rgRight.baseDistance = r_sz2.x;
        rgUp = new RayGroup2D(3, groundMask); rgUp.baseDistance = 0.25f; rgUp.doffset = r_sz2.y - rgUp.baseDistance;
        rgDown = new RayGroup2D(5, groundMask); rgDown.baseDistance = 0.25f; rgDown.doffset = r_sz2.y - rgDown.baseDistance;
    }
    void UpdateRayGroups()
    {
        rgLeft.baseDistance = r_sz2.x; 
        rgRight.baseDistance = r_sz2.x; 
        rgUp.doffset = r_sz2.y - rgUp.baseDistance;
        rgDown.doffset = r_sz2.y - rgDown.baseDistance;
    }

    protected void Move(float x = 0, float y = 0)
    { targetMove.x = Mathf.Clamp(x, -1, 1); targetMove.y = Mathf.Clamp(y, -1, 1); }
    protected void Move(Vector2 xy)
    { targetMove.x = Mathf.Clamp(xy.x, -1, 1); targetMove.y = Mathf.Clamp(xy.y, -1, 1); }

    protected void SetVelocity(Vector2 v)
    {
        _velocity = v;
    }



    protected virtual void FixedUpdate()
    {
        if (!simulationEnabled) return;
        if (rectChanged)
        {
            UpdateRect();
            UpdateRayGroups();
        }

        stunFlag = entityStats.GetFlag("stunned");

        maxMove.SetAll(1f);
        
        HandleCollisions();

        if (isGrounded)
        {
            HandleGroundedMove();
        }
        else
        {
            HandleAerialMove();
            HandleGravity();
        }

        HandlePosition();

        _relativeVelocity = _velocity - groundVelocity;
        targetMove = Vector2.zero;
        rectChanged = false;
    }



    void HandleGroundedMove()
    {
        Vector2 clampMove = Vector2.zero;
        Vector2 bvel = groundVelocity;
        Vector2 cvel = _velocity;
        Vector2 tvel = new Vector2(targetMove.x * speed + bvel.x, bvel.y);
        int p = CompareVals(tvel.x, cvel.x, bvel.x);
        float spd = accelSpeed;
        if (p != 0)
        {
            spd = decelSpeed;
            if (targetMove.x != 0)
            { spd *= 4; }
        }
        
        cvel.x = TowardsTargetValue(cvel.x, tvel.x, spd * Time.fixedDeltaTime);
        cvel.y = cvel.y > 0 ? Mathf.Max(cvel.y, bvel.y) : bvel.y;
        _velocity = cvel;
    }

    void HandleAerialMove()
    {
        if (targetMove.x != 0)
        {
            /*
            float xspd = 0;
            if (CompareVals(_velocity.x, targetMove.x, 0) == 0)
            {
                if (targetMove.x < 0) xspd = Mathf.Min(_velocity.x, targetMove.x);
                else xspd = Mathf.Max(_velocity.x, targetMove.x);
            }
            */
            Vector2 tvel = new Vector2(targetMove.x * speed * xSpeedMult + groundVelocity.x, groundVelocity.y);
            int p = CompareVals(tvel.x, _velocity.x, groundVelocity.x);
            float spd = accelSpeed * 0.75f;
            if (p != 0)
            {
                spd = decelSpeed * 0.75f;
                if (targetMove.x != 0)
                { spd *= 2; }
            }
            _velocity.x = TowardsTargetValue(_velocity.x, tvel.x, spd * xSpeedMult * Time.fixedDeltaTime);
        }

        ledgeLeft = false; ledgeRight = false;

        if (ledgegrabEnabled && _velocity.y < 0.1f)
        {
            Vector3 tpos = transform.position;
            int tdir = 0;
            if (targetMove.x > 0)
            { tdir = 1; }
            else if (targetMove.x < 0)
            { tdir = -1; }
            else if (wallRight)
            { tdir = 1; }
            else if (wallLeft)
            { tdir = -1; }
            else 
            { goto stoppa; } // goto stoppa label, drops out of if loop

            Vector2 p1 = new Vector2(tpos.x, tpos.y + r_sz2.y);
            float cdist = grabPoint.x + 0.05f + r_sz2.x;

            RaycastHit2D rhit = Physics2D.Raycast(p1, tdir > 0 ? Vector2.right : Vector2.left, cdist, groundMask);
            if (!rhit.collider)
            {
                p1.x += cdist * tdir;
                cdist = grabPoint.y + 0.05f - Mathf.Min(_velocity.y * Time.fixedDeltaTime, 0);
                rhit = Physics2D.Raycast(p1, Vector2.down, cdist, groundMask);
                if (rhit.collider)
                {
                    if (tdir < 0) ledgeLeft = true;
                    else ledgeRight = true;
                    tpos.y = rhit.point.y - (r_sz2.y - grabPoint.y);
                    transform.position = tpos;
                    _velocity.y = 0;
                    stopGravity = true;
                }
            }
        } stoppa:; // goto endpoint

    }

    void HandleGravity()
    {
        if (stopGravity)
        { stopGravity = false; cGrav = 0; return; }
        gravityMult = _velocity.y < 0 ? 1f : (isJumping ? 0.5f : 2f);
        cGrav = gravity * gravityMultiplier * gravityMult;
        _velocity.y = Mathf.Max(_velocity.y + cGrav * Time.fixedDeltaTime, -fallSpeedCap);
    }


    void HandleCollisions()
    {
        Vector3 tpos = transform.position;
        Vector2Range vecrang = GetRayRange(transform.position.Add(y:-rgDown.doffset), Vector2.right, r_sz2.x, groundMask, -0.05f);

        groundVelocity = Vector2.zero;
        // ground check
        rgDown.layerMask = !ignorePlatform? ((isGrounded || _velocity.y <= 0) ? gpMask : groundMask) : groundMask;
        if (rgDown.Cast(transform.position, Vector2.down, vecrang, 0.3f + Mathf.Max(-velocity.y * Time.fixedDeltaTime * 2, 0)) > 0)
        {
            if (rgDown.shortHit.point.y + r_sz2.y + 0.01f >= tpos.y)
            {
                //groundVelocity = Vector2.zero;
                foreach (RaycastHit2D rhit in rgDown.raycastHits)
                {
                    if (rhit.collider && rhit.collider.gameObject.TryGetComponent(out Entity e))
                    { 
                        groundVelocity.x += e.velocity.x; groundVelocity.y = Mathf.Max(groundVelocity.y, e.velocity.y); 
                    }
                }
                groundVelocity.x /= rgDown.hitCount;
                tpos.y = rgDown.shortHit.point.y + r_sz2.y; //Mathf.Max(tpos.y, rgDown.shortHit.point.y + rHalf.y);
                transform.position = tpos;
                if (!isGrounded)
                {
                    if (_velocity.y < -fallDamageTreshold)
                    {
                        float rang = fallSpeedCap - fallDamageTreshold;
                        float val = rang > 0 ? (Mathf.Abs(_velocity.y) - fallDamageTreshold) / rang : 1f;
                        OnEnterGrounded(_velocity, Mathf.Clamp(val, 0f, 1f));
                    }
                    else
                    { OnEnterGrounded(_velocity, 0f); }
                }
                //if (!isJumping) _velocity.y = groundVelocity.y;
                isGrounded = true; wasGrounded = 0f;
                isPlatform = LayerInMask(rgDown.shortHit.collider.gameObject.layer, platformMask);
            }
            else
            { isGroundedElse(); }
            maxMove.down = rgDown.hitBaseDistance;
        }
        else
        { isGroundedElse(); }


        vecrang = GetRayRange(transform.position.Add(y: rgUp.doffset), Vector2.right, r_sz2.x, groundMask, -0.05f);
        wallUp = false;
        if (rgUp.Cast(transform.position, Vector2.up, vecrang, 0.3f) > 0)
        {
            if (rgUp.hitCount > 1)
            {
                wallUp = true;
                tpos.y = Mathf.Min(tpos.y, rgUp.shortHit.point.y - r_sz2.y);
                transform.position = tpos;
                _velocity.y = Mathf.Min(_velocity.y, 0);
                maxMove.up = rgUp.hitBaseDistance;
            }
        }

        wallLeft = false;
        vecrang = GetRayRange(transform.position.Add(x: -rgLeft.doffset), Vector2.up, r_sz2.y, groundMask, -0.05f);
        if (rgLeft.Cast(transform.position, Vector2.left, vecrang, r_sz2.x + 0.01f) > 0)
        {
            if (rgLeft.hitCount == 1 && rgLeft.hitIndex == 0)
            {
                WallLeftElse();
            }
            else
            {
                if (rgLeft.raycastHits[2].collider && rgLeft.raycastHits[3].collider)//rgLeft.hitCount > 1)
                { 
                    if (!wallLeft) 
                    { OnTouchWallLeft(_velocity); }; 
                    wallLeft = true; 
                } 
                else { WallLeftElse(); }

                tpos.x = rgLeft.shortHit.point.x + r_sz2.x;//Mathf.Max(tpos.x, rgLeft.shortHit.point.x + rHalf.x);
                transform.position = tpos;
                _velocity.x = Mathf.Max(_velocity.x, 0);
                maxMove.left = rgLeft.hitBaseDistance;
            }
        }
        else
        { WallLeftElse(); }

        wallRight = false;
        vecrang = GetRayRange(transform.position.Add(x: rgRight.doffset), Vector2.up, r_sz2.y, groundMask, -0.05f);
        if (rgRight.Cast(transform.position, Vector2.right, vecrang, r_sz2.x + 0.01f) > 0)
        {
            if (rgRight.hitCount == 1 && rgRight.hitIndex == 0)
            {
                WallRightElse();
            }
            else
            {
                if (rgRight.raycastHits[2].collider && rgRight.raycastHits[3].collider)//  rgRight.hitCount > 1)
                { 
                    if (!wallRight) 
                    { OnTouchWallRight(_velocity); }; 
                    wallRight = true;  
                }
                else { WallRightElse(); }

                tpos.x = rgRight.shortHit.point.x - r_sz2.x;// Mathf.Min(tpos.x, rgRight.shortHit.point.x - rHalf.x);
                transform.position = tpos;
                _velocity.x = Mathf.Min(_velocity.x, 0);
                maxMove.right = rgRight.hitBaseDistance;
            }
        }
        else
        { WallRightElse(); }
        

        void isGroundedElse()
        {
            if (isGrounded)
            { OnExitGrounded(); }
            isGrounded = false; wasGrounded += Time.fixedDeltaTime;
            isPlatform = false;
        }

        void WallLeftElse()
        { wallLeft = false; }
        void WallRightElse()
        { wallRight = false; }
    }

    void HandlePosition()
    {
        Vector2 nmove = _velocity * Time.fixedDeltaTime;
        if (nmove.y < 0)
        { nmove.y = Mathf.Max(nmove.y, -Mathf.Max(maxMove.down, 0)); }
        else if (nmove.y > 0)
        { nmove.y = Mathf.Min(nmove.y, Mathf.Max(maxMove.up, 0)); }

        if (nmove.x < 0)
        { nmove.x = Mathf.Max(nmove.x, -Mathf.Max(maxMove.left, 0)); }
        else if (nmove.x > 0)
        { nmove.x = Mathf.Min(nmove.x, Mathf.Max(maxMove.right, 0)); }

        Vector3 tpos = transform.position.Add(nmove.x, nmove.y);
        transform.position = tpos;

        // late fix
        RaycastHit2D rhit = Physics2D.Raycast(tpos, Vector2.down, r_sz2 .y- 0.05f, groundMask);
        if (rhit.collider)
        { transform.position = new Vector3(tpos.x, rhit.point.y + r_sz2.y, tpos.z); }
    }



    protected virtual void OnEnterGrounded(Vector2 velocity, float fallDamageDelta) 
    { /*Debug.Log($"OnEnterGrounded({velocity}, {fallDamageDelta})");*/ }
    protected virtual void OnExitGrounded() 
    { /*Debug.Log("OnExitGrounded()");*/ }

    protected virtual void OnTouchWallLeft(Vector2 velocity)
    { /*Debug.Log("OnTouchWallLeft()");*/ }
    protected virtual void OnTouchWallRight(Vector2 velocity)
    { /*Debug.Log("OnTouchWallRight()");*/ }



    int CompareVals(float a, float b, float _base)
    {
        if (a > _base && b < _base)
        { return 1; }
        if (a < _base && b > _base)
        { return -1; }
        return 0;
    }

    bool jumpwaitdelay = false;
    protected void JumpInit(Vector2 force, float boostForce, float holdTime = 0.5f)
    {
        if (jumpwaitdelay) return;
        //Debug.Log("jump init");
        if (isJumping) return; 
        isJumping = true;
        _velocity += force;
        StartCoroutine(IJumpDelay(0.1f));
        StartCoroutine(IJumpBoost(boostForce, holdTime)); 
    }

    protected void JumpRelease()
    { isJumping = false; }


    Vector2Range GetRayRange(Vector2 origin, Vector2 dir_from_origin, float dist_from_origin, LayerMask layerMask, float padding = -0.05f)
    {
        RaycastHit2D rhit;
        return new(GetPoint(-dir_from_origin, padding), GetPoint(dir_from_origin, padding));

        Vector2 GetPoint(Vector2 dir, float add_padding)
        {
            rhit = Physics2D.Raycast(origin, dir, dist_from_origin, layerMask);
            if (rhit.collider) return origin + dir * (rhit.distance + add_padding);
            return origin + dir * (dist_from_origin + add_padding);
        }
    }

    IEnumerator IJumpDelay(float t)
    {
        jumpwaitdelay = true;
        yield return new WaitForSeconds(t);
        jumpwaitdelay = false;
    }

    IEnumerator IJumpBoost(float holdForce, float time)
    {
        float t = 0f;
        float vel = holdForce;
        while (t < time && isJumping)
        {
            t += Time.fixedDeltaTime;
            _velocity.y += Mathf.Lerp(vel, 0f, EaseOutCirc01(t / time)) * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        isJumping = false;
        //StartCoroutine(IWaitJumpApex(speedApexMult, 1f, 0.5f));
    }

    IEnumerator IJumpApexBoost(float multStart, float multEnd, float time)
    {
        float t = 0;
        xSpeedMult = multStart;
        while (t < time)
        {
            t += Time.fixedDeltaTime;
            xSpeedMult = CurveCombination(t / time, EaseInCirc01, EaseOutSine01, 0.25f);
            xSpeedMult = Mathf.Lerp(multStart, multEnd, t / time);
            yield return new WaitForFixedUpdate();
        }
        xSpeedMult = multEnd;
    }

    IEnumerator IWaitJumpApex(float multStart, float multEnd, float time)
    {
        while (_velocity.y > 0)
        {
            if (isGrounded) yield break;
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine(IJumpApexBoost(multStart, multEnd, time));
    }
    

    IEnumerator ILerpStaticMovement(Vector2 start, Vector2 end, float time)
    {
        simulationEnabled = false;
        float t = 0;
        while (t < time || !simulationEnabled)
        {
            t += Time.fixedDeltaTime;
            transform.position = Vector2.Lerp(start, end, t / time);
            yield return new WaitForFixedUpdate();
        }
        simulationEnabled = true;
    }


    struct Vector2Range
    {
        public readonly Vector2 start, end;
        public Vector2Range(Vector2 start = default, Vector2 end = default)
        { this.start = start; this.end = end; }
    }

    class RayGroup2D
    {
        protected Vector2 _cPoint;
        protected RaycastHit2D[] _rHits;
        protected float _dOffset = 0;
        protected float _hitDist, _hitBDist, _castDist, _baseDist = 0;
        protected int _hitIndex, _hitCount, _castCount, _rayCount;
        private int _rDiv;


        public LayerMask layerMask;

        public float doffset { get => _dOffset; set => _dOffset = value; }
        public Vector2 castPointOrigin { get => _cPoint; }
        public float hitDistance { get => _hitDist; }
        public float hitBaseDistance { get => _hitBDist; }
        public float castDistance { get => _castDist; }
        public float baseDistance { get => _baseDist; set => _baseDist = value; }
        public int hitIndex { get => _hitIndex; }
        public int rayCount { get => _rayCount; }
        public int hitCount { get => _hitCount; }
        public int castCount { get => _castCount; }
        public RaycastHit2D[] raycastHits { get => _rHits; }
        public RaycastHit2D shortHit { get => _rHits[_hitIndex]; }
        

        public RayGroup2D(int rayCount, LayerMask layerMask)
        {
            _rHits = new RaycastHit2D[rayCount];
            _rayCount = rayCount; _castCount = rayCount;
            _rDiv = Mathf.Max(rayCount - 1, 1);
            this.layerMask = layerMask;
        }


        public int Cast(Vector2 origin, Vector2 direction, Vector2Range range, float distance)
        {
            _cPoint = origin;
            ResetValues(distance);

            RaycastHit2D rhit; int index = -1; Vector2 rpos;
            int i; for (i = 0; i < _rayCount; i++)
            {
                rpos = Vector2.Lerp(range.start, range.end, (float)i / _rDiv);
                index++; rhit = Physics2D.Raycast(rpos, direction, distance, layerMask);
                _rHits[i] = rhit;
                if (!rhit.collider)
                { Debug.DrawRay(rpos, direction * distance, Color.white);  continue; }
                _hitCount++;
                if (rhit.distance < _hitDist)
                {
                    _hitIndex = index;
                    _hitDist = rhit.distance;
                }
                _rHits[i] = rhit;
                Debug.DrawRay(rpos, direction * _hitDist, Color.red);
            }
            _castCount = index + 1;
            _hitBDist = _hitDist - _baseDist;
            return _hitCount; 
        }

        public IEnumerable<RaycastHit2D> CastRays(Vector2 origin, Vector2 direction, Vector2Range range, float distance)
        {
            _cPoint = origin;
            ResetValues(distance);
            RaycastHit2D rhit;
            int i; for (i = 0; i < _rayCount; i++)
            {
                rhit = Physics2D.Raycast(origin + Vector2.Lerp(range.start, range.end, (float)i / _rDiv), direction, distance, layerMask);
                _castCount++;
                if (rhit.collider)
                {
                    _hitCount++;
                    if (rhit.distance < _hitDist)
                    {
                        _hitIndex = i;
                        _hitDist = rhit.distance;
                        _hitBDist = _hitDist - _baseDist;
                    }
                }
                yield return _rHits[i] = rhit;
            }
        }

        void ResetValues(float distance)
        {
            _castCount = 0;
            _castDist = distance;
            _hitCount = 0;
            _hitIndex = 0;
            _hitDist = distance;
            _hitBDist = _hitDist - _baseDist;
        }

    }

}





/*
IEnumerable<RaycastHit2D> CastRays(RaycastHit2D[] rhits, Vector2 origin, Vector2 dir, Vector2Range range, float dist, LayerMask layerMask)
{
    Vector2 point; int arrLen = rhits.Length, arrLen1 = arrLen > 2 ? arrLen - 1 : 1;
    for (int i = 0; i < arrLen; i++)
    {
        point = origin + Vector2.Lerp(range.start, range.end, (float)i / arrLen1);
        yield return rhits[i] = Physics2D.Raycast(point, dir, dist, layerMask);
    }
}
IEnumerable<RaycastHit2D> CastRays(int count, Vector2 origin, Vector2 dir, Vector2Range range, float dist, LayerMask layerMask)
{
    Vector2 point;
    for (int i = 0; i < count; i++)
    {
        point = origin + Vector2.Lerp(range.start, range.end, (float)i / (count - 1));
        yield return Physics2D.Raycast(point, dir, dist, layerMask);
    }
}
IEnumerable<Vector2> GetRayPositions(int count, Vector2Range range)
{
    for (int i = 0; i < count; i++)
    { yield return Vector2.Lerp(range.start, range.end, (float)i / (count - 1)); }
}
*/

