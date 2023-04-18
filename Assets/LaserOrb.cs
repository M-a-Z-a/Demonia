using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserOrb : Entity
{
    Transform laser_base;
    Transform[] laser_guns;

    public float rotationSpeed = 45f;

    protected override void Start()
    {
        base.Start();
        laser_base = transform.Find("Gun_Base");
    }


    void Update()
    {
        laser_base.eulerAngles = laser_base.eulerAngles.Add(z:rotationSpeed * Time.deltaTime);
    }
}
