﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public enum ItemType
    {
        MeleeWeapon,
        Pistol,
        Rifle,
        Heal
    }

    public ItemType itemType;
    public ItemData itemData;

    public Item(ItemType type, ItemData data)
    {
        itemType = type;
        if(!data.name.Contains("Instance"))
        {
            itemData = ScriptableObject.CreateInstance<ItemData>();

            itemData.itemName = data.itemName;
            itemData.itemSprite = data.itemSprite;
            if (data.isMeleeWeapon)
            {
                itemData.isMeleeWeapon = data.isMeleeWeapon;
                itemData.damage = data.damage;
                itemData.coolDownPeriod = data.coolDownPeriod;
                itemData.meleeRange = data.meleeRange;
                itemData.hasDurability = data.hasDurability;
                itemData.durability = data.durability;
            }
            else if (data.isRangedWeapon)
            {
                itemData.isRangedWeapon = data.isRangedWeapon;
                itemData.damage = data.damage;
                itemData.coolDownPeriod = data.coolDownPeriod;
                itemData.ammoType = data.ammoType;
                itemData.reloadTime = data.reloadTime;
                itemData.magSize = data.magSize;
            }
            else if (data.isHealingItem)
            {
                itemData.isHealingItem = data.isHealingItem;
                itemData.amountToHeal = data.amountToHeal;
            }

            itemData.name = data.name + "(Instance)";

        }
        else
        {
            itemData = data;
        }
        
    }

    
}