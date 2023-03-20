using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityControl_ : Entity
{

    bool _isAbsolute = false;
    public bool isAbsolute { get => _isAbsolute; set => _isAbsolute = value; }

    public static float gravity = 9.81f;
    public float gravityMult = 1;
    public float ground_accel, ground_break, air_accel, air_break;
    public float temporalGravityMult = 1;
    float _mass;
    public float mass { get => _mass; }

    public Vector2 stepSize = new Vector2(0.1f,1f);

    float collDistance = 0.04f, detectDistance = 0.05f;

    [SerializeField] Rigidbody2D rb;
    [SerializeField] BoxCollider2D bcoll;
    [SerializeField] protected Dictionary<string, ColliderStateGroup> stateGroups;
    [SerializeField] [HideInInspector] protected ColliderStateGroup activeStateGroup;
    [SerializeField] protected string defaultStateGroup;
    public LayerMask groundLayer;
    public bool isGrounded { get; protected set; }
    public float wasGrounded { get; protected set; }
    public bool wallLeft { get; protected set; }
    public bool wallRight { get; protected set; }

    Vector2 targetMove, addMove;
    public Vector2 velocity = Vector2.zero;
    Vector2 lastPosition = Vector2.zero;
    Vector2 groundVelocity = Vector2.zero;
    Vector2 VelocityClamp = new Vector2(15, 15);

    //Vector2 VelocityClamp = new Vector2(15, 15);
    EasedVector2 MoveVelocity = new EasedVector2(5, 5);

    RaycastGroup rcLeft { get => activeStateGroup.rcLeft; }
    RaycastGroup rcRight { get => activeStateGroup.rcRight; }
    RaycastGroup rcTop { get => activeStateGroup.rcTop; }
    RaycastGroup rcBottom { get => activeStateGroup.rcBottom; }
    float ASG_ox { get => activeStateGroup.castOffset.x; }
    float ASG_oy { get => activeStateGroup.castOffset.x; }

    List<EntityControl_> attachObjects = new();

    public void Move(Vector2 move)
    { targetMove += move; }
    public void Move(float move)
    { targetMove.x += move; }



    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        bcoll = GetComponent<BoxCollider2D>();

        PhysicsMaterial2D pmat = new PhysicsMaterial2D();
        pmat.bounciness = 0;
        pmat.friction = 0;
        rb.sharedMaterial = pmat;
        lastPosition = transform.position;

        SetupStateGroups();
    }

    private void Awake()
    {
        lastPosition = transform.position;
    }


    protected virtual void Update()
    {
        //CastGroundRays();
        UpdateMove();
    }


    void FetchAttributes()
    {
        if (entityStats != null)
        {
            
        }
    }



    void CastGroundRays()
    {
        //rcBottom.Cast(transform.position, detectDistance, groundLayer);
        activeStateGroup.CastBottom(transform.position, detectDistance, groundLayer);
        if (rcBottom.hitCount > 0)
        {
            isGrounded = true;
            wasGrounded = 0f;
            return;
        }
        isGrounded = false;
        wasGrounded += Time.deltaTime;
    }

    void UpdateMove()
    {
        Vector2 v = velocity;
        float maccel = targetMove.x != 0 ? 50f : 25f;
        
        CastGroundRays();
        CalcAttachedMovement();
        targetMove += groundVelocity;
        //CastWallRays();


        if (isGrounded)
        { 
            v.y = Mathf.Max(0, v.y); 
        }
        else
        { 
            v.y -= gravity * temporalGravityMult * 4 * gravityMult * Time.deltaTime; 
            maccel *= targetMove.x == 0 ? 0f : 0.25f; 
        }

        if (ComparePolarity(targetMove.x, v.x) != 0)
        { maccel *= 2; }
        v.x = TowardsTargetValue(v.x, targetMove.x, maccel * Time.deltaTime);

        Vector2 maxmove = v * Time.deltaTime;
        if (v.x > 0)
        {
            //rcRight.Cast(transform.position, maxmove.x + detectDistance, groundLayer);
            activeStateGroup.CastRight(transform.position, maxmove.x + detectDistance, groundLayer);
            wallRight = rcRight.hitCount > 2 && rcRight.shortDistance <= detectDistance + ASG_ox;
            if (!wallRight && rcRight[0].collider && CheckStep(maxmove.x, out Vector2 steppos))
            { addMove += steppos; }
            else
            { maxmove.x = Mathf.Min(maxmove.x + ASG_ox, rcRight.shortDistance - collDistance) - ASG_ox; }

            //rcLeft.Cast(transform.position, detectDistance, groundLayer);
            activeStateGroup.CastLeft(transform.position, detectDistance, groundLayer);
            wallLeft = rcLeft.hitCount > 2;
        }
        else if(v.x < 0)
        {
            //rcLeft.Cast(transform.position, -maxmove.x+detectDistance, groundLayer);
            activeStateGroup.CastLeft(transform.position, -maxmove.x + detectDistance, groundLayer);
            wallLeft = rcLeft.hitCount > 2 && rcLeft.shortDistance <= detectDistance + ASG_ox;
            if (!wallLeft && rcLeft[0].collider && CheckStep(maxmove.x, out Vector2 steppos))
            { addMove += steppos; }
            else
            { maxmove.x = Mathf.Max(maxmove.x - ASG_ox, -rcLeft.shortDistance + collDistance) + ASG_ox; }

            //rcRight.Cast(transform.position, detectDistance, groundLayer);
            activeStateGroup.CastRight(transform.position, detectDistance, groundLayer);
            wallRight = rcRight.hitCount > 2;
        }
        else
        {
            //rcLeft.Cast(transform.position, detectDistance, groundLayer);
            activeStateGroup.CastLeft(transform.position, detectDistance, groundLayer);
            wallLeft = rcLeft.hitCount > 2;
            //rcRight.Cast(transform.position, detectDistance, groundLayer);
            activeStateGroup.CastRight(transform.position, detectDistance, groundLayer);
            wallRight = rcRight.hitCount > 2;
        }


        if (v.y > 0)
        {
            //rcTop.Cast(transform.position, maxmove.y+detectDistance, groundLayer);
            activeStateGroup.CastTop(transform.position, maxmove.y + detectDistance, groundLayer);
            maxmove.y = Mathf.Min(maxmove.y + ASG_oy, rcTop.shortDistance - collDistance) - ASG_oy;
        }
        else if(v.y < 0)
        {
            //rcBottom.Cast(transform.position, -maxmove.y + detectDistance, groundLayer);
            activeStateGroup.CastBottom(transform.position, -maxmove.y + detectDistance, groundLayer);
            maxmove.y = Mathf.Max(maxmove.y - ASG_oy, -rcBottom.shortDistance + collDistance) + ASG_oy;
        }

        //Vector2 mmove = new Vector2(Mathf.Clamp(maxmove.x / Time.deltaTime, -VelocityClamp.x, VelocityClamp.x), Mathf.Clamp(maxmove.y / Time.deltaTime, -VelocityClamp.y, -VelocityClamp.y));
        lastPosition = (Vector2)transform.position + addMove;
        transform.position += (Vector3)(maxmove + addMove);// * Time.deltaTime;
        velocity = ((Vector2)transform.position - lastPosition) / Time.deltaTime;

        temporalGravityMult = 1f;
        targetMove = Vector2.zero;
        addMove = Vector2.zero;
    }

  
    void CalcAttachedMovement()
    {
        groundVelocity = Vector2.zero;
        for (int i = 0; i < attachObjects.Count; i++)
        { groundVelocity += attachObjects[i].velocity; }
    }


    bool CheckStep(float x, out Vector2 point)
    {
        point = Vector2.zero;
        if (x > 0)
        {
            Vector2 tpos = transform.position;
            if (!Physics2D.Raycast(tpos + rcRight.lastPoint, Vector2.right, stepSize.x + detectDistance, groundLayer).collider)
            {
                Vector2 vec = tpos + rcRight.lastPoint + new Vector2(stepSize.x, 0);
                float d = rcRight.pointsRangeAbs.y + detectDistance;
                RaycastHit2D rhit = Physics2D.Raycast(vec, Vector2.down, d, groundLayer);
                if (rhit.collider && rhit.distance >= d - stepSize.y)
                { point = new Vector2(x, d - rhit.distance); return true; }
            }
        }
        else if (x < 0)
        {
            Vector2 tpos = transform.position;
            if (!Physics2D.Raycast(tpos + rcLeft.lastPoint, Vector2.left, stepSize.x + detectDistance, groundLayer).collider)
            {
                Vector2 vec = tpos + rcLeft.lastPoint + new Vector2(-stepSize.x, 0);
                float d = rcLeft.pointsRangeAbs.y + detectDistance;
                RaycastHit2D rhit = Physics2D.Raycast(vec, Vector2.down, d, groundLayer);
                if (rhit.collider && rhit.distance >= d - stepSize.y)
                { point = new Vector2(x, d - rhit.distance); return true; }
            }
        }
        return false;
    }

    public bool SetStateGroup(string group_name)
    {
        if (stateGroups.TryGetValue(group_name, out ColliderStateGroup sgroup))
        {
            activeStateGroup = sgroup;
            bcoll.offset = sgroup.rect.position;
            bcoll.size = sgroup.rect.size;
            return true;
        }
        return false;
    }
    void SetupStateGroups()
    {
        if (stateGroups == null || stateGroups.Count == 0)
        {
            stateGroups = new();
            Rect rect = RectFromCollider(bcoll);
            RaycastGroupsFromRect(rect, true, 5, 5, 5, 5, out RaycastGroup gleft, out RaycastGroup gright, out RaycastGroup gtop, out RaycastGroup gbottom);
            stateGroups.Add("idle", new ColliderStateGroup("idle", rect, rect.size/2, gleft, gright, gtop, gbottom));
            activeStateGroup = stateGroups["idle"];
        }
        List<string> keys = new();
        foreach (string k in stateGroups.Keys)
        { keys.Add(k); }
        foreach (string k in keys)
        {
            RaycastGroupsFromRect(stateGroups[k].rect, true, 5, 5, 5, 5, out RaycastGroup gleft, out RaycastGroup gright, out RaycastGroup gtop, out RaycastGroup gbottom);
            stateGroups[k] = new ColliderStateGroup(k, stateGroups[k].rect, stateGroups[k].rect.size/2, gleft, gright, gtop, gbottom);
        }
    }
    void RaycastGroupsFromRect(Rect rect, bool centered, int left_count, int right_count, int top_count, int bottom_count, out RaycastGroup left, out RaycastGroup right, out RaycastGroup top, out RaycastGroup bottom)
    {
        if (centered)
        {
            GetRectEdgeCenters(rect, out Vector2 vl, out Vector2 vr, out Vector2 vt, out Vector2 vb);
            left = new RaycastGroup(left_count, Vector2.left, vb, vt);
            right = new RaycastGroup(right_count, Vector2.right, vb, vt);
            top = new RaycastGroup(top_count, Vector2.up, vl, vr);
            bottom = new RaycastGroup(bottom_count, Vector2.down, vl, vr);
            return;
        }

        GetRectCorners(rect, out Vector2 vtl, out Vector2 vtr, out Vector2 vbl, out Vector2 vbr);
        left = new RaycastGroup(left_count, Vector2.left, vbl, vtl);
        right = new RaycastGroup(right_count, Vector2.right, vbr, vtr);
        top = new RaycastGroup(top_count, Vector2.up, vtl, vtr);
        bottom = new RaycastGroup(bottom_count,  Vector2.down, vbl, vbr);
    }

    bool ValueSamePolarity(float a, float b)
    { return ComparePolarity(a, b) == 0; }
    int ComparePolarity(float a, float b)
    {
        if (a <= 0 && b > 0) return 1;
        if (a >= 0 && b < 0) return -1;
        return 0;
    }

    public float TowardsTargetValue(float value, float target, float add)
    {
        float diff = target - value;
        if (diff == 0) return value;
        if (diff < 0)
        { return value + Mathf.Max(-add, diff); }
        return value + Mathf.Min(add, diff);
    }
    public Vector2 TowardsTargetVector(Vector2 value, Vector2 target, Vector2 add)
    { return new Vector2(TowardsTargetValue(value.x, target.x, add.x), TowardsTargetValue(value.y, target.y, add.y)); }

    Vector2 VectorMaxDim(params Vector2[] v)
    {
        if (v.Length == 0) return Vector2.zero;
        Vector2 vec = v[0];
        for (int i = 1; i < v.Length; i++)
        { vec.x = Mathf.Max(vec.x, v[i].x); vec.y = Mathf.Max(vec.y, v[i].y); }
        return vec;
    }
    Vector2 VectorMinDim(params Vector2[] v)
    {
        if (v.Length == 0) return Vector2.zero;
        Vector2 vec = v[0];
        for (int i = 1; i < v.Length; i++)
        { vec.x = Mathf.Min(vec.x, v[i].x); vec.y = Mathf.Min(vec.y, v[i].y); }
        return vec;
    }

    Vector2 VectorClampDim(Vector2 value, Vector2 clamp)
    {
        Vector2 c = VectorAbs(clamp);
        return new Vector2(
            Mathf.Clamp(value.x, -c.x, c.x),
            Mathf.Clamp(value.y, -c.y, c.y));
    }
    Vector2 VectorClampDim(Vector2 value, Vector2 clampMin, Vector2 clampMax)
    {
        Vector2 min = clampMin, max = clampMax;
        if (clampMin.x > clampMax.x) { min.x = clampMax.x; max.x = clampMin.x; }
        if (clampMin.y > clampMax.y) { min.y = clampMax.y; max.y = clampMin.y; }
        return new Vector2(
            Mathf.Clamp(value.x, min.x, max.x), 
            Mathf.Clamp(value.x, min.y, max.y)); 
    }
    Vector2 VectorAbs(Vector2 value)
    { return new Vector2(Mathf.Abs(value.x), Mathf.Abs(value.y)); }

    Vector2[] GetRectCorners(Rect rect)
    {
        return new Vector2[]
        {
            new Vector2(rect.xMin, rect.yMax),
            new Vector2(rect.xMax, rect.yMax),
            new Vector2(rect.xMax, rect.yMin),
            new Vector2(rect.xMin, rect.yMin)
        };
    }
    void GetRectCorners(Rect rect, out Vector2 top_left, out Vector2 top_right, out Vector2 bottom_left, out Vector2 bottom_right)
    {
        top_left = new Vector2(rect.xMin, rect.yMax);
        top_right = new Vector2(rect.xMax, rect.yMax);
        bottom_left = new Vector2(rect.xMin, rect.yMin);
        bottom_right = new Vector2(rect.xMax, rect.yMin);
    }
    void GetRectEdgeCenters(Rect rect, out Vector2 left, out Vector2 right, out Vector2 top, out Vector2 bottom)
    {
        left = new Vector2(rect.xMin, rect.center.y);
        right = new Vector2(rect.xMax, rect.center.y);
        top = new Vector2(rect.yMax, rect.center.x);
        bottom = new Vector2(rect.yMin, rect.center.x);
    }
    Rect RectFromCollider(BoxCollider2D collider)
    { return new Rect(bcoll.offset - bcoll.size / 2, bcoll.size); }


    public static bool CompareMass(EntityControl_ a, EntityControl_ b)
    {
        return false;
    }


    public class EasedVector2
    {
        Vector2 vScale = Vector2.one, vDelta = Vector2.zero;
        const float PiHalf = Mathf.PI / 2;
        delegate void SetDimDelegate(float value);
        SetDimDelegate cSetX, cSetY;

        public float x { get => GetX(vDelta.x) * vScale.x; set => cSetX(value); }
        public float y { get => GetY(vDelta.y) * vScale.y; set => cSetY(value); }
        public float xDelta { get => vDelta.x; set => vDelta.x = value; }
        public float yDelta { get => vDelta.y; set => vDelta.y = value; }
        
        public Vector2 vector { get => new Vector2(x, y); }
        public Vector2 vectorUnscaled { get => new Vector2(GetX(vDelta.x), GetY(vDelta.y)); }
        public Vector2 vectorDelta { get => vDelta; set => vDelta = value; }
        public Vector2 vectorScale { get => vScale; set => SetScale(value); }

        public EasedVector2(float xScale = 1f, float yScale = 1f)
        { SetScaleX(xScale); SetScaleY(yScale); }
        public EasedVector2(Vector2 scale)
        { SetScale(scale); }


        float GetX(float d)
        { return Delta2ValueR1(d); }
        float GetY(float d)
        { return Delta2ValueR1(d); }
        void SetX(float v)
        { vDelta.x = Value2DeltaR1(v/vScale.x); }
        void SetY(float v)
        { vDelta.y = Value2DeltaR1(v/vScale.y); }

        void SetScaleX(float v)
        { 
            vScale.x = v;
            if (vScale.x == 0) cSetX = (v) => { vDelta.x = 0; };
            else cSetX = SetX;
        }
        void SetScaleY(float v)
        {
            vScale.y = v;
            if (vScale.y == 0) cSetY = (v) => { vDelta.y = 0; };
            else cSetY = SetY;
        }
        void SetScale(Vector2 v)
        {
            SetScaleX(v.x);
            SetScaleY(v.y);
        }

        float Delta2ValueR1(float d)
        { return Mathf.Sin(PiHalf * d); }
        float Value2DeltaR1(float v)
        { return Mathf.Sin(v); }

        public static implicit operator Vector2(EasedVector2 vec)
        { return new Vector2(vec.x, vec.y); }
    }







    [System.Serializable]
    public class ColliderStateGroup
    {
        public string name;
        public Rect rect;
        public Vector2 castOffset;
        public RaycastGroup rcLeft, rcRight, rcTop, rcBottom;
        public ColliderStateGroup(string name, Rect rect, Vector2 offset, RaycastGroup rc_left, RaycastGroup rc_right, RaycastGroup rc_top, RaycastGroup rc_bottom)
        {
            this.name = name;
            this.rect = rect;
            this.castOffset = offset;
            rcLeft = rc_left;
            rcRight = rc_right;
            rcTop = rc_top;
            rcBottom = rc_bottom;
        }

        public int CastLeft(Vector2 offset, float distance, LayerMask layerMask)
        { return rcLeft.Cast(offset + castOffset, distance + castOffset.x, layerMask); }
        public int CastRight(Vector2 offset, float distance, LayerMask layerMask)
        { return rcLeft.Cast(offset + castOffset, distance + castOffset.x, layerMask); }
        public int CastTop(Vector2 offset, float distance, LayerMask layerMask)
        { return rcLeft.Cast(offset + castOffset, distance + castOffset.y, layerMask); }
        public int CastBottom(Vector2 offset, float distance, LayerMask layerMask)
        { return rcLeft.Cast(offset + castOffset, distance + castOffset.y, layerMask); }
    }







    public class RaycastGroup
    {
        Vector2[] _points;
        RaycastHit2D[] _rayHits;
        public Vector2[] points { get => _points; }
        public RaycastHit2D[] rayHits { get => _rayHits; }
        public Vector2 firstPoint { get; protected set; }
        public Vector2 lastPoint { get; protected set; }
        public Vector2 pointsRange { get; protected set; }
        public Vector2 pointsRangeAbs { get; protected set; }


        public int hitCount { get; protected set; }
        public List<int> hitIndexes { get; protected set; }
        public int shortIndex { get; protected set; }
        public float shortDistance { get; protected set; }
        public Vector2 shortDistanceVector { get => rayHitShort.point - rayHitShort.centroid; }
        public RaycastHit2D rayHitShort { get => rayHits[shortIndex]; }

        public Vector2 castDirection = Vector2.right;



        public RaycastGroup(Vector2 castDirection, params Vector2[] castPoints)
        {
            this.castDirection = castDirection;
            _points = castPoints;
            _rayHits = new RaycastHit2D[_points.Length];
            firstPoint = _points[0];
            lastPoint = _points[_points.Length - 1];
            pointsRange = lastPoint - firstPoint;
            pointsRangeAbs = new Vector2(Mathf.Abs(pointsRange.x), Mathf.Abs(pointsRange.y));
        }
        public RaycastGroup(int count, Vector2 castDirection, Vector2 start_point, Vector2 end_point)
        {
            this.castDirection = castDirection;
            count = Mathf.Max(count, 0);
            _points = new Vector2[Mathf.Max(count)];
            _rayHits = new RaycastHit2D[_points.Length];

            Vector2 diff = end_point - start_point;
            Vector2 jump = diff / Mathf.Max(count - 1, 1);
            for (int i = 0; i < count; i++)
            { _points[i] = start_point + jump * i; }

            firstPoint = _points[0];
            lastPoint = _points[_points.Length - 1];
            pointsRange = lastPoint - firstPoint;
            pointsRangeAbs = new Vector2(Mathf.Abs(pointsRange.x), Mathf.Abs(pointsRange.y));
        }

        //public int Cast(Vector2 offset, Vector2 direction, float distance, LayerMask layerMask)
        //{ return Cast(offset, direction, distance, layerMask, out _); }

        public int Cast(Vector2 offset, float distance, LayerMask layerMask)//, out List<RaycastHit2D> hits_out)
        {
            hitCount = 0;
            shortIndex = 0;
            shortDistance = distance;
            hitIndexes = new();
            //hits_out = new();
            for (int i = 0; i < _points.Length; i++)
            {
                Vector2 p = _points[i] + offset;
                RaycastHit2D rhit = Physics2D.Raycast(p, castDirection, distance, layerMask);
                _rayHits[i] = rhit;

                float ddistance = distance;
                Color dcolor = Color.white;
                if (rhit.collider)
                {
                    hitCount++;
                    if (rhit.distance < shortDistance)
                    { shortDistance = rhit.distance; shortIndex = i; }
                    hitIndexes.Add(i);
                    //hits_out.Add(rhit);
                    ddistance = rhit.distance;
                    dcolor = Color.red;
                }
                Debug.DrawRay(p, castDirection * ddistance, dcolor);
            }
            return hitCount;
        }


        public RaycastHit2D this[int index]
        { get => rayHits[index]; }

    }

}


