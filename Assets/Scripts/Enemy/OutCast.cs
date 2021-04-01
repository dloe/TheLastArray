using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutCast : BaseEnemy
{
    [Header ("Outcast items to drop")]
    public GameObject[] itemsToDrop;



    void DropItem()
    {
        int i = Random.Range(0, itemsToDrop.Length - 1);
        print(itemsToDrop[i].name);
        GameObject mine = Instantiate(itemsToDrop[i], this.gameObject.transform.position, itemsToDrop[i].transform.rotation);
        mine.transform.rotation.y.Equals(0);
    }

    public override void OnDeath()
    {
        DropItem();
        base.OnDeath();
    }
}
