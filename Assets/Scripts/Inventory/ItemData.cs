﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


public enum AmmoType
{
    LightAmmo,
    HeavyAmmo
}
public enum ItemType
{
    MeleeWeapon,
    Pistol,
    Rifle,
    Heal
}
[Serializable][CreateAssetMenu]
public class ItemData : ScriptableObject
{
    


   

    public Sprite itemSprite;
    public ItemType itemType;
    public string itemName = "write item name here";

    public bool hasDurability = false;
    public int damage = 1;
    public int durability = -1;
    public float coolDownPeriod = 0.5f;
    public float reloadTime = 1f;

    public float meleeRange = 1f;

    public AmmoType ammoType;
    public int magSize = 5;
    public int loadedAmmo;
    

    public int amountToHeal = 1;

    public bool canAttack = true;
    public bool reloading = false;
    

    public IEnumerator CoolDown()
    {
        canAttack = false;
        yield return new WaitForSeconds(coolDownPeriod);
        canAttack = true;
    }

    public IEnumerator Reload(int amountToReload)
    {
        if(itemType != ItemType.Pistol && itemType != ItemType.Rifle)
        {
            yield return new WaitForEndOfFrame();
            Debug.LogError("This Function cannot be called unless the item is a ranged weapon, refrain from doing so");
        }
        else
        {
            reloading = true;
            Debug.Log("Reloading...");
            yield return new WaitForSeconds(reloadTime);
            Debug.Log("Reloaded");
            loadedAmmo += amountToReload;
            switch (ammoType)
            {
                case AmmoType.LightAmmo:
                    Player.Instance.currentLightAmmo -= amountToReload;
                    break;
                case AmmoType.HeavyAmmo:
                    Player.Instance.currentHeavyAmmo -= amountToReload;
                    break;
                default:
                    break;
            }

            InventoryUI.Instance.RefreshUI();
            reloading = false;
        }
    }
    
}

[Serializable]
public class ItemDataSave
{
   
    public Sprite itemSprite;
    public ItemType itemType;
    public string itemName ;

    public bool hasDurability;
    public int damage ;
    public int durability;
    public float coolDownPeriod ;
    public float reloadTime;

    public float meleeRange;

    public AmmoType ammoType;
    public int magSize;
    public int loadedAmmo;

    public int amountToHeal;

    public void SaveFromItemData(ItemData itemData)
    {
        itemSprite = itemData.itemSprite;
        itemType = itemData.itemType;
        itemName = itemData.itemName;
        hasDurability = itemData.hasDurability;
        damage = itemData.damage;
        durability = itemData.durability;
        coolDownPeriod = itemData.coolDownPeriod;
        reloadTime = itemData.reloadTime;
        meleeRange = itemData.meleeRange;
        ammoType = itemData.ammoType;
        magSize = itemData.magSize;
        loadedAmmo = itemData.loadedAmmo;
        amountToHeal = itemData.amountToHeal;
    }

    public void LoadToItemData(ItemData itemData)
    {
        itemData.itemSprite = itemSprite;
        itemData.itemType = itemType;
        itemData.itemName = itemName;
        itemData.hasDurability = hasDurability;
        itemData.damage = damage;
        itemData.durability = durability;
        itemData.coolDownPeriod = coolDownPeriod;
        itemData.reloadTime= reloadTime;
        itemData.meleeRange = meleeRange;
        itemData.ammoType = ammoType;
        itemData.magSize = magSize;
        itemData.loadedAmmo = loadedAmmo;
        itemData.amountToHeal = amountToHeal;


    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    ItemData itemData;

    private void OnEnable()
    {
        itemData = (ItemData)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        //_ = DrawDefaultInspector();

        EditorGUI.BeginChangeCheck();

        itemData.itemSprite = (Sprite)EditorGUILayout.ObjectField(itemData.itemSprite, typeof(Sprite), false, GUILayout.Width(80), GUILayout.Height(80));

        itemData.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", itemData.itemType);

        itemData.itemName = EditorGUILayout.TextField("Item's Name",itemData.itemName);

        //if(!itemData.isRangedWeapon && !itemData.isHealingItem)
        //{
        //    itemData.isMeleeWeapon = EditorGUILayout.Toggle("Melee Weapon?", itemData.isMeleeWeapon);
        //    
        //}
        //
        //if (!itemData.isMeleeWeapon && !itemData.isHealingItem)
        //{
        //    itemData.isRangedWeapon = EditorGUILayout.Toggle("Ranged Weapon?", itemData.isRangedWeapon);
        //}
        //
        //if (!itemData.isRangedWeapon && !itemData.isMeleeWeapon)
        //{
        //    itemData.isHealingItem = EditorGUILayout.Toggle("Healing Item?", itemData.isHealingItem);
        //}

        if(itemData.itemType == ItemType.MeleeWeapon)
        {
            itemData.hasDurability = EditorGUILayout.Toggle("Durability?", itemData.hasDurability);
            if(itemData.hasDurability)
            {
                itemData.durability = EditorGUILayout.IntSlider("Durability Amount", itemData.durability, -1, 50);
            }
            else
            {
                itemData.durability = -1;
            }
            itemData.damage = EditorGUILayout.IntField("Damage to Deal", itemData.damage);
            itemData.coolDownPeriod = EditorGUILayout.FloatField("Attack Cooldown", itemData.coolDownPeriod);
            itemData.meleeRange = EditorGUILayout.FloatField("Melee Range", itemData.meleeRange);
        }

        if (itemData.itemType == ItemType.Pistol || itemData.itemType == ItemType.Rifle)
        {
            itemData.damage = EditorGUILayout.IntField("Damage to Deal", itemData.damage);
            itemData.coolDownPeriod = EditorGUILayout.FloatField("Shot Cooldown", itemData.coolDownPeriod);
            itemData.ammoType = (AmmoType)EditorGUILayout.EnumPopup("Ammo Type", itemData.ammoType);
            itemData.reloadTime = EditorGUILayout.FloatField("Reload Time(seconds)", itemData.reloadTime);
            itemData.magSize = EditorGUILayout.IntField("Magazine Size", itemData.magSize);
            
        }

        if(itemData.itemType == ItemType.Heal)
        {
            itemData.amountToHeal = EditorGUILayout.IntField("Amount to Heal", itemData.amountToHeal);
            
        }

        itemData.loadedAmmo = EditorGUILayout.IntField("Amount of Loaded Ammo", itemData.loadedAmmo);

        bool somethingChanged = EditorGUI.EndChangeCheck();

        if(somethingChanged)
        {
            EditorUtility.SetDirty(itemData);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
