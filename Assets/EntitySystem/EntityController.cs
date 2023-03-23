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

    //RaycastGroup2 rcLeft, rcRight, rcUp, rcDown;
    //RayHits rhLeft, rhRight, rhUp, rhDown;

    EntityStats.Attribute speed, accel;

    public bool isGrounded = false, lastGrounded = false, wallLeft = false, wallRight = false, wallTop = false;
    public float wasGrounded = 0f, gravityScale = 1f, temporalGravityMult = 1f;

    
    Vector2 _gVelocity = Vector2.zero;

    public new Vector2 velocity { get => _velocity; set => _velocity = value; }
    public Vector2 groundVelocity { get => _gVelocity; }
    public float xVelocity { get => _velocity.x; set => _velocity = new Vector2(value, _velocity.y); }
    public float yVelocity { get => _velocity.y; set => _velocity = new Vector2(_velocity.x, value); }

    float groundedVelocity = 0f;

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

        /*
        rcLeft = new RaycastGroup2(5, new Vector2(r.center.x, yMin), new Vector2(r.center.x, yMax), groundMask);
        rcRight = new RaycastGroup2(5, new Vector2(r.center.x, yMin), new Vector2(r.center.x, yMax), groundMask);
        rcUp = new RaycastGroup2(3, new Vector2(xMin, r.center.y), new Vector2(xMax, r.center.y), groundMask);
        rcDown = new RaycastGroup2(3, new Vector2(xMin, r.center.y), new Vector2(xMax, r.center.y), groundMask);
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



    }
    void GroundedMove()
    {

    }
    void AerialMove()
    {

    }




    float dDistMax(float value, float dDist)
    { return Mathf.Max(dDist + value, dDist); }
    float dDistMin(float value, float dDist)
    { return Mathf.Min(dDist + value, dDist); }




    public void Move(Vector2 dir)
    { targetMove = dir; }


}

