using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : BaseEnemy
{
    /// <summary>
    /// Dog Behaviors
    /// Alex
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - inherited from BaseEnemy parent
    /// </summary>
    public bool root = true;
    public GameObject sp1, sp2;
    public GameObject dog;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if(root)
        {
            GameObject dogclone1 = Instantiate(dog, sp1.transform.position, this.transform.rotation);
            dogclone1.transform.parent = this.transform.parent;
            dogclone1.name += "_CloneOf_" + this.transform.name;

            GameObject dogclone2 = Instantiate(dog, sp1.transform.position, this.transform.rotation);
            dogclone2.transform.parent = this.transform.parent;
            dogclone2.name += "_Clone2Of_" + this.transform.name;
        }
    }

   

}
