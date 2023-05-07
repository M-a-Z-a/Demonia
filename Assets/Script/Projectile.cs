using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity
{
    List<Collider2D> hits = new();

    public string projectileTag = "";
    public float lifeTime = 3f;
    public float timeToLive = 3f;
    public List<Action<Projectile, Collider2D>> onHitActions = new();
    public Vector2 direction;
    public bool directionRelative = true, flip = false;
    public float velocityMult = 1f;
    public AnimationCurve velocityOverTime;
    [SerializeField] Renderer distort;
    [SerializeField] AnimationCurve distortOverTime;
    Vector3 origScale;
    public Vector2 knockback = Vector2.zero;

    protected override void Awake()
    {
        base.Awake();
        origScale = transform.localScale;
        //Debug.Log($"{origScale}");
    }
    void SetOrigScale(Vector3 localscale)
    { origScale = localscale; }
    protected override void Start()
    {
        base.Start();
        timeToLive = lifeTime;
        direction = direction.normalized;
        Update();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        timeToLive -= Time.deltaTime;
        float d = 1f - timeToLive / lifeTime;
        float v = velocityOverTime.Evaluate(d) * velocityMult * Time.deltaTime;
        if (flip)
        { v = -v; transform.localScale = origScale.Mult(x: -1); }
        else
        { transform.localScale = origScale; }
        transform.position += directionRelative ? transform.right * v : (Vector3)(direction * v);
        if (distort)
        { distort.material.SetFloat("_DistortStrength", distortOverTime.Evaluate(d)); }
        if (timeToLive <= 0)
        { Destroy(gameObject); }
    }
    void OnDestroy()
    {
        
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (hits.Contains(collision))
        { return; }
        hits.Add(collision);
        for (int i = 0; i < onHitActions.Count; i++)
        { onHitActions[i].Invoke(this, collision); }
    }


}
