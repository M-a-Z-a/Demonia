using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static InputManagerClasses;
using static Utility;

public class Player : MonoBehaviour
{
    public static Transform pTransform { get; protected set; }
    public static Player instance { get; protected set; }


    Rigidbody2D rb;
    EntityController entity;
    EntityStats entityStats;
    Transform cameraTarget;

    InputVector2 inputVector;
    InputKey left, right, jump;
    EntityStats.Attribute speed, jumpForce, airJumpForce, wallGrabGravMult, wallGrabDrag, WallGrabLowVelocity;

    Vector2 jumpInitPoint, jumpInitVelocity;
    float moveRightMult = 1f, moveLeftMult = 1f;
    int wallGrabbed = 0;
    bool allowWallJump = false;

    float coyoteTime = 0.1f;
    int multiJumps = 0, multiJump_c = 1;

    Coroutine cHaltActions, cHaltMovement, cHaltJump;
    bool haltActions = false, haltMovement = false, haltJump = false;
    float haltActions_t = 0f, haltMovement_t = 0f, haltJump_t = 0f;

    void Start()
    {
        if (instance)
        { Debug.Log("Instance of \"Player\" already exists?"); }
        instance = this;

        rb = GetComponent<Rigidbody2D>();
        entity = GetComponent<EntityController>();
        entityStats = GetComponent<EntityStats>();
        inputVector = InputManager.GetInputVector<InputVector2>("direction");
        left = inputVector.inputX.negative;
        left = inputVector.inputX.positive;
        jump = InputManager.SetInputKey("jump", KeyCode.X);

        cameraTarget = transform.Find("CameraTarget");

        speed = entityStats.GetSetAttribute("speed", 10);
        jumpForce = entityStats.GetSetAttribute("jumpforce", 10);
        airJumpForce = entityStats.GetSetAttribute("airjumpforce", 5);
        wallGrabGravMult = entityStats.GetSetAttribute("wallgrabgravmult", 0.5f);
        wallGrabDrag = entityStats.GetSetAttribute("wallgrabdrag", 4f);
        WallGrabLowVelocity = entityStats.GetSetAttribute("wallgrablowvelocity", -15f);

        CameraControl.instance.followTarget = transform;
    }


    void Update()
    {
        Vector2 dir = (Vector2)inputVector;

        if (entity.isGrounded)
        { multiJumps = multiJump_c; }

        if (!haltActions)
        {
            UpdateMove(dir);
        }
    }


    void UpdateMove(Vector2 dir)
    {
        cameraTarget.localPosition = entity.velocity * Time.deltaTime;
        if (haltMovement) return;

        if (dir.x < 0)
        {
            //rb.AddForce(new Vector2(dir.x * moveRightMult * speed, 0));
            entity.Move(new Vector2(dir.x * moveLeftMult, 0));
            if (entity.wallLeft)// && entity.velocity.y > WallGrabLowVelocity)
            { wallGrabbed = -1; }
            else
            { wallGrabbed = 0; }
        }
        else if (dir.x > 0)
        {
            //rb.AddForce(new Vector2(dir.x * moveRightMult * speed, 0));
            entity.Move(new Vector2(dir.x * moveRightMult, 0));
            if (entity.wallRight)// && entity.velocity.y > WallGrabLowVelocity)
            { wallGrabbed = 1; }
            else
            { wallGrabbed = 0; }
        }
        else
        { entity.Move(Vector2.zero); }

        if (wallGrabbed > 0 && entity.wallRight)// && entity.velocity.y > WallGrabLowVelocity)
        {
            multiJumps = multiJump_c;
            allowWallJump = WallGrabVelocityUpdate();
        }
        else if (wallGrabbed < 0 && entity.wallLeft)// && entity.velocity.y > WallGrabLowVelocity)
        {
            multiJumps = multiJump_c;
            allowWallJump = WallGrabVelocityUpdate();
        }
        else
        {
            wallGrabbed = 0;
            allowWallJump = false;
        }


        if (jump.down)
        {
            if (!haltJump)
            {
                if (wallGrabbed != 0 && !entity.isGrounded && allowWallJump)
                {
                    Debug.Log($"Walljump wall:[{(wallGrabbed > 0 ? "right" : "left")}]");
                    Vector2 vvel = new Vector2(-wallGrabbed, 1).normalized;
                    //if (entity.velocity.y < 0)
                    entity.yVelocity = 0;
                    Jump(vvel * jumpForce, Vector2.up * jumpForce, -wallGrabbed);
                }
                else if (entity.wasGrounded < coyoteTime)
                {
                    Debug.Log($"Jump coyoteTime:[{entity.wasGrounded}/{coyoteTime}]");
                    float xabs = Mathf.Abs(entity.velocity.x);
                    Vector2 vvel = new Vector2(dir.x, 4).normalized;
                    Jump(vvel * jumpForce, Vector2.up * jumpForce);
                }
                else if (multiJumps > 0 && entity.velocity.y > WallGrabLowVelocity)
                {
                    multiJumps--;
                    Debug.Log($"Multijump jumps:[{multiJump_c - multiJumps}/{multiJump_c}]");
                    Vector2 vvel = new Vector2(dir.x, 2).normalized * airJumpForce.value;
                    if (entity.velocity.y < 0)
                    { entity.yVelocity = 0; }
                    Jump(vvel, Vector2.up * airJumpForce);
                }
            }
        }

    }
    

