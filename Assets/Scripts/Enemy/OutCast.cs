using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct Dropable
{
    public GameObject itemToDrop;
    public int bottomChance;
    public int topChance;
}

public class OutCast : BaseEnemy
{

    public Dropable[] dropableItem;



    void DropItem()
    {
        int i = UnityEngine.Random.Range(0, 100);
        //print(i);
        GameObject temp = null;
        for (int t = 0; t <= dropableItem.Length - 1; t++)
        {
            temp = returnItem(dropableItem[t], i);
            if (temp != null)
                break;
        }
        if (temp != null)
        {
            Instantiate(temp, this.gameObject.transform.position, temp.transform.rotation);
            
        }
    }

    GameObject returnItem(Dropable temp,int i)
    {
        GameObject drop = null;
        for(int e = temp.bottomChance; e <= temp.topChance; e++)
        {
            
            if (i == e)
            { 
                return temp.itemToDrop;
            }
        }
        return drop;
    }

    public override void OnDeath()
    {
        DropItem();
        base.OnDeath();
    }
}
