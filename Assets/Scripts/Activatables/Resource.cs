using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Resource : Activatable
{
    /// <summary>
    /// Activatable
    /// Dylan Loe
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - activatable used to add resources to the players inventory
    /// </summary>
    public enum ResourceType
    {
        scrap,
        meds,
        cloth
    }

    public ResourceType resourceType;
    public int amountToAdd = 1;
    public Text resourceText;
    
    public void Start()
    {
        amountToAdd = Random.Range(1, 4);
        resourceText.text += " (" + amountToAdd + ")";
    }

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
