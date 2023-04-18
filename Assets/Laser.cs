using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Entity
{
    public float distance;
    LineRenderer lrend;
    public bool isActive;
    Coroutine fireCoroutine;
    BoxCollider2D bcoll;
    public LayerMask stopLayer;

    protected override void Awake()
    {
        base.Awake();
        lrend = GetComponent<LineRenderer>();
        lrend.widthMultiplier = 3f;
        bcoll = GetComponent<BoxCollider2D>();
        bcoll.size = new Vector2(0.2f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.up * distance); 
    }


    private void OnEnable()
    {
        fireCoroutine = StartCoroutine(IFire());
    }
    private void OnDisable()
    {
        if (fireCoroutine != null) StopCoroutine(fireCoroutine);
    }

    
    protected override void Start()
    {
        base.Start();
        if (isActive) Activate();
    }

    public void Activate()
    {
        isActive = true;
        lrend.positionCount = 2;
        if (fireCoroutine != null) StopCoroutine(fireCoroutine);
        fireCoroutine = StartCoroutine(IFire());
    }
    public void Deactivate()
    {
        isActive = false;
        if (fireCoroutine != null) StopCoroutine(fireCoroutine);
        lrend.positionCount = 0;
    }

    RaycastHit2D rhit;
    IEnumerator IFire()
    {
        while(true)
        {
            rhit = Physics2D.Raycast(transform.position, transform.up, distance, stopLayer);
            lrend.SetPosition(0, transform.position);
            if (rhit.collider)
            {
                Debug.DrawLine(transform.position, rhit.point, Color.red);
                lrend.SetPosition(1, rhit.point);
                bcoll.offset = new Vector2(0, rhit.distance / 2f);
                bcoll.size = new Vector2(bcoll.size.x, rhit.distance);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.up * distance, Color.red);
                lrend.SetPosition(1, transform.position + transform.up*distance);
                bcoll.offset = new Vector2(0, distance / 2f);
                bcoll.size = new Vector2(bcoll.size.x, distance);
            }
            yield return null;
        }
    }


}
