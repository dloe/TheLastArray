using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    
    public ItemData itemData;

    public Item(ItemData data)
    {

        if (!data.name.Contains("Instance"))
        {
            itemData = ScriptableObject.CreateInstance<ItemData>();

            itemData.itemName = data.itemName;
            itemData.itemSprite = data.itemSprite;
            itemData.itemType = data.itemType;
            if (data.itemType == ItemType.MeleeWeapon)
            {
                itemData.damage = data.damage;
                itemData.coolDownPeriod = data.coolDownPeriod;
                itemData.meleeRange = data.meleeRange;
                itemData.hasDurability = data.hasDurability;
                itemData.durability = data.durability;
            }
            else if (data.itemType == ItemType.RangedWeapon)
            {
                itemData.damage = data.damage;
                itemData.coolDownPeriod = data.coolDownPeriod;
                itemData.ammoType = data.ammoType;
                itemData.reloadTime = data.reloadTime;
                itemData.magSize = data.magSize;
                itemData.loadedAmmo = data.loadedAmmo;
            }
            else if (data.itemType == ItemType.Heal)
            {
                itemData.amountToHeal = data.amountToHeal;
            }


            itemData.name = data.itemName + "(Instance)";

        }
        else
        {
            itemData = data;
        }
        
    }


    
}