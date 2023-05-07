using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(LineRenderer))]
public class PredictionSpawner : MonoBehaviour
{
    public enum OnEndActionType { None = 0, Disable, Deactivate, Loop, Destroy }
    public OnEndActionType onEndAction = OnEndActionType.Destroy;
    LineRenderer lrend;

    [SerializeField] GameObject prefab;
    public Vector3 direction;
    public float distance, time;
    public AnimationCurve pointAtTimeDelta = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    public UnityEvent<Vector3, Vector3, Vector3> onLineReady;

    private void OnValidate()
    {
        if (lrend != null)
        { lrend = GetComponent<LineRenderer>(); }
        direction = direction.normalized;
    }


    private void Start()
    {
        if (lrend != null)
        { lrend = GetComponent<LineRenderer>(); }
        direction = direction.normalized;
    }

    private void OnEnable()
    {
        lrend.useWorldSpace = true;
        lrend.positionCount = 2;
        lrend.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });

        StartCoroutine(IDrawLine());
    }

    IEnumerator IDrawLine()
    {
        float t = 0, d = 0;
        Vector3 tpos = transform.position, epos;
        lrend.SetPosition(0, transform.position);
        while (t < time)
        {
            lrend.SetPosition(1, tpos + direction * (distance * pointAtTimeDelta.Evaluate(d)));
            t += Time.deltaTime;
            d = t / time;
            yield return new WaitForEndOfFrame();
        }
        epos = tpos + direction * distance;
        lrend.SetPosition(1, epos);
        onLineReady.Invoke(tpos, epos, direction);
        OnEnd();
    }

    void OnEnd()
    {
        switch (onEndAction)
        {
            case OnEndActionType.Destroy:
                Destroy(gameObject); break;
            case OnEndActionType.Disable:
                enabled = false; break;
            case OnEndActionType.Deactivate:
                gameObject.SetActive(false); break;
            case OnEndActionType.Loop:
                OnEnable(); break;
        }
    }

}
