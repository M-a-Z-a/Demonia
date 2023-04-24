using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class Light2DDuo : MonoBehaviour
{

    Light2D thisl2, l2;
    [SerializeField] float lightMult = 0.5f;

    private void OnValidate()
    {
        if (thisl2 == null) thisl2 = GetComponent<Light2D>();
        string n = "Light2DExtra";
        if (l2 == null)
        {
            l2 = GetComponentInChildren<Light2D>();
            if (l2 == null)
            {
                if (l2.gameObject.name != n)
                {
                    GameObject go = new GameObject("Light2DExtra");
                    go.transform.parent = transform;
                    go.AddComponent<Light2D>();
                }
            }
        }
        l2.color = thisl2.color;
        l2.shadowsEnabled = false;
        l2.pointLightInnerAngle = thisl2.pointLightInnerAngle;
        l2.pointLightOuterAngle = thisl2.pointLightOuterAngle;
        l2.pointLightInnerRadius = thisl2.pointLightInnerRadius * lightMult;
        l2.pointLightOuterRadius = thisl2.pointLightOuterRadius * lightMult;
    }

}
