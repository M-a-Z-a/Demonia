using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMaterial : Parallax
{
    protected Renderer rend;
    protected Material mat;

    protected override void Awake()
    {
        base.Awake();
        rend = GetComponent<Renderer>();
        mat = new Material(rend.sharedMaterial);
        rend.sharedMaterial = mat;
    }
    protected override void Start()
    {
        base.Start();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        rend.sharedMaterial.mainTextureOffset = targetPosition;
    }
}
