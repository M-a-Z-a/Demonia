using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class Shockwave : MonoBehaviour
{
    public enum FinishAction { Destroy = 0, Disable, Loop }
    public enum TimeScale { ScaledTime = 0, UnscaledTime }
    Material mat;
    [SerializeField] float time = 0.2f, widthMultiplier = 1, strengthMultiplier = 1;
    [SerializeField] AnimationCurve widthOverTime, deltaOverTime, strengthOverTime;
    float timer, t = 0;
    public FinishAction finishAction = FinishAction.Destroy;
    public TimeScale timeScale = TimeScale.ScaledTime;
    
    Renderer rend;

    private void OnValidate()
    {
        rend = GetComponent<Renderer>();
        //rend.materials = new Material[1];
        if (rend.sharedMaterial == null) return;
        mat = new Material(rend.sharedMaterial);
        rend.material = mat;
    }
    private void Awake()
    {
        rend = GetComponent<Renderer>();
        mat = rend.sharedMaterial;
        Reset();
    }
    private void OnEnable()
    {
        Reset();
        rend.enabled = true;
    }
    private void OnDisable()
    {
        rend.enabled = false;
        Deactivate();
    }


    public void Activate()
    {
        Debug.Log("Activate()");
        gameObject.SetActive(true);
        rend.enabled = true;
        enabled = true;
    }
    public void Deactivate()
    {
        Debug.Log("Deactivate()");
        //gameObject.SetActive(false);
        rend.enabled = false;
        enabled = false;
    }

    private void Reset()
    { t = 0; }

    private void Update()
    {
        if (t < 1f)
        {
            switch (timeScale)
            {
                case TimeScale.ScaledTime:
                    t += Time.deltaTime * (1f / time); break;
                case TimeScale.UnscaledTime:
                    t += Time.unscaledDeltaTime * (1f / time); break;
            }
            float d = t / 1f;
            float v = widthOverTime.Evaluate(d) * widthMultiplier;
            float dv = deltaOverTime.Evaluate(d) * 0.4f;
            float sv = strengthOverTime.Evaluate(d) * strengthMultiplier;
            mat.SetFloat("_Delta", dv);
            mat.SetFloat("_Size", (1f - d) * v);
            mat.SetFloat("_DistortStrength", sv);
        }
        else
        {
            switch (!Application.isEditor ? finishAction : FinishAction.Loop)
            {
                case FinishAction.Disable:
                    Deactivate(); break;
                case FinishAction.Loop:
                    t = 0; break;
                case FinishAction.Destroy:
                default:
                    Destroy(gameObject); break;
            }
        }
    }


}
