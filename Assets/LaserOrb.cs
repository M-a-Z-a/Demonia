using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserOrb : Entity
{
    Transform laser_base;
    [SerializeField] GameObject laser_prefab;

    public float rotationSpeed = 45f;
    [SerializeField] int lasers = 3;
    Vector3 initRot;

    protected override void Awake()
    {
        base.Awake();
        laser_base = transform.Find("Gun_Base");
        SetLasers(lasers);
        initRot = laser_base.eulerAngles;
    }
    protected override void Start()
    {
        base.Start();
    }

    private void OnDisable()
    {
        laser_base.transform.eulerAngles = initRot;
    }

    void Update()
    {
        laser_base.eulerAngles = laser_base.eulerAngles.Add(z:rotationSpeed * Time.deltaTime);
    }

    void SetLasers(int count)
    {
        float a_one = 360f / count; GameObject go;
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = new Vector2(Mathf.Cos(a_one * i * Mathf.Deg2Rad), Mathf.Sin(a_one * i * Mathf.Deg2Rad)) * 0.8f;
            go = Instantiate(laser_prefab, laser_base.position.Add(x: pos.x, y: pos.y), Quaternion.Euler(0, 0, a_one * i + -90), laser_base);
            go.transform.parent = laser_base;
        }
    }
}
