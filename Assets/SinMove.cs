using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SinMove : MonoBehaviour
{

    public float rotatePerUnit = 180f;
    public Vector2 moveMult = Vector2.one;
    public Vector3 xyzMult = new Vector3(0, 1f, 0), xyzOffset = new Vector3(0f,0f,0f);
    Vector2 cpos, lpos;
    float ldist = 0;

    [SerializeField] UnityEvent onEnable, onDisable;

    private void OnEnable()
    { 
        lpos = transform.parent.position; 
        ldist = 0;
        onEnable.Invoke();
    }
    private void OnDisable()
    {
        onDisable.Invoke();
    }

    public void FixedUpdate()
    {
        cpos = transform.parent.position;
        CalcSpin(lpos, cpos);
        lpos = cpos;
    }

    void CalcSpin(Vector2 a, Vector2 b)
    {
        ldist += Vector2.Distance(a*moveMult, b*moveMult);
        float rot = ldist * rotatePerUnit;
        transform.localPosition = new Vector3(
            Mathf.Sin((rot + xyzOffset.x) * Mathf.Deg2Rad) * xyzMult.x,
            Mathf.Sin((rot + xyzOffset.y) * Mathf.Deg2Rad) * xyzMult.y,
            Mathf.Sin((rot + xyzOffset.z) * Mathf.Deg2Rad) * xyzMult.z);
    }

}
