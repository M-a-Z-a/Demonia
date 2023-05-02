using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] GameObject proj;


    public void Spawn()
    {
        var go = Instantiate(proj, transform.position, Quaternion.identity, null);
        Projectile_ proj_ = go.GetComponent<Projectile_>();
        proj_.velocity = Utility.AngleToVector2(Random.Range(-180f, 180f)) * Vector2.Distance(Vector2.zero, proj_.velocity);
    }
}
