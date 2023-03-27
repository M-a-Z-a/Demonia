
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
    Rect rect; 
    bool rectChanged = false;

    [SerializeField] LayerMask groundMask;

    [Range(0f, 1f)]
    [SerializeField] float attackIn, attackOut, releaseIn, releaseOut;


    EntityStats.Attribute speed, jumpForce, airJumpForce, fallSpeedCap, fallDamageTreshold, speedApexMult;
    float xSpeedMult = 1f, gravityMult = 1f;

    RayGroup2D rgLeft, rgRight, rgUp, rgDown;

    protected float wasGrounded = 0;
    
    
    public bool isGrounded { get; protected set; }
    Vector2 targetMove = Vector2.zero;

    private void OnDrawGizmos()
    {
        velAttack = new CurveSlider(attackIn, attackOut);
        velRelease = new CurveSlider(releaseIn, releaseOut);
        Gizmos.color = Color.green;
        rect.DrawGizmo(transform.position);

        //velAttack.TESTVAL();
        //velRelease.TESTVAL();
        Gizmos.color = Color.green;
        velAttack.DrawGizmo(Vector2.zero, new Vector2(1, 2), 20);
        Gizmos.color = Color.yellow;
        velRelease.DrawGizmo(new Vector2(1, 2), new Vector2(1, -2), 10);
    }

    
    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        SetRayGroups();

        speed = entityStats.GetSetAttribute("speed", 10f);
        speedApexMult = entityStats.GetSetAttribute("speedapexmult", 2f);
        jumpForce = entityStats.GetSetAttribute("jumpforce", 10f);
        airJumpForce = entityStats.GetSetAttribute("airjumpforce", 5f);
        fallSpeedCap = entityStats.GetSetAttribute("fallSpeedCap", 100f);
        fallDamageTreshold = entityStats.GetSetAttribute("fallDamageTreshold", 50f);
    }

    protected override void Start()
    {
        base.Start();
    }


    void SetRayGroups()
    {
        rect = coll.Rect();
        Vector2 szhalf = rect.size / 2;

        rgLeft = new RayGroup2D(5, groundMask); rgLeft.baseDistance = szhalf.x;
        rgRight = new RayGroup2D(5, groundMask); rgRight.baseDistance = szhalf.x;
        rgUp = new RayGroup2D(5, groundMask); rgUp.baseDistance = 0.25f;
        rgDown = new RayGroup2D(5, groundMask); rgDown.baseDistance = 0.25f;
    }



    protected virtual void Update()
    {
        
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

        _velocity.y = Mathf.Max(fallSpeedCap, _velocity.y);
        Vector3 tpos = transform.position.Add(_velocity.x * Time.deltaTime, _velocity.y * Time.deltaTime);
        transform.position = tpos;
        rectChanged = false;
        targetMove = Vector2.zero;
    }


    void UpdateRect()
    { rect = coll.Rect(); rectChanged = true; }

    void HandleGroundedMove()
    {
        _velocity.y = 0;
    }

    void HandleAerialMove()
    {

    }

    void HandleGravity()
    {
        gravityMult = velocity.y < 0 ? 2f : 1f;
        _velocity.y -= 9.81f * gravityMult * Time.deltaTime;
    }

    void HandlePositionFix()
    {

    }

    void HandleCollisions()
    {
        Vector2 rHalf = rect.GetHalfSize();
        Vector2Range vecrang = GetRayRange(transform.position.Add(y:-rHalf.y + 0.25f), Vector2.right, rHalf.x, groundMask, -0.05f);

        if (rgDown.Cast(transform.position, Vector2.down, vecrang, 0.25f + 0.05f + Mathf.Max(-velocity.y * Time.deltaTime, 0)) > 0)
        {
            if (!isGrounded) 
            {
                if (_velocity.y < -fallDamageTreshold)
                {
                    float rang = fallDamageTreshold - fallSpeedCap;
                    OnEnterGrounded(rang > 0 ? (Mathf.Abs(_velocity.y) - fallDamageTreshold) / rang : 1f );
                }
                else
                { OnEnterGrounded(0f); }
            }
            isGrounded = true; wasGrounded = 0f;
        }
        else 
        { 
            if (isGrounded)
            { OnExitGrounded(); }
            isGrounded = false; wasGrounded += Time.deltaTime; 
        }
    }


    protected virtual void OnEnterGrounded(float fallDamageDelta) { }
    protected virtual void OnExitGrounded() { }



    Vector2Range GetRayRange(Vector2 origin, Vector2 dir_from_origin, float dist_from_origin, LayerMask layerMask, float padding = -0.05f)
    {
        RaycastHit2D rhit;
        return new(GetPoint(-dir_from_origin, padding), GetPoint(dir_from_origin, padding));

        Vector2 GetPoint(Vector2 dir, float add_padding)
        {
            rhit = Physics2D.Raycast(origin, dir, dist_from_origin, layerMask);
            if (rhit.collider) return origin + dir * (dist_from_origin + add_padding);
            return origin + dir * dist_from_origin;
        }
    }


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

    IEnumerator IJumpBoost(float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator IJumpApexBoost(float multStart, float multEnd, float time)
    {
        float t = 0;
        xSpeedMult = multStart;
        while (t < time)
        {
            t += Time.deltaTime;
            xSpeedMult = Mathf.Lerp(multStart, multEnd, t / time);
            yield return null;
        }
        xSpeedMult = multEnd;
    }

    

    IEnumerator IWaitActions(int updates, Action onStart, Action onEnd, Action<int, float> onLoop = null)
    {
        onStart?.Invoke();
        int u = 0;
        if (onLoop == null)
        {
            while (u < updates)
            { u++; yield return null; }
        }
        else
        {
            while (u < updates)
            {
                onLoop(u, (float)u / updates); u++;
                yield return null;
            }
        }
        onEnd?.Invoke();
    }
    IEnumerator IWaitActions(float time, Action onStart, Action onEnd, Action<float, float> onLoop = null)
    {
        onStart?.Invoke();
        if (onLoop == null)
        { yield return new WaitForSeconds(time); }
        else
        {
            float t = 0;
            while (t < time)
            {
                onLoop(t, t / time);
                t += Time.deltaTime;
                yield return null;
            }
        }
        onEnd?.Invoke();
    }


    struct Vector2Range
    {
        public readonly Vector2 start, end;
        public Vector2Range(Vector2 start = default, Vector2 end = default)
        { this.start = start; this.end = end; }
    }

    class RayGroup2D
    {
        protected RaycastHit2D[] _rHits;
        protected float _hitDist, _hitBDist, _castDist, _baseDist;
        protected int _hitIndex, _hitCount, _castCount, _rayCount;
        private int _rDiv;

        public LayerMask layerMask;


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
                Debug.DrawRay(rpos, direction * _hitDist, Color.red);
            }
            _castCount = index + 1;
            _hitBDist = _hitDist - _baseDist;
            return _hitCount; 
            
        }

        public IEnumerator<RaycastHit2D> CastRays(Vector2 origin, Vector2 direction, Vector2Range range, float distance)
        {
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
