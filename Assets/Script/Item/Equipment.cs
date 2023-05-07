using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
    public enum EquipmentType { Default = 0 };

    [SerializeField] EquipmentType _equipmentType = default;
    List<EquipmentEffect> effects;

    public virtual void OnEquip(EntityStats estats)
    {
        foreach (EquipmentEffect ef in effects)
        { estats.AddEffect(ef, this); }
    }
    public virtual void OnUnequip(EntityStats estats)
    {
        foreach (EquipmentEffect ef in effects)
        { estats.RemoveEffect(ef); }
    }

    public class EquipmentEffect : StatusEffect
    {
        public EquipmentEffect(string name) : base(name)
        { }

        protected override bool Init()
        { return base.Init(); }
        public override void OnStart()
        { base.OnStart(); }
        public override void OnUpdate(float dt)
        { base.OnUpdate(dt); }
        public override void OnEnd()
        { base.OnEnd(); }

    }

}
