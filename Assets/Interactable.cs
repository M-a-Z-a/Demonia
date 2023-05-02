using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [SerializeField] UnityEvent<Interactable> onInteract = new();
    [SerializeField] Transform uiTransform;
    [SerializeField] float interactDelay = 0.5f;
    float delay = 0f;

    public void Interact()
    { 
        if (delay <= 0)
        {
            onInteract.Invoke(this);
            StartCoroutine(IDelay());
        }
    }

    private void Start()
    {
        SetUIState(false);
    }

    void OnUIEnabled()
    {
        uiTransform.gameObject.SetActive(true);
    }
    void OnUIDisabled()
    {
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
