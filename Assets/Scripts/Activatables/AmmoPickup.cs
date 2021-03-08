using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoPickup : Activatable
{
    public AmmoType ammoType;
    public int amountToAdd;
    public Text displayText;

    private void Start()
    {
        ammoType = (AmmoType)Random.Range(0, 2);
        amountToAdd = Random.Range(1, 6);
        switch (ammoType)
        {
            case AmmoType.LightAmmo:
                displayText.text = "Light Ammo (" + amountToAdd + ")" ;
                break;
            case AmmoType.HeavyAmmo:
                displayText.text = "Heavy Ammo (" + amountToAdd + ")";
                break;
            default:
                break;
        }
    }

    public override void Activate()
    {
        switch (ammoType)
        {
            case AmmoType.LightAmmo:
                Player.Instance.currentLightAmmo += amountToAdd;
                break;
            case AmmoType.HeavyAmmo:
                Player.Instance.currentHeavyAmmo += amountToAdd;
                break;
            default:
                break;
        }

        InventoryUI.Instance.RefreshUI();

        Destroy(gameObject);
    }
}
