using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class LastArrayInteractable : WorldItem
{
    /// <summary>
    /// Activatable
    /// Dylan Loe
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - World Item, used for level objectives
    /// </summary>
    public GameObject bossDoor;
    public GameObject ammoDrop;

    private void Start()
    {

        gameObject.GetComponentInChildren<Text>().text = worldItemData.itemName;
        if (worldItemData.itemSprite != null)
        {
            gameObject.GetComponentInChildren<Image>().sprite = worldItemData.itemSprite;
        }
        gameObject.name = worldItemData.itemName + "_Interatable";
    }

    //when player picks up the final objective on the final level, they flee to the train (level start)
    public override void Activate()
    {
        //base.Activate();
        if (worldItemData.itemType == ItemType.finalObjective)
        {
            Debug.Log("activated last array, return to entrance...");
            Objectives.Instance.UpdateFinalObjective(2);
            bossDoor.SetActive(false);
            Instantiate(ammoDrop, this.transform.position, this.transform.rotation);

            Destroy(this.gameObject);
        }
    }
}
