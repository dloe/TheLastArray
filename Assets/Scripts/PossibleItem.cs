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
    public ObjectWeightType objectWeight;
   // [HideInInspector]
   // public int itemIndex = -1;
}
