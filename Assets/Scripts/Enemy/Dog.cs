using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dog : BaseEnemy
{
    public bool root = true;
    public GameObject sp1, sp2;
    public GameObject dog;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        if(root)
        {
            Instantiate(dog, sp1.transform.position, this.transform.rotation);
            
            Instantiate(dog, sp1.transform.position, this.transform.rotation);
            
        }
    }

   

}
