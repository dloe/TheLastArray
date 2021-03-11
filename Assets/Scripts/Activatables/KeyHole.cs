using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyHole : Activatable
{
    public GameObject doorObject;
    public ItemData keyData;
    public override void Activate()
    {
        if(!isActivated && Player.Instance.inventory.Contains(keyData))
        {
            doorObject.SetActive(false);
            Player.Instance.inventory.RemoveItemByType(ItemType.Key);
            isActivated = true;
            Destroy(gameObject);
        }

        
    }
}
