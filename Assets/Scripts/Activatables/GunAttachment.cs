using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttachType
{
    laser,
    tunedBarrel
}

public class GunAttachment : Activatable
{
    public AttachType attachType;

    public override void Activate()
    {
        if(Player.Instance.inventory.selectedItem != null && Player.Instance.inventory.selectedItem.itemData.itemType == ItemType.RangedWeapon)
        {
            switch (attachType)
            {
                case AttachType.laser:
                    if(!Player.Instance.inventory.selectedItem.itemData.hasLaserSight)
                    {
                        Player.Instance.inventory.selectedItem.itemData.hasLaserSight = true;
                        Player.Instance.laserLine.gameObject.SetActive(true);
                        Destroy(gameObject);
                    }
                    break;
                case AttachType.tunedBarrel:
                    break;
                default:
                    break;
            }
        }
    }
}
