using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public static CameraTarget activeTarget { get; protected set; }
    public static Transform targetTransform { get; protected set; }

    private void Start()
    {
        SetActive();
    }

    public void SetActive()
    {
        activeTarget = this;
        targetTransform = this.transform;
    }

}
