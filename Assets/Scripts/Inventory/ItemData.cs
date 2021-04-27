using System;
using System.Collections;
using System.IO;
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
    RangedWeapon,
    Heal,
    Key,
    Binoculars,
    BackPack,
    finalObjective,
    UnstableStim
}
[Serializable][CreateAssetMenu]
public class ItemData : ScriptableObject
{





    public Sprite itemSprite;
    public ItemType itemType;
    public string itemName = "write item name here";
    public string itemDescription = "write desc here";

    public bool hasDurability = false;
    public int damage = 1;
    public int durability = -1;
    public float coolDownPeriod = 0.5f;
    public float reloadTime = 1f;

    public float meleeRange = 1f;

    public AmmoType ammoType;
    public int magSize = 5;
    public int loadedAmmo;

    private int _FireAmmo;
    public int fireLoadedAmmo
    {
        get => _FireAmmo;
        set
        {
            _FireAmmo = value;
            if(_FireAmmo == 0)
            {
                
                InventoryUI.Instance.StartCoroutine(FireToNormal());
            }
        }
        
    }


    public int amountToHeal = 1;
    public int healthDecrease = 5;
    public int damageModifier = 5;

    public bool canAttack = true;
    public bool reloading = false;
    public bool hasLaserSight = false;
    public bool usingFireBullets = false;


    public IEnumerator CoolDown()
    {
        canAttack = false;
        yield return new WaitForSeconds(coolDownPeriod);
        canAttack = true;
    }

    public IEnumerator Reload(int amountToReload)
    {
        if(itemType != ItemType.RangedWeapon)
        {
            yield return new WaitForEndOfFrame();
            Debug.LogError("This Function cannot be called unless the item is a ranged weapon, refrain from doing so");
        }
        else
        {
            reloading = true;

            Debug.Log("Reloading...");
            for(int i = (int)reloadTime; i > 0; i--)
            {
                InventoryUI.Instance.currentAmmoName.text = "Reloading... " + i;
                yield return new WaitForSeconds(1);
            }

            
            //yield return new WaitForSeconds(reloadTime);
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

    public IEnumerator FireToNormal()
    {
        if (itemType != ItemType.RangedWeapon)
        {
            yield return new WaitForEndOfFrame();
            Debug.LogError("This Function cannot be called unless the item is a ranged weapon, refrain from doing so");
        }
        else
        {
            reloading = true;

            Debug.Log("Reloading...");
            for (int i = (int)reloadTime; i > 0; i--)
            {
                InventoryUI.Instance.currentAmmoName.text = "Reloading... " + i;
                yield return new WaitForSeconds(1);
            }
            Debug.Log("Reloaded");


            usingFireBullets = false;
            InventoryUI.Instance.RefreshUI();
            reloading = false;
        }
    }

    public void LoadFireBullets()
    {
        usingFireBullets = true;
        fireLoadedAmmo = magSize;
        InventoryUI.Instance.RefreshUI();
    }

}

[Serializable]
public class ItemDataSave
{
    public string itemSpritePath;
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

    public bool hasLaserSight;

    public void SaveFromItemData(ItemData itemData)
    {
        //Debug.Log(itemData.itemSprite.name);
        itemSpritePath = "ItemSprites/" + itemData.itemSprite.name;
        //Debug.Log(itemSpritePath) ;
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
        hasLaserSight = itemData.hasLaserSight;
        //Debug.Log(loadedAmmo + "item save ammo");
        //Debug.Log(itemData.loadedAmmo + "item data ammo");
    }

    public void LoadToItemData(ItemData itemData)
    {
        itemData.itemSprite = Resources.Load<Sprite>(itemSpritePath);
        //Debug.Log(itemSpritePath);
        //Debug.Log(itemData.itemSprite, itemData.itemSprite);
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
        itemData.hasLaserSight = hasLaserSight;

        //Debug.Log(itemData.loadedAmmo, itemData);
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

        EditorGUI.BeginChangeCheck();

        EditorStyles.textField.wordWrap = true;

        itemData.itemSprite = (Sprite)EditorGUILayout.ObjectField(itemData.itemSprite, typeof(Sprite), false, GUILayout.Width(80), GUILayout.Height(80));

        itemData.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", itemData.itemType);

        itemData.itemName = EditorGUILayout.TextField("Item's Name",itemData.itemName);

        EditorGUILayout.PrefixLabel("Item Description");
        itemData.itemDescription = EditorGUILayout.TextArea(itemData.itemDescription, GUILayout.MaxHeight(80));

        if (itemData.itemType == ItemType.MeleeWeapon)
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

        if (itemData.itemType == ItemType.RangedWeapon)
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

        if(itemData.itemType == ItemType.UnstableStim)
        {
            itemData.healthDecrease = EditorGUILayout.IntField("Amount to Decrease Health", itemData.healthDecrease);
            itemData.damageModifier = EditorGUILayout.IntField("Amount to Modify Damage", itemData.damageModifier);
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
