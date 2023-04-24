using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMover : Entity
{

    [SerializeField] float speed = 45f;
    Vector3 origPos;
    float angle = 0;

    protected override void Start()
    {
        base.Start();
        origPos = transform.position;
    }

    public void FixedUpdate()
    {
        Vector2 tposlast = transform.position;
        angle += speed * Time.deltaTime;
        if (angle > 360) angle %= 360;
        else if (angle < 0)
        { angle = 360 - (Mathf.Abs(angle) % 360); }
        //float y = Mathf.Sin(angle * Mathf.Deg2Rad);
        float x = Mathf.Cos(angle * Mathf.Deg2Rad);
        transform.position = origPos.Add(x: x * 4f);//, y: y * 2);
        _velocity = ((Vector2)transform.position - tposlast) / Time.deltaTime;
    }

}
