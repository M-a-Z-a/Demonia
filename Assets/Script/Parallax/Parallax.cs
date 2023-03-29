using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Parallax : MonoBehaviour
{
    protected CameraControl cam;
    protected Transform cTransform;

    protected Vector3 targetPosition = Vector3.zero;
    public Vector2 relativePosition = Vector2.zero;
    public Vector3 referencePosition = Vector3.zero;

    protected virtual void Awake()
    {  }
    protected virtual void Start()
    {
        cam = CameraControl.instance;
        cTransform = cam.transform;
    }

    Vector3 tposdif;
    protected virtual void LateUpdate()
    {
        tposdif = cTransform.position - referencePosition;
        tposdif.x *= relativePosition.x;
        tposdif.y *= relativePosition.y;
        targetPosition = referencePosition.Add(tposdif.x, tposdif.y);
    }



}
