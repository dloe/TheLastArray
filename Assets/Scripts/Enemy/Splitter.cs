using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitter : BaseEnemy
{
    [Header("spliter shit")]
    public GameObject Splited;
    public GameObject loc1, loc2;
    public override void OnDeath()
    {
        Split();
        base.OnDeath();
    }

    void Split()
    {
        Instantiate(Splited, loc1.transform.position, this.transform.rotation);
        Instantiate(Splited, loc2.transform.position, this.transform.rotation);
    }

}
