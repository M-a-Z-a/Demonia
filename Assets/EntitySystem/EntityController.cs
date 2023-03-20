using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityMoverClasses;
using static Utility;

public class EntityController : Entity
{

    const float PiHalf = Mathf.PI / 2;

    public LayerMask groundMask;
    Rigidbody2D rb;
    BoxCollider2D bcoll;
    Rect rect;

    Vector2 targetMove, addMove, detectDist, collDist, collHalf;

    RaycastGroup rcLeft, rcRight, rcUp, rcDown;
    RayHits rhLeft, rhRight, rhUp, rhDown;

    EntityStats.Attribute speed, accel;

    public bool isGrounded = true, wallLeft = false, wallRight = false, wallTop = false;
    public float wasGrounded = 0f, gravityScale = 1f, temporalGravityMult = 1f;

    
    Vector2 _gVelocity = Vector2.zero;

    public new Vector2 velocity { get => _velocity; set => _velocity = value; }
    public Vector2 groundVelocity { get => _gVelocity; }
    public float xVelocity { get => _velocity.x; set => _velocity = new Vector2(value, _velocity.y); }
    public float yVelocity { get => _velocity.y; set => _velocity = new Vector2(_velocity.x, value); }
    

    protected override void Start()
    {
        base.Start();
        
        rb = GetComponent<Rigidbody2D>();
        bcoll = GetComponent<BoxCollider2D>();

        rb.sharedMaterial = new PhysicsMaterial2D();
        rb.sharedMaterial.bounciness = 0;
        rb.sharedMaterial.friction = 0;

        speed = entityStats.GetSetAttribute("speed", 10);
        accel = entityStats.GetSetAttribute("accel", 20);
        
        SetRayGroups();
    }


    void SetRayGroups()
    {
        Rect r = new Rect(bcoll.offset - bcoll.size / 2, bcoll.size);
        rect = r;

        Vector2 offset = new Vector2(0.01f, 0.01f); // Vector2.zero;
        collDist = Vector2.one * Physics2D.defaultContactOffset + offset; //new Vector2(0.04f, 0.04f);
        detectDist = new Vector2(0.05f, 0.05f);
        collHalf = bcoll.size / 2 + collDist;
        Debug.Log($"Constact offset{Physics2D.defaultContactOffset}");
        float xMin = r.xMin + offset.x, 
            xMax = r.xMax - offset.x, 
            yMin = r.yMin + offset.y, 
            yMax = r.yMax - offset.y;

        rcLeft = new RaycastGroup(5, new Vector2(r.center.x, yMin), new Vector2(r.center.x, yMax), groundMask);
        rcRight = new RaycastGroup(5, new Vector2(r.center.x, yMin), new Vector2(r.center.x, yMax), groundMask);
        rcUp = new RaycastGroup(3, new Vector2(xMin, r.center.y), new Vector2(xMax, r.center.y), groundMask);
        rcDown = new RaycastGroup(3, new Vector2(xMin, r.center.y), new Vector2(xMax, r.center.y), groundMask);

    }


    private void FixedUpdate()
    {
        UpdateMove();
    }
    protected void Update()
    {
        
    }

    void UpdateMove()
    {
        Vector2 tpos = transform.position;
        CastDown(tpos);


        if (isGrounded)
        { rb.gravityScale = 0; GroundedMove(); }
        else
        { rb.gravityScale = 1; AerialMove(); }

    }


    void GroundedMove()
    {
        Vector2 nvel = rb.velocity;
        if (nvel.x > 0)
        {
            RaycastHit2D rhit = Physics2D.Raycast(transform.position, Vector2.right, detectDist.x, groundMask);
            if (rhit.collider)
            {
                nvel = nvel.Redirect(rhit.normal.Turn90CW());
            }
            else
            {
                rhit = Physics2D.Raycast(transform.position, Vector2.down, detectDist.x, groundMask);
                if (rhit.collider)
                {
                    nvel = nvel.Redirect(rhit.normal.Turn90CW());
                }
            }
        }
        else if (nvel.x < 0)
        { 

        }
    }
    void AerialMove()
    {
        Vector2 nvel = rb.velocity;
        nvel.y = Mathf.Max(0, rb.velocity.y); rb.gravityScale = 0;
    }




    float dDistMax(float value, float dDist)
    { return Mathf.Max(dDist + value, dDist); }
    float dDistMin(float value, float dDist)
    { return Mathf.Min(dDist + value, dDist); }

