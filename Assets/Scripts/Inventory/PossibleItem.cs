using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectWeightType
{
    None,
    Weapon,
    Item,
    Resource
}
public class PossibleItem : MonoBehaviour
{
    /// <summary>
    /// PossibleItem
    /// Dylan Loe
    /// 
    /// Updated: 5/25/22
    /// 
    /// - Stores possible item that the tile generation reads in for determining items
    /// 
    /// </summary>
    public ObjectWeightType objectWeight;
    [HideInInspector]
    public bool inUse = false;
   // [HideInInspector]
   // public int itemIndex = -1;
}
