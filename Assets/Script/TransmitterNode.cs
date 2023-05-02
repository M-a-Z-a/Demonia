using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TransmitterNode : MonoBehaviour
{
    [SerializeField] List<UnityAction> onEnableActions, onDisableActions, actions;
    [SerializeField] List<UnityActionGroup> namedActions;

    public void InvokeGroup()
    {
        InvokeActions(actions);
    }
    public void InvokeGroup(string name)
    {
        foreach (UnityActionGroup g in namedActions)
        {
            if (g.name == name)
            { InvokeActions(g.actions); }
        }
    }

    private void OnEnable()
    { InvokeActions(onEnableActions); }
    private void OnDisable()
    { InvokeActions(onDisableActions); }

    void InvokeActions (IEnumerable<UnityAction> action_list)
    { 
        foreach (UnityAction action in action_list)
        { action.Invoke(); }
    }

    [System.Serializable]
    public struct UnityActionGroup
    { 
        public string name;
        public List<UnityAction> actions;
        public UnityActionGroup(string name, List<UnityAction> actions) 
        { this.name = name; this.actions = actions; }
    }

}

