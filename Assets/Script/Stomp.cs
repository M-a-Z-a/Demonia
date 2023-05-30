using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Stomp : MonoBehaviour
{
    Shockwave swave;
    CapsuleCollider2D coll;

    Vector2 _origin;
    public Vector2 origin { get => transform.position.Add(_origin.x, _origin.y); }
    public Vector2 coll_pos_s, coll_pos_e, coll_size_s, coll_size_e;
    [HideInInspector] public UnityEvent<Stomp, Collider2D, float> onTargetHit;
    float sdelta = 0;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<CapsuleCollider2D>();
        swave = GetComponent<Shockwave>();
        swave.onUpdate.AddListener(OnShockwaveUpdate);
        UpdateStompOrigin();
    }

    private void OnDrawGizmosSelected()
    {
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        Vector2 off = rend.sharedMaterial.GetVector("_Offset");
        Vector2 offdelta = new Vector2(off.x + 1f, off.y + 1f) / 2;
        Vector2 sz = rend.size;
        Vector2 orig = sz * offdelta - sz / 2;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position.Add(x: orig.x-.5f, y: orig.y), Vector3.right);
        Gizmos.DrawRay(transform.position.Add(x: orig.x, y: orig.y-.5f), Vector3.up);
    }

    void OnShockwaveUpdate(float d, float dv, float v, float sv)
    {
        sdelta = dv;
        coll.offset = Vector2.Lerp(coll_pos_s, coll_pos_e, dv);
        coll.size = Vector2.Lerp(coll_size_s, coll_size_e, dv);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onTargetHit.Invoke(this, collision, sdelta);
    }

    void UpdateStompOrigin()
    {
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        Vector2 off = rend.material.GetVector("_Offset");
        Vector2 offdelta = new Vector2(off.x + 1f, off.y + 1f) / 2;
        Vector2 sz = rend.size;
        _origin = sz * offdelta - sz / 2;
    }
}
