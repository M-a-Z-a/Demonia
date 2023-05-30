using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] UnityEvent<Interactable> onInteract, onTrue, onFalse;
    [SerializeField] Transform uiTransform;
    [SerializeField] float interactDelay = 0.5f;
    float delay = 0f;
    [SerializeField] bool isToggle = false, _state;
    [SerializeField] Sprite stateOn, stateOff;
    public bool switchState { get => _state; }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        DrawLines(onInteract);
        Gizmos.color = Color.green;
        DrawLines(onTrue, -0.1f);
        Gizmos.color = Color.red;
        DrawLines(onFalse, 0.1f);
    }

    void DrawLines(UnityEvent<Interactable> uevent, float side_offset = 0)
    {
        GameObject go = null;
        Object obj;
        Vector3 tpos = transform.position, diff;
        Vector2 a;
        List<GameObject> gos = new();
        int tcount = uevent.GetPersistentEventCount();
        for (int i = 0; i < tcount; i++)
        {
            go = uevent.GetPersistentTarget(i) as GameObject;
            if (go == null) go = (uevent.GetPersistentTarget(i) as MonoBehaviour).gameObject;
            if (go == null) continue;
            if (gos.Contains(go)) continue;
            gos.Add(go);
            if (side_offset != 0)
            {
                diff = go.transform.position - tpos;
                a = new Vector2(diff.y, -diff.x).normalized * side_offset;
            }
            else
            { a = Vector2.zero; }
            Gizmos.DrawLine(tpos.Add(a.x, a.y), go.transform.position.Add(a.x, a.y));
        }
    }
#endif
    private void OnValidate()
    {
        UpdateState();
    }
    void UpdateState()
    {
        SpriteRenderer rend = GetComponent<SpriteRenderer>();
        if (rend == null) return;
        if (_state)
        { if (stateOn != null) rend.sprite = stateOn; }
        else
        { if (stateOff != null) rend.sprite = stateOff; }
    }

    public void Interact()
    { 
        if (delay <= 0)
        {
            _state = !_state;
            onInteract.Invoke(this);
            UpdateState();
            if (isToggle)
            {
                if (_state) onTrue.Invoke(this);
                else onFalse.Invoke(this);
            }
            StartCoroutine(IDelay());
        }
    }

    private void Start()
    {
        SetUIState(false);
    }

    void OnUIEnabled()
    {
        if (uiTransform == null) return;
        uiTransform?.gameObject.SetActive(true);
    }
    void OnUIDisabled()
    {
        if (uiTransform == null) return;
        uiTransform.gameObject.SetActive(false);
    }

    public void TestMessage(string msg)
    {
        Debug.Log(msg);
    }

    public void SetUIState(bool value)
    {
        if (value)
        { OnUIEnabled(); return; }
        OnUIDisabled();
    }

    IEnumerator IDelay()
    {
        delay = interactDelay;
        while (delay > 0)
        {
            delay -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        delay = 0;
    }
}
