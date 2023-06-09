using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityUtil;
using static Utility;

public class EntityController : Entity
{

    Rigidbody2D rb;
    protected BoxCollider2D coll;
    protected bool simulationEnabled = true;

    Rect rect;
    Vector2 r_sz2;
    bool rectChanged = false;

    protected LayerMask groundMask, platformMask, gpMask;

    RayGroup2D rgLeft, rgRight, rgUp, rgDown;

    protected bool isGrounded, isPlatform, ignorePlatform, wallLeft, wallRight, wallUp, ledgeLeft, ledgeRight;
    float groundDist, roofDist;
    protected float wasGrounded = 0;

    Vector2 groundVelocity = Vector2.zero, _relativeVelocity = Vector2.zero, targetMove;
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
    }

    protected override void Start()
    {
        base.Start();
    }

    protected virtual void FixedUpdate()
    {
        CheckRect();

        if (!simulationEnabled) return;

        maxMove.SetAll(1f);
        HandleCollisions();

        HandleMovement();

        HandlePosition();

        HandleEnd();
    }


    protected void Move(float x = 0, float y = 0)
    { targetMove.x = Mathf.Clamp(x, -1, 1); targetMove.y = Mathf.Clamp(y, -1, 1); }
    protected void Move(Vector2 xy)
    { targetMove.x = Mathf.Clamp(xy.x, -1, 1); targetMove.y = Mathf.Clamp(xy.y, -1, 1); }



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

        rgDown = new RayGroup2D(5, groundMask);
        rgDown.baseDistance = 0.25f;
        rgDown.doffset = r_sz2.y - rgDown.baseDistance - rect.position.y;
        groundDist = r_sz2.y - rect.position.y;

        rgUp = new RayGroup2D(3, groundMask);
        rgUp.baseDistance = r_sz2.y;
        rgUp.doffset = r_sz2.y - rgUp.baseDistance + rect.position.y;
        roofDist = r_sz2.y + rect.position.y;
    }
    void UpdateRayGroups()
    {
        rgLeft.baseDistance = r_sz2.x;
        rgRight.baseDistance = r_sz2.x;

        rgDown.doffset = r_sz2.y - rgDown.baseDistance - rect.position.y;
        groundDist = r_sz2.y - rect.position.y;

        rgUp.doffset = r_sz2.y - rgUp.baseDistance + rect.position.y;
        roofDist = r_sz2.y + rect.position.y;
    }


    protected void HandleEnd()
    {
        _relativeVelocity = _velocity - groundVelocity;
        targetMove = Vector2.zero;
    }

    protected void CheckRect()
    {
        if (rectChanged)
        {
            UpdateRect();
            UpdateRayGroups();
            rectChanged = false;
        }
    }



    protected virtual void HandleMovement()
    { _velocity += targetMove; }


    void HandleCollisions()
    {
        Vector3 tpos = transform.position;
        Vector2Range vecrang = GetRayRange(transform.position.Add(y: -rgDown.doffset), Vector2.right, r_sz2.x, groundMask, -0.05f);

        groundVelocity = Vector2.zero;
        // ground check
        rgDown.layerMask = !ignorePlatform ? ((isGrounded || _velocity.y <= 0) ? gpMask : groundMask) : groundMask;
        if (rgDown.Cast(transform.position, Vector2.down, vecrang, rgDown.baseDistance + 0.05f + Mathf.Max(-velocity.y * Time.fixedDeltaTime * 2, 0)) > 0)
        {
            if (rgDown.shortHit.point.y + groundDist + 0.01f >= tpos.y)
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
                tpos.y = rgDown.shortHit.point.y + groundDist;//r_sz2.y; //Mathf.Max(tpos.y, rgDown.shortHit.point.y + rHalf.y);
                transform.position = tpos;
                if (!isGrounded)
                { OnEnterGrounded(_velocity, 0f); }
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
        if (rgUp.Cast(transform.position, Vector2.up, vecrang, rgUp.baseDistance + Mathf.Max(velocity.y * Time.fixedDeltaTime * 2, 0)) > 0)
        {
            if (rgUp.hitCount > 1 && rgUp.shortHit.point.y - roofDist - 0.01f <= tpos.y)
            {
                wallUp = true;
                tpos.y = Mathf.Min(tpos.y, rgUp.shortHit.point.y - roofDist);
                transform.position = tpos;
                _velocity.y = Mathf.Min(_velocity.y, 0);
                maxMove.up = rgUp.hitBaseDistance;
            }
        }

        wallLeft = false;
        vecrang = GetRayRange(transform.position.Add(x: -rgLeft.doffset, y: rect.position.y), Vector2.up, r_sz2.y, groundMask, -0.05f);
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
        vecrang = GetRayRange(transform.position.Add(x: rgRight.doffset, y: rect.position.y), Vector2.up, r_sz2.y, groundMask, -0.05f);
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
        RaycastHit2D rhit = Physics2D.Raycast(tpos, Vector2.down, groundDist - 0.05f, groundMask);
        if (rhit.collider)
        { transform.position = new Vector3(tpos.x, rhit.point.y + groundDist, tpos.z); }
    }



    protected virtual void OnEnterGrounded(Vector2 velocity, float fallDamageDelta)
    { /*Debug.Log($"OnEnterGrounded({velocity}, {fallDamageDelta})");*/ }
    protected virtual void OnExitGrounded()
    { /*Debug.Log("OnExitGrounded()");*/ }

    protected virtual void OnTouchWallLeft(Vector2 velocity)
    { /*Debug.Log("OnTouchWallLeft()");*/ }
    protected virtual void OnTouchWallRight(Vector2 velocity)
    { /*Debug.Log("OnTouchWallRight()");*/ }


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
                { Debug.DrawRay(rpos, direction * distance, Color.white); continue; }
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

