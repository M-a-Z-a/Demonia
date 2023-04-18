using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    float mult = 1f;
    public static Checkpoint activeCheckpoint;

    private void Awake()
    {
        
    }
    private void Start()
    {
        
    }

    private void Update()
    {
        transform.Rotate(new Vector3(90f, 65f, -76f) * mult * Time.deltaTime);
    }

    public void SetActiveCheckpoint()
    {
        if (activeCheckpoint == this) return;
        activeCheckpoint = this;
        StartCoroutine(ISpin(10f, 2f));
        Debug.Log($"Checkpoint set: {gameObject.name}");
    }

    IEnumerator ISpin(float m, float time)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            mult = Mathf.Lerp(m, 1f, t / time);
            yield return null;
        }
        mult = 1f;
    }

}
