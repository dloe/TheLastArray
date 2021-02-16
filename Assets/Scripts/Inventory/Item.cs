using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public enum ItemType
    {
        MeleeWeapon,
        Pistol,
        Rifle,
        MedKit
    }

    public ItemType itemType;

    public int weaponAmmo = 0;
    public int weaponReserves = 0;

}