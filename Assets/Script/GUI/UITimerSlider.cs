using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITimerSlider : MonoBehaviour
{

    [SerializeField] RectTransform sEdges, sBase, sSlider;

    public float length, delta, start_offset = 4.5f, end_offset = 4.5f, base_start = 5f, base_end = 5f;

    private void OnValidate()
    { 
        //SetDelta(delta); 
    }


    public void SetDelta(float d)
    {
        d = Mathf.Clamp01(d);

        Vector2 szd = sEdges.sizeDelta;
        szd.x = length + start_offset + end_offset;
        sEdges.sizeDelta = szd;

        szd = sBase.sizeDelta;
        szd.x = length + base_start + base_end;
        sBase.sizeDelta = szd;

        sSlider.anchoredPosition = new Vector2(start_offset + (length * d), 0);
    }

    public class SliderZone 
    { public float length, delta; }


}
