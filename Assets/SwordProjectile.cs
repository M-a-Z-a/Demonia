using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer), typeof(Animator), typeof(Rigidbody2D))]
public class SwordProjectile : Entity
{
    SpriteRenderer rend;
    Animator animator;
    RaycastHit2D rhit;
    Collider2D coll;

    public UnityEvent<SwordProjectile, Collider2D> onTargetHit, onStuck;

    public LayerMask stuckMask;
    [SerializeField] Vector2 _direction = Vector2.right;
    public Vector2 gravityOverride = Vector2.zero;
    public float rotationOffset = -90f, speed = 10f;
    [Min(0f)] public float timeToLive = 6f, stuckDistance = 2f;
    [SerializeField] bool _rotateTowardsDirection = false, _flipX = false, _flipY = false, _isHorizontal = false;
    bool _isStuck = false;
    public Vector2 direction { get => _direction; set => _direction = value.normalized; }
    public bool rotateTowardsDirection { get => _rotateTowardsDirection; set => Set_rotateTowardsDirection(value); }
    public bool flipX { get => _flipX; set => Set_flipX(value); }
    public bool flipY { get => _flipY; set => Set_flipY(value); }
    public bool isHorizontal { get => _isHorizontal; set => Set_isHorizontal(value); }
    public bool isStuck { get => _isStuck; protected set => Set_isStuck(value); }

    private void OnDrawGizmosSelected()
    {
        Vector3 tpos = transform.position;
        int segments = 16;
        float a, a_one = 360f / segments * Mathf.Deg2Rad;
        Vector3 npoint, lpoint = new Vector3(tpos.x + stuckDistance, tpos.y, tpos.z);
        Gizmos.color = Color.red;
        for (int i = 1; i <= segments; i++)
        {
            a = a_one * i;
            npoint = new Vector3(tpos.x + Mathf.Cos(a) * stuckDistance, tpos.y + Mathf.Sin(a) * stuckDistance, tpos.z);
            Gizmos.DrawLine(lpoint, npoint);
            lpoint = npoint;
        }
        Gizmos.DrawLine(tpos, tpos + (Vector3)(_direction * stuckDistance));
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        rend = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        direction = _direction;
        flipX = _flipX;
        isHorizontal = _isHorizontal;
        isStuck = _isStuck;
        rotateTowardsDirection = _rotateTowardsDirection;
    }

    void Set_rotateTowardsDirection(bool b)
    {
        _rotateTowardsDirection = b;
        if (b)
        { UpdateRotation(); }
    }
    void Set_flipX(bool b)
    {
        _flipX = b;
        if (!rend) rend = GetComponent<SpriteRenderer>();
        rend.flipX = _flipX;
    }
    void Set_flipY(bool b)
    {
        _flipY = b;
        if (!rend) rend = GetComponent<SpriteRenderer>();
        rend.flipY = _flipY;
    }

    void Set_isHorizontal(bool b)
    {
        _isHorizontal = b;
        animator.SetBool("isHorizontal", _isHorizontal);
    }
    void Set_isStuck(bool b)
    {
        _isStuck = b;
        animator.SetBool("isStuck", _isStuck);
        if (coll) coll.enabled = !b;
    }
    

    protected override void Awake()
    {
        base.Awake();
        rend = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        coll = GetComponent<Collider2D>();
    }
    

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        rotateTowardsDirection = _rotateTowardsDirection;
        flipX = _flipX;
        isHorizontal = _isHorizontal;
        _velocity = _direction * speed;
    }


    // Update is called once per frame
    void Update()
    {
        timeToLive -= Time.deltaTime;
        if (timeToLive <= 0) Destroy(gameObject);

        if (_isStuck) return;
        if (gravityOverride != Vector2.zero)
        {
            _velocity += gravityOverride * Time.deltaTime;
            _direction = _velocity.normalized;
            speed = Vector2.Distance(Vector2.zero, _velocity);
        }
        else
        { _velocity = _direction * speed; }
        
        if (rotateTowardsDirection)
        { UpdateRotation(); }
        transform.position += (Vector3)(_velocity * Time.deltaTime);
        RaycastForward();
    }

    void RaycastForward()
    {
        rhit = Physics2D.Raycast(transform.position, _direction, stuckDistance, stuckMask);
        if (rhit.collider)
        {
            isStuck = true;
            Vector3 tpos = rhit.point - _direction * stuckDistance;
            tpos.z = transform.position.z;
            transform.position = tpos;
            onStuck.Invoke(this, rhit.collider);
        }
    }

    void UpdateRotation()
    {
        Vector3 eul = transform.eulerAngles;
        eul.z = Vector2.SignedAngle(Vector2.right, _direction) + rotationOffset;
        transform.eulerAngles = eul;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    { onTargetHit.Invoke(this, collision); }

}
