using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    //RaycastGroup2 rcLeft, rcRight, rcUp, rcDown;
    //RayHits rhLeft, rhRight, rhUp, rhDown;

    EntityStats.Attribute speed, accel;

    public bool isGrounded = false, lastGrounded = false, wallLeft = false, wallRight = false, wallTop = false;
    public float wasGrounded = 0f, gravityScale = 1f, temporalGravityMult = 1f;

    RaycastGroup2D rcgDown, rcgUp, rcgLeft, rcgRight;
    
    Vector2 _gVelocity = Vector2.zero, _gNormal = Vector2.zero;


    public Vector2 VelTest;

    public new Vector2 velocity { get => _velocity; set => _velocity = value; }
    public Vector2 groundVelocity { get => _gVelocity; }
    public float xVelocity { get => _velocity.x; set => _velocity = new Vector2(value, _velocity.y); }
    public float yVelocity { get => _velocity.y; set => _velocity = new Vector2(_velocity.x, value); }

    float groundedVelocity = 0f;

    protected override void Awake()
    {
        base.Awake();
        
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

        rcgDown = new RaycastGroup2DVertical(3, Vector2.down, new Vector2(0, -0.5f), groundMask);
        rcgUp = new RaycastGroup2DVertical(3, Vector2.up, new Vector2(0, 0.5f), groundMask);
        rcgLeft = new RaycastGroup2DHorizontal(5, Vector2.left, new Vector2(0, 0), groundMask);
        rcgRight = new RaycastGroup2DHorizontal(5, Vector2.right, new Vector2(0, 0), groundMask);
        /*
        float xMin = r.xMin + offset.x, 
            xMax = r.xMax - offset.x, 
            yMin = r.yMin + offset.y, 
            yMax = r.yMax - offset.y;
        */
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
        Vector2 nvel = _velocity; 

        Vector2 inColl = Vector2.zero;

        isGrounded = false; wasGrounded += Time.fixedDeltaTime; _gNormal = Vector2.up;
        if (rcgDown.Cast(tpos, rect, detectDist.y, collDist.y) > 0)
        { 
            isGrounded = true; wasGrounded = 0f;
            _gNormal = rcgDown.shortHit.normal;
            Debug.DrawRay(rcgDown.shortHit.centroid, rcgDown.shortHit.normal, Color.red);
            if (rcgDown.hitTresholdDistance < 0)
            {
                inColl.y += rcgDown.hitTresholdDistance;
            }
        }

        wallTop = false;
        if (rcgUp.Cast(tpos, rect, detectDist.y, collDist.y) > 0)
        { 
            wallTop = true;
            if (rcgUp.hitTresholdDistance < 0)
            {
                inColl.y -= rcgUp.hitTresholdDistance;
            }
        }

        wallLeft = false;
        if (rcgLeft.Cast(tpos, rect, detectDist.x,collDist.x) > 1)
        { 
            wallLeft = true;
            if (rcgLeft.hitTresholdDistance < 0)
            {
                inColl.x += rcgLeft.hitTresholdDistance;
            }
        }

        wallRight = false;
        if (rcgRight.Cast(tpos, rect, detectDist.x, collDist.x) > 1)
        { 
            wallRight = true;
            if (rcgRight.hitTresholdDistance < 0)
            {
                inColl.x -= rcgRight.hitTresholdDistance;
            }
        }

        groundedVelocity = 0f;
        if (isGrounded) 
        {
            if (!lastGrounded)
            { 
                float d = -Vector2.SignedAngle(nvel, -rcgDown.shortHit.normal);
                nvel.y = Mathf.Abs(d) / 90;
            }
            GroundedMove(ref tpos, ref nvel);
        }
        else
        { 
            AerialMove(ref tpos, ref nvel);
        }


        _velocity = (tpos + (nvel * Time.fixedDeltaTime) - tpos) / Time.fixedDeltaTime;

        if (inColl.x < 0)
        { _velocity.x = Mathf.Max(0, _velocity.x); }
        else if (inColl.x > 0)
        { _velocity.x = Mathf.Min(0, _velocity.x); }

        if (inColl.y < 0)
        { _velocity.y = Mathf.Max(0, _velocity.y); }
        else if (inColl.y > 0)
        { _velocity.y = Mathf.Min(0, _velocity.y); }

        tpos += (nvel * Time.fixedDeltaTime) - inColl;
        transform.position = tpos;
        VelTest = _velocity;
        lastGrounded = isGrounded;
    }
    void GroundedMove(ref Vector2 pos, ref Vector2 vel)
    {
        groundedVelocity = vel.magnitude;
        vel = TowardsTargetVector(vel, _gNormal.Turn90CW() * targetMove.x * speed, 4f * Time.deltaTime);
        Debug.Log(vel);
        if (vel.x > 0)
        {
            float a = 0;
            if (rcgRight.first.collider) 
            {
                a = Vector2.Angle(rcgRight.first.normal, Vector2.up);
            }
            else if (rcgDown.first.collider)
            {
                a = Vector2.Angle(rcgDown.first.normal, Vector2.up);
            }
        }
        else if (vel.x < 0)
        {
            float a = 0;
            if (rcgLeft.first.collider)
            {
                a = Vector2.Angle(rcgLeft.first.normal, Vector2.up);
            }
            else if (rcgDown.last.collider)
            {
                a = Vector2.Angle(rcgDown.last.normal, Vector2.up);
            }
        }
        
    }
    void AerialMove(ref Vector2 pos, ref Vector2 vel)
    {
        vel.x = TowardsTargetValue(vel.x, 0, 4f * Time.fixedDeltaTime);
        vel.y = TowardsTargetValue(vel.y, -20, 9.81f * 2 * Time.fixedDeltaTime);
    }


    public void Move(Vector2 dir)
    { targetMove = dir; }


    float dDistMax(float value, float dDist)
    { return Mathf.Max(dDist + value, dDist); }
    float dDistMin(float value, float dDist)
    { return Mathf.Min(dDist + value, dDist); }





}

