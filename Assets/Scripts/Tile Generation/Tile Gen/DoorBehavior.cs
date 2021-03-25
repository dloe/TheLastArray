using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    //when activated, it will not spawn any blockage possibly pick different variants of doors or entryways
    //otherwise player cannot go through this way so it will spawn walls or something to stop them from progressing
    public bool isDoor = true;
    public int num;
    public bool replaceme = false;

    public bool notInUse = false;
    public void ActivateDoor(bool isActive)
    {
        
        //Debug.Log("activated door");
        isDoor = isActive;
        
        if (isActive)
        {
            //if another door is hitting this door and active, this door deletes itself
            CheckForReplacementDoor();

            //activate other stuff after door is for sure staying
        }
        else
        {
            notInUse = true;
           // Debug.Log("Destroyed door " + this.name);
            Destroy(this.gameObject);
        }
    }

    //if an active door already exists in this spot, destroy this door
    public void CheckForReplacementDoor()
    {
        Debug.Log(this.name + " Checking for replacements");
        Transform[] _possibleDoors = collidersToTransforms(Physics.OverlapSphere(transform.position, 2.5f));
        foreach (Transform potentialTarget in _possibleDoors)
        {
            //Debug.Log(this.name + "---pt: " + potentialTarget.name);
            if (potentialTarget.gameObject.tag == "Door")
            {
                //replace this door in the tile with the door that already exists
                //WIP
                notInUse = true;
                Debug.Log(this.name + " Destroyed door because of preexisting door " + potentialTarget.gameObject.name);
                Destroy(this.gameObject);
            }
        }
    }

    private void OnDrawGizmos()
    {
     //   Gizmos.color = Color.red;
       // Gizmos.DrawSphere(transform.position, 5);
    }

    //locate transforms from colliders found in sphere
    private Transform[] collidersToTransforms(Collider[] colliders)
    {
        Transform[] transforms = new Transform[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            transforms[i] = colliders[i].transform;
        }
        return transforms;
    }
}
