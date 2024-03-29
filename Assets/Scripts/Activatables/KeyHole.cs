﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHole : Activatable
{
    /// <summary>
    /// Activatable
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - activatable, requires a key to open door
    /// </summary>
    public GameObject doorObject;
    public ItemData keyData;
    public GameObject interior;

    public void Start()
    {
        if(TrainEntry.Instance)
        {
            this.transform.rotation = TrainEntry.Instance.transform.rotation;
        }
        
        if (interior != null)
        {
            foreach (Transform child in interior.transform)
            {
                child.gameObject.layer = 15;
                if (child.transform.childCount > 0)
                {
                    foreach (Transform gradchild in child.transform)
                    {
                        //Debug.Log(gradchild.name);
                        gradchild.gameObject.layer = 15;
                        if(gradchild.childCount > 0 && gradchild.GetChild(0).TryGetComponent<Canvas>(out Canvas mCanv))
                        {
                            gradchild.GetChild(0).transform.gameObject.layer = 15;
                        }
                    }
                }
            }
        }
    }

    public override void Activate()
    {
        if(!isActivated && Player.Instance.inventory.Contains(keyData))
        {
            doorObject.SetActive(false);
            Player.Instance.inventory.RemoveItemByType(ItemType.Key);
            isActivated = true;

            if (interior != null)
            {
                foreach (Transform child in interior.transform)
                {
                    child.gameObject.layer = 0;
                    if (child.transform.childCount > 0)
                    {
                        foreach (Transform gradchild in child.transform)
                        {
                            gradchild.gameObject.layer = 0;
                            if (gradchild.childCount > 0 && gradchild.GetChild(0).TryGetComponent<Canvas>(out Canvas mCanv))
                            {
                                gradchild.GetChild(0).transform.gameObject.layer = 5;
                            }
                        }
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
