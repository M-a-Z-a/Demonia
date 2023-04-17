using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Mathematics;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    List<Entity> ents = new();

    public MonoBehaviour origin;
    public EntityStats.Damage damage;
    public bool destroyOnEnd = false;
    public float hitRate = 0;
    public float t = 0, time = 0.1f;
    [SerializeField] Vector3 ePoint;
    Vector3 sPoint;
    public bool xflip = false;
    public bool Active { get => gameObject.activeSelf; set => SetState(value); }

    private void Awake()
    {
        sPoint = transform.localPosition;
    }

    public void SetState(bool state)
    {
        if (state)
        { Enable(); return; }
        Disable();
    }
    public void Enable()
    {
        if (gameObject.activeSelf) return;
        gameObject.SetActive(true);
        StartCoroutine(Progress());
    }
    public void Disable()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + ePoint);
    }




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Entity>(out Entity ent))
        {
            if (!ents.Contains(ent))
            { 
          ents.Add(ent);
                ent.ApplyDamage(damage, origin);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Entity>(out Entity ent))
        { ents.Remove(ent); }
    }

    IEnumerator Progress()
    {
        int m = xflip ? -1 : 1;
        t = 0;
        Vector3 sp = sPoint.Mult(x: m);
        Vector3 ep = sp + ePoint.Mult(x:m);
        while (t < time)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(sp, ep, t / time);
            yield return null; 
        }
        Disable();
    }

}
