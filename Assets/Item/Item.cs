using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemType { Generic = 0, Equipment = 1, Consumable = 2 }
    public enum ItemQuality { Unknown = -2, Trash = -1, Common = 0, Uncommon = 1, Rare = 2, Exotic = 3, Legendary = 4 }
    Dictionary<ItemQuality, float> _valueMultiplier = new()
    {
        { ItemQuality.Unknown, 0f },
        { ItemQuality.Trash, 0.5f },
        { ItemQuality.Common, 1f },
        { ItemQuality.Uncommon, 1.5f },
        { ItemQuality.Rare, 2.5f },
        { ItemQuality.Exotic, 5f },
        { ItemQuality.Legendary, 10f }
    };
    protected Dictionary<ItemQuality, float> valueMultiplier { get => _valueMultiplier; }

    [SerializeField] string _itemName = "";
    [SerializeField] string _itemDescription = "";
    [SerializeField] ItemType _itemType = ItemType.Generic;
    [SerializeField] ItemQuality _itemQuality = ItemQuality.Common;
    [SerializeField] protected bool ignoreQualityValue = false;
    [SerializeField] float _value = 0;

    public string itemName { get => _itemName; protected set => _itemName = value; }
    public ItemType itemType { get => _itemType; protected set => _itemType = value; }
    public ItemQuality itemQuality { get => _itemQuality; protected set => _itemQuality = value; }
    public string itemDescription { get => _itemDescription; protected set => _itemDescription = value; }
    public float itemBaseValue { get => _value; protected set => _value = value; }
    public float itemValue { get => GetItemValue(); }
    
    float GetItemValue()
    { return ignoreQualityValue ? _value : _value * _valueMultiplier[_itemQuality]; }

}
