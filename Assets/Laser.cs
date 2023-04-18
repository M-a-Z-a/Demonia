using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{

    public Transform origin;
    public float distance;
    LineRenderer lrend;
    public bool isActive;
    Coroutine fireCoroutine;
    BoxCollider2D bcoll;
    public LayerMask stopLayer;

    private void Awake()
    {
        lrend = GetComponent<LineRenderer>();
        lrend.widthMultiplier = 2f;
        bcoll = GetComponent<BoxCollider2D>();
        bcoll.size = new Vector2(0.2f, 1f);
    }

    private void Start()
    {
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
            rhit = Physics2D.Raycast(origin.position, origin.up, distance, stopLayer);
            lrend.SetPosition(0, origin.position);
            if (rhit.collider)
            {
                Debug.DrawLine(origin.position, rhit.point, Color.red);
                lrend.SetPosition(1, rhit.point);
                bcoll.offset = new Vector2(0, rhit.distance / 2f);
                bcoll.size = new Vector2(bcoll.size.x, rhit.distance);
            }
            else
            {
                Debug.DrawRay(origin.position, origin.up * distance, Color.red);
                lrend.SetPosition(1, origin.position + origin.up*distance);
                bcoll.offset = new Vector2(0, distance / 2f);
                bcoll.size = new Vector2(bcoll.size.x, distance);
            }
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

}