/*
 if (isGrounded)
        {
            maxMove *= Time.deltaTime;
            if (v.x > 0)
            {
                rcRight.Cast(transform.position, Vector2.right, maxMove.x, groundLayer);
                maxMove.x = Mathf.Min(maxMove.x, rcRight.shortDistance);
            }
            else if (v.x < 0)
            {
                rcLeft.Cast(transform.position, Vector2.left, maxMove.x, groundLayer);
                maxMove.x = Mathf.Min(maxMove.x, rcLeft.shortDistance);
            }
            v.y = Mathf.Max(0, v.y);
        }
        else
        {
            v.y -= gravity * gravityMult * Time.deltaTime;
            maxMove = v * Time.deltaTime;
            if (v.x > 0)
            {
                rcRight.Cast(transform.position, Vector2.right, maxMove.x, groundLayer);
                maxMove.x = Mathf.Min(maxMove.x, rcRight.shortDistance);
            }
            else if (v.x < 0)
            {
                rcLeft.Cast(transform.position, Vector2.left, maxMove.x, groundLayer);
                maxMove.x = Mathf.Max(maxMove.x, rcLeft.shortDistance);
            }

            if (v.y > 0)
            {
                rcTop.Cast(transform.position, Vector2.up, maxMove.y, groundLayer);
                maxMove.y = Mathf.Min(maxMove.y, rcTop.shortDistance);
            }
            else if (v.y < 0)
            {
                rcBottom.Cast(transform.position, Vector2.down, maxMove.y, groundLayer);
                maxMove.y = Mathf.Max(maxMove.y, rcBottom.shortDistance);
            }
        }
 */