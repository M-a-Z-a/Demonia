using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piston : MonoBehaviour
{

    [SerializeField] List<PartPipe> parts;
    [SerializeField][Range(0,1)] float _delta = 1f, _deltaMin = 0f, _deltaMax = 1f;
    [SerializeField] float extend_speed = 20f, retract_speed = 1f;
    float _deltaScaled = 1f;
    public float DeltaMin { get => _deltaMin; set => SetDeltaMin(value); }
    public float DeltaMax { get => _deltaMax; set => SetDeltaMax(value); }
    public float Delta { get => _delta; set => SetDelta(value); }
    public float ScaledDelta { get => _deltaScaled; }

    public AnimationCurve shakeStrengthByDistance;
    public float shakeDistance = 50f;


    bool _isMoving = false;
    public bool isMoving { get => _isMoving; }

    Coroutine moveCoroutine;


    public void Extend()
    { Extend(extend_speed); }
    public void Extend(float speed)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(IExtend(speed));
    }
    public void Retract()
    { Retract(retract_speed); }
    public void Retract(float speed)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(IRetract(speed));
    }

    private void OnValidate()
    {
        UpdatePositions();
    }

    void SetDeltaMin(float value)
    {
        _deltaMin = Mathf.Clamp01(value);
        UpdatePositions();
    }
    void SetDeltaMax(float value)
    {
        _deltaMax = Mathf.Clamp01(value);
        UpdatePositions();
    }
    void SetDelta(float d, bool ignore_moving = false)
    {
        if (isMoving && !ignore_moving) return;
        _delta = Mathf.Clamp01(d);
        UpdatePositions();
    }
    void UpdatePositions()
    {
        _deltaScaled = Mathf.Lerp(_deltaMin, _deltaMax, _delta);
        foreach (PartPipe part in parts)
        { part.SetDelta(_deltaScaled); }
    }


    void OnExtended()
    {
        float dist = Vector2.Distance(parts[parts.Count - 1].transform.position, Player.pDamageRelayTransform.position);
        float mag = shakeStrengthByDistance.Evaluate(dist / shakeDistance);
        CameraControl.instance.Shake(Vector2.down * mag, 10f, 0.25f);
    }
    void OnRetracted()
    { }


    IEnumerator IExtend(float speed)
    {
        _isMoving = true;
        while (_delta < 1f)
        {
            SetDelta(_delta + Time.deltaTime * speed, true);
            yield return new WaitForFixedUpdate();
        }
        _isMoving = false;
        OnExtended();
    }
    IEnumerator IRetract(float speed)
    {
        _isMoving = true;
        while (_delta > 0f)
        {
            SetDelta(_delta - Time.deltaTime * speed, true);
            yield return new WaitForFixedUpdate();
        }
        _isMoving = false;
        OnRetracted();
    }


    [System.Serializable]
    public class PartPipe
    {
        public Transform transform;
        public Vector3 point0, point1;
        public PartPipe(Transform transform, Vector3 point0, Vector3 point1)
        {
            this.transform = transform;
            this.point0 = point0;
            this.point1 = point1;
        }
        public void SetDelta(float d)
        { transform.localPosition = Vector3.Lerp(point0, point1, d); }
    }

}
