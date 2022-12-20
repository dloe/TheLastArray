using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitter : BaseEnemy
{
    /// <summary>
    /// Splitter Enemy Behaviors
    /// Alex
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - inherited from BaseEnemy parent
    /// </summary>
    
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
