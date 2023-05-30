using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGWall : MonoBehaviour
{
    [SerializeField] Vector2 scale = Vector2.one, offset = Vector2.zero;
    [SerializeField] bool update;
    Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        UpdateScaling();
    }

    void UpdateScaling()
    {
        Vector2 tscale = transform.localScale;
        Vector2 nscale = tscale / scale, noffset = offset * scale;
        rend.material.SetTextureOffset("_MainTex", noffset);
        rend.material.SetTextureScale("_MainTex", nscale);
        rend.material.SetTextureOffset("_NormalMap", noffset);
        rend.material.SetTextureScale("_NormalMap", nscale);
    }
}
