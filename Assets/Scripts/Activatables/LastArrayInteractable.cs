using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class LastArrayInteractable : WorldItem
{
    public GameObject bossDoor;
    private void Start()
    {

        gameObject.GetComponentInChildren<Text>().text = worldItemData.itemName;
        if (worldItemData.itemSprite != null)
        {
            gameObject.GetComponentInChildren<Image>().sprite = worldItemData.itemSprite;
        }
        gameObject.name = worldItemData.itemName + "_Interatable";
    }


    public override void Activate()
    {
        base.Activate();
        if (worldItemData.itemType == ItemType.finalObjective)
        {
            Debug.Log("activated last array, go back now lol");
            Objectives.Instance.UpdateFinalObjective(2);
            bossDoor.SetActive(false);
        }
    }
}
