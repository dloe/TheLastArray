using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : Activatable
{
    public enum ResourceType
    {
        scrap,
        meds,
        cloth
    }

    public ResourceType resourceType;
    public int amountToAdd = 1;

    public override void Activate()
    {
        switch (resourceType)
        {
            case ResourceType.scrap:
                Player.Instance.ScrapCount += amountToAdd;
                break;
            case ResourceType.meds:
                Player.Instance.MedsCount += amountToAdd;
                break;
            case ResourceType.cloth:
                Player.Instance.ClothCount += amountToAdd;
                break;
            default:
                break;
        }
        Debug.Log("Resource Picked Up: " + amountToAdd + " " + resourceType);
        Destroy(gameObject);
    }

}
