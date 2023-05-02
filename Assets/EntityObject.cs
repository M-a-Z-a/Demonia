using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityObject : Entity
{
    Rigidbody2D rb;
    public new Vector2 velocity { get => rb.velocity; set => rb.velocity = value; }

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected void FixedUpdate()
    {
        //_velocity = rb.velocity;
    }

    public override void AddForce(Vector2 force)
    {
        //base.AddForce(force);
        //rb.AddForce(force, ForceMode2D.Impulse);
        rb.velocity += force;
    }

}
