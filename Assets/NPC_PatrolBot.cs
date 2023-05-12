using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_PatrolBot : Entity
{
    [SerializeField] LaserPointer laser;
    [SerializeField] Transform gunMount;
    Transform ltrans, target;
    Vector2 target_lpos;

    float scanCenterDefault = -90f, scanCenter = 0, scanCone = 90, scanSpeed = 0.25f;
    enum AlertState { Normal, Caution, Alert }
    AlertState pState = 0;

    float alertLevel = 0;
    bool flipX = false;

    void SetScanRate(float value)
    { scanSpeed = 1f / value; }

    void SetState(AlertState state, bool set_only_new = true)
    {
        if (set_only_new && state == pState) return;
        pState = state;
        switch (state)
        {
            case AlertState.Alert:
                laser.color = Color.red;
                scanCone = 30;
                alertLevel = 5f;
                SetScanRate(1f);
                break;
            case AlertState.Caution:
                laser.color = Color.yellow;
                scanCone = 90f;
                SetScanRate(2f);
                break;
            case AlertState.Normal:
                laser.color = Color.green;
                scanCone = 90f;
                SetScanRate(4f);
                break;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ltrans = laser.transform;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        laser.onHitNew.Add(OnScanHit);
        SetState(pState, false);
        //StartCoroutine(IScan());
    }

    private void OnEnable()
    {
        scanCenter = scanCenterDefault;
        StartCoroutine(IScan());
    }
    private void OnDisable()
    {
        StopCoroutine(IScan());
    }

    // Update is called once per frame
    void Update()
    {

        pLastHitSince += Time.deltaTime;

        if (alertLevel > 4) SetState(AlertState.Alert);
        else if (alertLevel > 1) SetState(AlertState.Caution);
        else SetState(AlertState.Normal);

        switch (pState)
        {
            case AlertState.Alert:
                AlertUpdate();
                break;

            case AlertState.Caution:
                CautionUpdate();
                break;

            //case PatrolState.Normal:
            default:
                NormalUpdate();
                break;
        }
        alertLevel = Mathf.Clamp(alertLevel, 0, 5f);
    }

    void AlertUpdate()
    {

        if (target != null)
        {
            alertLevel = 5f;
            target_lpos = target.position;
            GetScanCenterAngle(target_lpos);
        }
        else if (pLastHitSince < 5f)
        {
            GetScanCenterAngle(target_lpos);
        }
        else
        { alertLevel -= Time.deltaTime * 0.2f; scanCenter = scanCenterDefault; }
    }
    void CautionUpdate()
    {
        if (target != null)
        { alertLevel += Time.deltaTime * 2f; }
        else if (pLastHitSince > 1f)
        { alertLevel -= Time.deltaTime * 0.6f; }

        scanCenter = scanCenterDefault;
    }
    void NormalUpdate()
    {
        if (target != null)
        { alertLevel += 2f * Time.deltaTime; }
        else
        { alertLevel -= Time.deltaTime; }
        scanCenter = scanCenterDefault;
    }

    void GetScanCenterAngle(Vector2 p)
    {
        Vector2 posdif = p - (Vector2)ltrans.position;
        scanCenter = Vector2.SignedAngle(Vector2.up, posdif);
    }

    float pLastHitSince = 0f;

    RaycastHit2D scanHit;
    void OnScanHit(RaycastHit2D rhit)
    {
        scanHit = rhit;
        if (rhit.collider?.tag == "Player")
        { target = rhit.collider.transform; alertLevel = 2; pLastHitSince = 0; }
        else
        { target = null; }
    }

    
    IEnumerator IScan()
    {
        //float[] scan_offset = { 0, 90f, 180f, 270f };
        float[] scan_offset = { 0, 120f, 240f };
        int off_index = 0, off_len = scan_offset.Length;
        bool alt = false;
        bool alt2 = false;
        float sinangle = 0, roundangle;
        while (true)
        {
            sinangle += 360f * scanSpeed * Time.deltaTime;
            roundangle = RoundScale(sinangle, 12);
            float siny = Mathf.Sin((roundangle + scan_offset[off_index++% off_len]) * Mathf.Deg2Rad) * scanCone;
            Vector3 eul = ltrans.eulerAngles;
            eul.z = (flipX ? transform.eulerAngles.z + 180f : transform.eulerAngles.z) + scanCenter + siny * 0.5f;
            //eul.z = (flipX ? transform.eulerAngles.z + 180f : transform.eulerAngles.z) + scanCenter + ((alt = !alt) ? -siny : siny) * 0.5f;
            ltrans.eulerAngles = eul;
            laser.UpdateLaser();
            yield return new WaitForEndOfFrame();
        }
    }

    float RoundScale(float value, float scale)
    { return Mathf.Round(value * scale) / scale; }

}
