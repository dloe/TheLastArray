using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public enum ResourceType
    {
        scrap,
        meds,
        cloth
    }

    public ResourceType resourceType;
    public int amountToAdd = 1;

}
