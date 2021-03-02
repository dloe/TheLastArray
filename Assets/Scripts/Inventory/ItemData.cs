using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


public enum AmmoType
{
    pistolAmmo,
    rifleAmmo
}
public enum ItemType
{
    MeleeWeapon,
    Pistol,
    Rifle,
    Heal,
    KeyItem
}
[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    


   

    public Sprite itemSprite;
    public ItemType itemType;
    public string itemName = "write item name here";

    public bool isMeleeWeapon = false;
    public bool hasDurability = false;
    public bool isRangedWeapon = false;
    public int damage = 1;
    public int durability = -1;
    public float coolDownPeriod = 0.5f;
    public float reloadTime = 1f;

    public float meleeRange = 1f;

    public AmmoType ammoType;
    public int magSize = 5;
    

    public bool isHealingItem = false;
    public int amountToHeal = 1;

    
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

        if(!itemData.isRangedWeapon && !itemData.isHealingItem)
        {
            itemData.isMeleeWeapon = EditorGUILayout.Toggle("Melee Weapon?", itemData.isMeleeWeapon);
            
        }

        if (!itemData.isMeleeWeapon && !itemData.isHealingItem)
        {
            itemData.isRangedWeapon = EditorGUILayout.Toggle("Ranged Weapon?", itemData.isRangedWeapon);
        }

        if (!itemData.isRangedWeapon && !itemData.isMeleeWeapon)
        {
            itemData.isHealingItem = EditorGUILayout.Toggle("Healing Item?", itemData.isHealingItem);
        }

        if(itemData.isMeleeWeapon)
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

        if (itemData.isRangedWeapon)
        {
            itemData.damage = EditorGUILayout.IntField("Damage to Deal", itemData.damage);
            itemData.coolDownPeriod = EditorGUILayout.FloatField("Shot Cooldown", itemData.coolDownPeriod);
            itemData.ammoType = (AmmoType)EditorGUILayout.EnumPopup("Ammo Type", itemData.ammoType);
            itemData.reloadTime = EditorGUILayout.FloatField("Reload Time(seconds)", itemData.reloadTime);
            itemData.magSize = EditorGUILayout.IntField("Magazine Size", itemData.magSize);
        }

        if(itemData.isHealingItem)
        {
            itemData.amountToHeal = EditorGUILayout.IntField("Amount to Heal", itemData.amountToHeal);
            
        }

        bool somethingChanged = EditorGUI.EndChangeCheck();

        if(somethingChanged)
        {
            EditorUtility.SetDirty(itemData);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
