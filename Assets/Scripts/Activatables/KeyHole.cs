﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHole : Activatable
{
    public GameObject doorObject;
    public ItemData keyData;
    public GameObject interior;

    public void Start()
    {
        if (interior != null)
        {
            foreach (Transform child in interior.transform)
            {
                child.gameObject.layer = 15;
                if (child.transform.childCount > 0)
                {
                    foreach (Transform gradchild in child.transform)
                    {
                        gradchild.gameObject.layer = 15;
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
                        }
                    }
                }
            }

            Destroy(gameObject);
        }

        
    }
}