    int CastDown(Vector2 point)
    {
        Vector2 goffset = new Vector2(0.2f, 0.2f);
        Vector2 rpos = new Vector2(point.x + rect.center.x, point.y + rect.yMin + goffset.y);
        
        float baseDist = goffset.y;
        rhDown.shortDistance = baseDist + detectDist.y;
        rhDown.shortBaseDistance = baseDist - rhDown.shortDistance;

        float rwhalf = rect.width / 2;
        
        RaycastHit2D rhit_min = Physics2D.Raycast(rpos, Vector2.left, rwhalf, groundMask);
        RaycastHit2D rhit_max = Physics2D.Raycast(rpos, Vector2.right, rwhalf, groundMask);

        Vector2 start = rpos, end = rpos;
        if (rhit_min.collider) start = rhit_min.point;
        else start.x -= rwhalf;
        if (rhit_max.collider) end = rhit_max.point;
        else end.x += rwhalf;

        float xdiff = start.x + end.x;
        float xjump = xdiff / (rhDown.rayHits.Length - 1);
        _gVelocity = Vector2.zero;

        rhDown.hits = 0;
        RaycastHit2D rhit;
        for (int i = 0; i < rhDown.rayHits.Length; i++)
        {
            rhit = Physics2D.Raycast(start.Add(xjump * i, goffset.y), Vector2.down, goffset.y + detectDist.y, groundMask);
            if (rhit.collider)
            {
                rhDown.hits++; 
                if (rhit.distance < rhDown.shortDistance)
                {
                    rhDown.shortDistance = rhit.distance;
                    rhDown.shortBaseDistance = rhDown.shortDistance - baseDist;
                }
                if (rhit.collider.gameObject.TryGetComponent<Entity>(out Entity ent))
                { _gVelocity += ent.velocity; }
            }
            rhDown.rayHits[i] = rhit;
        }
        isGrounded = false;
        if (rhDown.hits > 0)
        {
            _gVelocity /= rhDown.hits;
            isGrounded = true;
        }
        return rhDown.hits;
    }

    int CastLeft(Vector2 point)
    {
        Vector2 goffset = new Vector2(0.2f, 0.2f);
        Vector2 rpos = new Vector2(point.x + rect.center.x, point.y + rect.yMin + goffset.y);

        float baseDist = goffset.x;
        rhLeft.shortDistance = baseDist + detectDist.x;
        rhLeft.shortBaseDistance = baseDist - rhLeft.shortDistance;

        RaycastHit2D rhit_min = Physics2D.Raycast(rpos, Vector2.down, rect.height / 2, groundMask);
        RaycastHit2D rhit_max = Physics2D.Raycast(rpos, Vector2.up, rect.height / 2, groundMask);

        Vector2 start = rhit_min.collider ? rhit_min.point : rect.height / 2 * Vector2.down;
        float ydiff = rhit_max.distance + rhit_min.distance;
        float yjump = ydiff / (rhLeft.rayHits.Length - 1);

        rhLeft.hits = 0;
        RaycastHit2D rhit;
        for (int i = 0; i < rhLeft.rayHits.Length; i++)
        {
            rhit = Physics2D.Raycast(start + new Vector2(goffset.x + point.x, yjump * i + point.y), Vector2.left, goffset.x + detectDist.x, groundMask); 
            if (rhit.collider)
            {
                rhLeft.hits++;
                if (rhit.distance < rhLeft.shortDistance)
                {
                    rhLeft.shortDistance = rhit.distance;
                    rhLeft.shortBaseDistance = rhLeft.shortDistance - baseDist;
                }
            }
        }

        wallLeft = rhLeft.hits > 0;
        return rhLeft.hits;
    }

    int CastRight(Vector2 point)
    {
        Vector2 goffset = new Vector2(0.2f, 0.2f);
        Vector2 rpos = new Vector2(point.x + rect.center.x, point.y + rect.yMin + goffset.y);

        float baseDist = goffset.y;
        rhRight.shortDistance = baseDist + detectDist.x;
        rhRight.shortBaseDistance = baseDist - rhRight.shortDistance;

        RaycastHit2D rhit_min = Physics2D.Raycast(rpos, Vector2.down, rect.height / 2, groundMask);
        RaycastHit2D rhit_max = Physics2D.Raycast(rpos, Vector2.up, rect.height / 2, groundMask);

        Vector2 start = rhit_min.collider ? rhit_min.point : rect.height / 2 * Vector2.down;
        float ydiff = rhit_max.distance + rhit_min.distance;
        float yjump = ydiff / (rhRight.rayHits.Length - 1);

        rhRight.hits = 0;
        RaycastHit2D rhit;
        for (int i = 0; i < rhRight.rayHits.Length; i++)
        {
            rhit = Physics2D.Raycast(start + new Vector2(goffset.x + point.x, yjump * i + point.y), Vector2.left, goffset.x + detectDist.x, groundMask);
            if (rhit.collider)
            {
                rhRight.hits++;
                if (rhit.distance < rhRight.shortDistance)
                {
                    rhRight.shortDistance = rhit.distance;
                    rhRight.shortBaseDistance = rhRight.shortDistance - baseDist;
                }
            }
        }

        wallRight = rhRight.hits > 0;
        return rhRight.hits;
    }

