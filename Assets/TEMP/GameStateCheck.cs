using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameStateCheck : MonoBehaviour
{
    public static GameStateCheck instance { get; protected set; }
    [SerializeField] UnityEvent onConditionMet;
    [SerializeField] int _collected = 0, _collectTarget = 10;
    bool conditionMetCalled;
    public static int collected { get => instance._collected; set => instance.SetCollected(value); }

    private void Awake()
    { 
        instance = this;
        conditionMetCalled = false;
    }

    void SetCollected(int c)
    {
        _collected = c;
        if (_collected >= _collectTarget && !conditionMetCalled)
        {
            instance.onConditionMet.Invoke(); 
            conditionMetCalled = true; 
        }
    }

    public void ResetCheck()
    {
        _collected = 0;
        conditionMetCalled = false;
    }
}

/*
public class Collectable : MonoBehaviour
{


    public void Collect()
    {
        GameStateCheck.collected += 1;
    }


}
*/