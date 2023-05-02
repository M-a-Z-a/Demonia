using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopPatternTest : MonoBehaviour
{
    LoopPattern lpattern;
    LaserPointer laser;
    public float wtime = 0.1f;

    public float centerAngle = 180f, coneAngle = 90f, speed = 1f;
    float sinangle = 0;

    private void Awake()
    {
        lpattern =GetComponent<LoopPattern>();
        laser = GetComponent<LaserPointer>();
    }

    private void Start()
    {
        //StartCoroutine(IWait());
        laser.onHitNew.Add(OnHitNew);
        laser.manual_update = true;
    }

    private void Update()
    {
        sinangle += 360f * speed * Time.deltaTime;
        float siny = Mathf.Sin(sinangle * Mathf.Deg2Rad) * coneAngle;
        Vector3 eul = transform.eulerAngles;
        eul.z = centerAngle + siny * 0.5f;
        transform.eulerAngles = eul;
        laser.UpdateLaser();
    }

    IEnumerator IWait()
    {
        Vector3 eul;
        while (true)
        {
            yield return new WaitForSeconds(wtime);
            eul = transform.eulerAngles;
            eul.z = lpattern.GetNext();
            transform.eulerAngles = eul;
            laser.UpdateLaser();
        }
    }

    public void OnHitNew(RaycastHit2D rhit)
    {
        switch (rhit.collider.tag)
        {
            case "Player":
                laser.color = Color.red; 
                break;

            default: 
                laser.color = Color.blue; 
                break;
        }
        
    }

}