    int CastUp(Vector2 point)
    {
        Vector2 goffset = new Vector2(0.2f, 0.2f);
        Vector2 rpos = new Vector2(point.x + rect.center.x, point.y + rect.yMin + goffset.y);

        float baseDist = goffset.y;
        rhUp.shortDistance = baseDist + detectDist.y;
        rhUp.shortBaseDistance = baseDist - rhUp.shortDistance;

        RaycastHit2D rhit_min = Physics2D.Raycast(rpos, Vector2.down, rect.height / 2, groundMask);
        RaycastHit2D rhit_max = Physics2D.Raycast(rpos, Vector2.up, rect.height / 2, groundMask);

        Vector2 start = rhit_min.collider ? rhit_min.point : rect.height / 2 * Vector2.down;
        float ydiff = rhit_max.distance + rhit_min.distance;
        float yjump = ydiff / (rhUp.rayHits.Length - 1);

        rhUp.hits = 0;
        RaycastHit2D rhit;
        for (int i = 0; i < rhUp.rayHits.Length; i++)
        {
            rhit = Physics2D.Raycast(start + new Vector2(goffset.x + point.x, yjump * i + point.y), Vector2.left, goffset.x + detectDist.x, groundMask);
            if (rhit.collider)
            {
                rhUp.hits++;
                if (rhit.distance < rhUp.shortDistance)
                {
                    rhUp.shortDistance = rhit.distance;
                    rhUp.shortBaseDistance = rhUp.shortDistance - baseDist;
                }
            }
        }

        wallTop = rhUp.hits > 0;
        return rhUp.hits;
    }


    public void Move(Vector2 dir)
    { targetMove = dir; }


    public class RayHits
    {
        public float shortDistance, shortBaseDistance;
        public RaycastHit2D[] rayHits;
        public int hits;
    }

}


/*

void _UpdateMove()
{
    //Vector3 tpos = transform.position;
    Vector2 nvel = _velocity;

    nvel.x = TowardsTargetValue(nvel.x, targetMove.x * speed, (isGrounded ? 50f : 30f) * Time.fixedDeltaTime);
    nvel.y = TowardsTargetValue(nvel.y, -20, 9.81f * Time.fixedDeltaTime);

    nvel *= Time.fixedDeltaTime;
    Vector2 ftpos = transform.position;
    Vector2 tpos = ftpos + nvel;

    Vector2 in_wall = Vector2.zero;
    Vector2 negDist = new Vector2(Mathf.Max(detectDist.x, detectDist.x - nvel.x), Mathf.Max(detectDist.y, detectDist.y - nvel.y));
    Vector2 posDist = new Vector2(Mathf.Max(detectDist.x, detectDist.x + nvel.x), Mathf.Max(detectDist.y, detectDist.y + nvel.y));

    Vector2 tpos2 = tpos;

    if (rcLeft.CastRange(3, 1, tpos, Vector2.left, 0, collHalf.x) > 0)
    { tpos2.x += rcLeft.shortBaseDistance; }
    if (rcRight.CastRange(3, 1, tpos, Vector2.right, 0, collHalf.x) > 0)
    { tpos2.x += rcRight.shortBaseDistance; }

    isGrounded = false; wasGrounded += Time.fixedDeltaTime;
    //Debug.Log($"{negDist} {posDist} {collHalf}");
    if (rcDown.Cast(tpos2, Vector2.down, negDist.y, collHalf.y) > 0)
    {
        if (rcDown.shortBaseDistance < detectDist.y)
        {
            isGrounded = true; wasGrounded = 0f;
        }
        if (rcDown.shortBaseDistance < 0)
        {
            in_wall.y += rcDown.shortBaseDistance;
        }

        GetGroundVelocity();
    }

    wallTop = false;
    if (rcUp.Cast(tpos2, Vector2.up, posDist.y, collHalf.y) > 0)
    {
        if (rcUp.shortBaseDistance < detectDist.y)
        {
            wallTop = true;
        }
        if (in_wall.y == 0 && rcUp.shortBaseDistance < 0)
        {
            in_wall.y -= rcUp.shortBaseDistance;
        }
    }


    Vector2 tpos3 = tpos2 - in_wall;
    in_wall.y = 0;

    wallLeft = false;
    if (rcLeft.Cast(tpos3, Vector2.left, negDist.x, collHalf.x) > 0)
    {
        if (rcLeft.shortBaseDistance < detectDist.x)
        {
            wallLeft = true;
        }
        if (rcLeft.shortBaseDistance < 0)
        {
            in_wall.x += rcLeft.shortBaseDistance;
        }
    }

    wallRight = false;
    if (rcRight.Cast(tpos3, Vector2.right, posDist.x, collHalf.x) > 0)
    {
        if (rcRight.shortBaseDistance < detectDist.x)
        {
            wallRight = true;
        }
        if (rcRight.shortBaseDistance < 0)
        {
            in_wall.x -= rcRight.shortBaseDistance;
        }
    }






    Vector2 npos = tpos3 - in_wall;//new Vector2(ftpos.x + maxMove.x, ftpos.y + maxMove.y);
    transform.position = npos;
    //rb.MovePosition(npos);

    _velocity = (tpos - ftpos) / Time.fixedDeltaTime;
    if (isGrounded) _velocity.y = Mathf.Max(0, _velocity.y);
}
*/