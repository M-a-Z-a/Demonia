using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;


[RequireComponent(typeof(Camera))]
public class CameraResCopy : MonoBehaviour
{
    [SerializeField] Camera cameraToCopy;
    [SerializeField] float resMult = 1f;
    RenderTexture rtex;
    PixelPerfectCamera pcam;
    Camera cam;
    delegate Vector2Int GetResDel();
    GetResDel getRes;

    Vector2Int lres;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        rtex = cam.targetTexture;
        getRes = cameraToCopy.TryGetComponent(out PixelPerfectCamera pcam) ? GetResPPCam : GetResCam;
    }


    private void Update()
    {
        if (getRes() != lres)
        {
            lres = getRes();
            cam.orthographic = cameraToCopy.orthographic;
            cam.orthographicSize = cameraToCopy.orthographicSize;
            rtex.width = (int)(lres.x * resMult); rtex.height = (int)(lres.y * resMult);
        }
    }

    Vector2Int GetResPPCam()
    { return new Vector2Int(pcam.refResolutionX, pcam.refResolutionY); }
    Vector2Int GetResCam()
    { return new Vector2Int(Screen.width, Screen.height); }
}
