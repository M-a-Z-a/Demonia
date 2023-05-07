using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{

    [SerializeField] GameObject proj;
    Vector2 pdir;

    public void Spawn(Vector2 direction)
    {
        var go = Instantiate(proj, transform.position, Quaternion.identity, null);
        Projectile_ proj_ = go.GetComponent<Projectile_>();
        proj_.velocity = direction * Vector2.Distance(Vector2.zero, proj_.velocity);
    }
    public void Spawn()
    {
        pdir = Utility.AngleToVector2(Random.Range(-180f, 180f));

        var go = Instantiate(proj, transform.position, Quaternion.identity, null);
        Projectile_ proj_ = go.GetComponent<Projectile_>();
        proj_.velocity = pdir * Vector2.Distance(Vector2.zero, proj_.velocity);
    }

}
