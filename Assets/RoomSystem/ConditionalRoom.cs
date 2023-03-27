using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalRoom : Room
{


    [SerializeField] List<ConnectionCondition> conditions;

    
    public bool TestConditions()
    {
        int conds = 0;
        for (int i = 0; i < conditions.Count; i++)
        { if (conditions[i].TestCondition()) conds++; }
        return conds == conditions.Count;
    }

    public override bool Load()
    {
        if (_roomState == RoomState.Disabled && TestConditions())
        { _roomState = RoomState.Enabled; return true; }
        return false;
    }

    [System.Serializable]
    public class ConnectionCondition
    {
        public enum ConditionComparison
        {
            NotEqual = 0,
            Equal = 1,
            BiggerThan = 2,
            BiggerThanOrEqual = 3,
            Smaller = 4,
            SmallerOrEqual = 5
        }
        public string key;
        public ConditionComparison comparison;
        public int value;

        public bool TestCondition()
        {
            if (conditionValues.TryGetValue(key, out int v))
            {
                switch (comparison)
                {
                    case ConditionComparison.NotEqual:
                        return value != v;
                    case ConditionComparison.Equal:
                        return value == v;
                    case ConditionComparison.BiggerThan:
                        return value > v;
                    case ConditionComparison.BiggerThanOrEqual:
                        return value >= v;
                    case ConditionComparison.Smaller:
                        return value < v;
                    case ConditionComparison.SmallerOrEqual:
                        return value <= v;
                }
            }
            return true;
        }
    }
}
