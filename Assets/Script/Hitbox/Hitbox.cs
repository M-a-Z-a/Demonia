using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    List<Entity> ents = new();

    public MonoBehaviour origin;
    public EntityStats.Damage damage;
    public bool destroyOnEnd = false;
    public float hitRate = 0;


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

}
