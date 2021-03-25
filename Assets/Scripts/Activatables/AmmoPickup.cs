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
        RefreshText();
    }

    public override void Activate()
    {
        
        switch (ammoType)
        {
            case AmmoType.LightAmmo:
                if(Player.Instance.currentLightAmmo < Player.Instance.maxLightAmmo)
                {
                    if(Player.Instance.maxLightAmmo - Player.Instance.currentLightAmmo >= amountToAdd)
                    {
                        Player.Instance.currentLightAmmo += amountToAdd;
                        Destroy(gameObject);
                    }
                    else
                    {
                        amountToAdd -= Player.Instance.maxLightAmmo - Player.Instance.currentLightAmmo;
                        Player.Instance.currentLightAmmo += Player.Instance.maxLightAmmo - Player.Instance.currentLightAmmo; 
                    }
                    RefreshText();
                }
                else
                {
                    Debug.Log("Light Ammo is Full");
                }
                
                break;
            case AmmoType.HeavyAmmo:
                if (Player.Instance.currentHeavyAmmo < Player.Instance.maxHeavyAmmo)
                {
                    if (Player.Instance.maxHeavyAmmo - Player.Instance.currentHeavyAmmo >= amountToAdd)
                    {
                        Player.Instance.currentHeavyAmmo += amountToAdd;
                        Destroy(gameObject);
                    }
                    else
                    {
                        amountToAdd -= Player.Instance.maxHeavyAmmo - Player.Instance.currentHeavyAmmo;
                        Player.Instance.currentHeavyAmmo += Player.Instance.maxHeavyAmmo - Player.Instance.currentHeavyAmmo;
                    }
                    RefreshText();
                }
                else
                {
                    Debug.Log("Heavy Ammo is Full");
                }
                break;
            default:
                break;
        }

        InventoryUI.Instance.RefreshUI();

        
    }

    public void RefreshText()
    {
        switch (ammoType)
        {
            case AmmoType.LightAmmo:
                displayText.text = "Light Ammo (" + amountToAdd + ")";
                break;
            case AmmoType.HeavyAmmo:
                displayText.text = "Heavy Ammo (" + amountToAdd + ")";
                break;
            default:
                break;
        }
    }
}
