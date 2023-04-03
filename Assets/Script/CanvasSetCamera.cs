using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasSetCamera : MonoBehaviour
{
    Canvas canvas;
    void Start()
    {
        canvas = GetComponent<Canvas>();
        Debug.Log("no cam");
        canvas.worldCamera = CameraControl.cam; 
    }

}
