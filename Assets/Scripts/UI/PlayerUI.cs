using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [Header("Player Health")]
    public Text healthValue;
    public Text[] resourceValueText = new Text[3];
    [Space(10)]
    [Header("Player Weapon")]
    public Text weaponReservesText;
    public Text weaponMagText;
    //public Image weaponIcon;
    public Text weaponName;

    public void UpdateHealthUI()
    {

    }

    public void UpdateWeaponAmmoUI()
    {

    }

    //switching to slot in inventory
    public void SwitchWeaponsUI(int slot)
    {

    }
}
