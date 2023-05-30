using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Activator : MonoBehaviour
{

    [SerializeField] float stateTimeMult = 1f;

    [SerializeField] UnityEvent onStateTrue, onStateFalse;
    [SerializeField] List<State> states;

    float stateLength;

    Coroutine loopCoroutine;
    private void OnEnable()
    {
        loopCoroutine = StartCoroutine(ILoop());
    }
    private void OnDisable()
    { if (loopCoroutine != null) StopCoroutine(loopCoroutine); }

    private void Awake()
    {
        states.Sort((a, b) => { return a.t.CompareTo(b.t); });
        stateLength = states[states.Count - 1].t;
    }

    [System.Serializable]
    public class State
    {
        public float t;
        public bool state;
        public UnityEvent onStateTrue;
        public UnityEvent onStateFalse;
        public State(bool state, float t)
        { this.state = state; this.t = t; }
    }

    IEnumerator ILoop(float t = 0)
    {
        int ind = 0;
        float wait_t = states[ind].t;
        while (true)
        {
            t += Time.deltaTime / stateTimeMult;
            if (t > wait_t)
            {
                if (states[ind].state)
                {
                    onStateTrue.Invoke();
                    states[ind].onStateTrue.Invoke();
                }
                else
                {
                    onStateFalse.Invoke();
                    states[ind].onStateFalse.Invoke();
                }
                t %= stateLength;
                ind = (ind + 1) % states.Count;
                wait_t = states[ind].t;
            }
            yield return new WaitForEndOfFrame();
        }
    }
}