    void Jump(Vector2 force, Vector2 holdForce, int walljump = 0)
    {
        jumpInitPoint = transform.position;
        jumpInitVelocity = entity.velocity;

        entity.velocity += force;
        
        StartCoroutine(IHoldJump(holdForce));
        HaltJump(coyoteTime+.05f);
        switch (walljump)
        { 
            case 1: StartCoroutine(IWallJumpRight()); break;
            case -1: StartCoroutine(IWallJumpLeft()); break;
        }
    }

    bool WallGrabVelocityUpdate()
    {
        if (entity.velocity.y <= 0)
        {
            entity.temporalGravityMult = wallGrabGravMult;
            entity.yVelocity = TowardsTargetValue(entity.velocity.y, 0f, wallGrabDrag * Time.deltaTime);
            return entity.velocity.y >= WallGrabLowVelocity;
        }
        return true;
    }

    /*
    * ***********************************
    * !!! MOM'S SPAGHETTI, IT'S READY !!!
    * ***********************************
    */


    public void HaltActions(float t)
    {
        if (cHaltActions == null)
        { cHaltActions = StartCoroutine(IHaltActions(t)); return; }
        haltActions_t = t;
    }
    public void HaltActionsAdditive(float t)
    { HaltActions(haltActions_t + t); }
    public void HaltMovement(float t)
    {
        if (cHaltMovement == null)
        { cHaltMovement = StartCoroutine(IHaltMovement(t)); return; }
        haltMovement_t = t;
    }
    public void HaltMovementAdditive(float t)
    { HaltMovement(haltMovement_t + t); }


    public void HaltJump(float t)
    {
        if (cHaltJump == null)
        { cHaltJump = StartCoroutine(IHaltJump(t)); return; }
        haltJump_t = t;
    }
    public void HaltJumpAdditive(float t)
    { HaltJump(haltJump_t + t); }


    // IEnumerators

    // jumps
    IEnumerator IHoldJump(Vector2 force)
    {
        float cvalue = 0;
        while(cvalue < 1f && jump.hold && !entity.wallTop)
        {
            entity.velocity += InverseSineSlider(cvalue) * force * Time.deltaTime;
            cvalue += Time.deltaTime * 2;
            yield return null;
        }
    }

    IEnumerator IWallJumpLeft()
    {
        float cvalue = 0;
        while (cvalue < 0.5f)
        {
            moveRightMult = cvalue;
            cvalue += Time.deltaTime;
            yield return null;
        }
        moveRightMult = 1f;
    }
    IEnumerator IWallJumpRight()
    {
        float cvalue = 0;
        while (cvalue < 0.5f)
        {
            moveLeftMult = cvalue;
            cvalue += Time.deltaTime;
            yield return null;
        }
        moveLeftMult = 1f;
    }

    // halt actions
    IEnumerator IHaltActions(float t)
    {
        haltActions_t = t;
        haltActions = true;
        while (haltActions_t > 0 && haltActions)
        {
            haltActions_t -= Time.deltaTime;
            yield return null;
        }
        haltActions_t = 0;
        haltActions = false;

        cHaltActions = null;
    }
    IEnumerator IHaltMovement(float t)
    {
        haltMovement_t = t;
        haltMovement = true;
        while (haltMovement_t > 0 && haltMovement)
        {
            haltMovement_t -= Time.deltaTime;
            yield return null;
        }
        haltMovement_t = 0;
        haltMovement = false;

        cHaltMovement = null;
    }

    IEnumerator IHaltJump(float t)
    {
        haltJump_t = t;
        haltJump = true;
        while (haltJump_t > 0 && haltJump)
        {
            haltJump_t -= Time.deltaTime;
            yield return null;
        }
        haltJump_t = 0;
        haltJump = false;

        cHaltJump = null;
    }

}
